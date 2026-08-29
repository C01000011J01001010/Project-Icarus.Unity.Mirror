using CoreEngine;
using CoreEngine.EventBus;
using CoreEngine.Interface;
using CoreEngine.Network;
using CoreEngine.Helpers;
using CoreEngine.Facades;
using FishNet.Object; // FishNet 필수 네임스페이스
using Icarus.Camera;
using UnityEngine;


// 서버 전용: 클라이언트의 입력을 SharedActor에게 전달하는 이벤트
public struct SharedActorMoveEvent : IEvent
{
    public int ClientId;      // 누가 밀었는가? (4명 구분용)
    public Vector2 MoveVector; // 어느 방향으로 밀었는가?

    public SharedActorMoveEvent(int clientId, Vector2 moveVector)
    {
        ClientId = clientId;
        MoveVector = moveVector;
    }
}

public struct SharedActorFlapEvent : IEvent
{
    public int ClientId; // 누가 날개를 펄럭였는가?
    public bool IsLeft; // 어느쪽 날개인가

    public SharedActorFlapEvent(int clientId, bool isLeft)
    {
        ClientId = clientId;
        IsLeft = isLeft;
    }
}

public enum MovementDirectionMode
{
    // w방향이 z+방향 고정
    WorldFixed, 

    // 카메라y축 회전량에 따라 w방향의 이동방향 정함
    // 한번 결정되면 다른 이동입력이 변화가 있기 전까지 카메라를 회전해도 고정
    CameraRelativeStatic,

    // 카메라y축 회전량에 따라 w방향의 이동방향 정함
    // 이후 입력을 바꾸지 않아도 화면 회전량에 따라 이동방향 변경
    CameraRelativeDynamic,
}

public class PlayerInputSender : BaseActorNetworked, ITickable//, IControllerSetter
{
    private IPlayerInputProvider _inputProvider; // TODO: InterfaceReceiver로 수정해야할 속성
    private InterfaceReceiver<ICameraRotationProvider> _cameraReceiver = new();

    // AutoBinder 이름을 InterfaceBinderContainer로 수정
    private InterfaceBinderContainer _binderContainer = new(); // 2개 이상을 묶어야하니 컨테이너 사용

    protected override NetworkTickTarget networkTickTarget => NetworkTickTarget.OwnerOnly;
    public TickGroup TickGroup => TickGroup.Controller;

    // 캐싱 변수 분리
    private Vector2 _lastRawMove;  // 순수 키보드(WASD) 입력 캐싱용
    private Vector2 _lastSentMove; // 서버로 보낸 최종 결과값 캐싱용

    public MovementDirectionMode _movementDirectionMode;

    //public override void Awake()
    //{
    //    base.Awake();
    //    _binderContainer.Add(_cameraReceiver);
    //}

    private void Start()
    {
        _binderContainer.Add(_cameraReceiver);
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        if (!IsOwner) return;

        EventBus<OnSpaceBarWingFlappedEvent>.Unsubscribe(OnLocalSpaceBarFlapPerformed);
        EventBus<OnMouseClickWingFlappedEvent>.Unsubscribe(OnLocalMouseFlapPerformed);
        _binderContainer.UnbindAll();
    }
    public override void OnStartClient()
    {
        base.OnStartClient();

        if(!IsOwner) return;

        _inputProvider = CoreFacade.GetManager<UserInputManager>();
        if (SystemHelper.isUnityNull(_inputProvider)) return;

        EventBus<OnSpaceBarWingFlappedEvent>.Subscribe(OnLocalSpaceBarFlapPerformed);
        EventBus<OnMouseClickWingFlappedEvent>.Subscribe(OnLocalMouseFlapPerformed);
        _binderContainer.BindAll();
    }

    public void Tick(float deltaTime)
    {
        if(!SystemHelper.isUnityNull(_inputProvider))
            InputMove();
    }

    private void InputMove()
    {
        Vector2 rawMove = _inputProvider.Move;
        Vector2 finalMove = rawMove;

        switch (_movementDirectionMode)
        {
            case MovementDirectionMode.WorldFixed:
                // 순수 입력이 이전과 같으면 스킵
                if (rawMove == _lastRawMove) return;
                break;

            case MovementDirectionMode.CameraRelativeStatic:
                // 순수 입력이 이전과 같으면 스킵 (방향키를 새로 눌렀을 때만 기준 갱신)
                if (rawMove == _lastRawMove) return;
                finalMove = GetCameraRelativeDirection(rawMove);
                break;

            case MovementDirectionMode.CameraRelativeDynamic:
                // 순수 입력이 0,0인데 이전 순수 입력도 0,0이면 계산할 필요 없이 스킵 (마우스 회전 무시)
                if (rawMove == Vector2.zero && _lastRawMove == Vector2.zero) return;
                finalMove = GetCameraRelativeDirection(rawMove);
                break;
        }

        // 플레이어 raw입력 캐싱
        _lastRawMove = rawMove;

        // 최종 전송 방어막:
        // 연산된 결과값이 이전 전송값과 완전히 동일하다면 보내지 않음
        if (finalMove == _lastSentMove) return;

        // 최종 서버에 보낼 최종입력값 캐싱
        _lastSentMove = finalMove;
        ServerRpcSendMove(finalMove); // 명명규칙 통일
    }

    private Vector2 GetCameraRelativeDirection(Vector2 worldFixedMove)
    {
        Vector2 relateiveMove = worldFixedMove;
        if (_cameraReceiver.Target != null)
        {
            // 카메라의 현재 월드 Y 각도 가져오기
            float camY = _cameraReceiver.Target.WorldYRotation;

            // Vector2(X, Y) 입력을 3D 평면(X, 0, Z)으로 변환
            Vector3 input3D = new Vector3(worldFixedMove.x, 0, worldFixedMove.y);

            // 카메라의 Y축 각도만큼 입력 벡터를 회전
            Vector3 rotated3D = Quaternion.Euler(0, camY, 0) * input3D;

            // 다시 서버 전송용 Vector2로 압축
            relateiveMove = new Vector2(rotated3D.x, rotated3D.z);
        }
        return relateiveMove;
    }

    private void OnLocalSpaceBarFlapPerformed(OnSpaceBarWingFlappedEvent evt)
    {
        ServerRpcSpaceBarFlap();
    }

    private void OnLocalMouseFlapPerformed(OnMouseClickWingFlappedEvent evt)
    {
        ServerRpcMouseFlap(evt.isLeft);
    }

    // =========================================================
    // 🌐 SERVER 영역 (호스트/서버에서만 실행되는 함수)
    // =========================================================

    [ServerRpc]
    private void ServerRpcSendMove(Vector2 moveInput)
    {
        // 💡 핵심 4: 클라이언트가 보낸 입력을 서버 공간에 도착하자마자 EventBus로 방송합니다!
        // base.OwnerId는 FishNet이 보장하는 클라이언트 고유 식별 번호입니다.
        EventBus<SharedActorMoveEvent>.Publish(new SharedActorMoveEvent(OwnerId, moveInput));
    }

    [ServerRpc]
    private void ServerRpcSpaceBarFlap()
    {
        // 0번부터 좌우 순서
        bool isLeft = OwnerId % 2 == 0;
        EventBus<SharedActorFlapEvent>.Publish(new SharedActorFlapEvent(OwnerId, isLeft));
    }

    [ServerRpc]
    private void ServerRpcMouseFlap(bool isLeft)
    {
        EventBus<SharedActorFlapEvent>.Publish(new SharedActorFlapEvent(OwnerId, isLeft));
    }
    
}