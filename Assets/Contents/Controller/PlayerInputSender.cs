using UnityEngine;
using FishNet.Object; // FishNet 필수 네임스페이스
using Core.Interface;
using Core.EventBus;
using Core.EventBus.Event;
using Core.Network;
using Core.Manager;


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

    public SharedActorFlapEvent(int clientId)
    {
        ClientId = clientId;
    }
}

public class PlayerInputSender : BaseActorNetworked<ControllerType>, IControllerSetter, ITickable
{
    private IPlayerInputProvider _inputProvider;

    public override ControllerType GroupType => ControllerType.InputSender;

    protected override NetworkTickTarget networkTickTarget => NetworkTickTarget.OwnerOnly;
    public TickGroup TickGroup => TickGroup.Controller;


    private Vector2 _lastSentMove;

    public override void OnStopClient()
    {
        base.OnStopClient();
        if (IsOwner && _inputProvider != null)
        {
            //EventBus<R_TickEvent>.Publish(new R_TickEvent(this, TickGroup.Controller, false));
            EventBus<OnWingFlappedEvent>.Unsubscribe(OnLocalFlapPerformed);
        }
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        // 💡 핵심 1: 이 껍데기가 '내 것'일 때만 입력 매니저를 연결합니다.
        // 다른 유저의 껍데기가 내 키보드 입력을 빼앗아가는 것을 방지합니다.
        if (IsOwner)
        {
            EventBus<ControllerSettingEvent>.Publish(new ControllerSettingEvent(this));

            //if(_inputProvider != null)
            //{
            //    EventBus<R_TickEvent>.Publish(new R_TickEvent(this, TickGroup.Controller, true));
            //}
            EventBus<OnWingFlappedEvent>.Subscribe(OnLocalFlapPerformed);
        }
    }

    public void Tick(float deltaTime)
    {
        if(_inputProvider != null)
            InputMove();
    }

    private void InputMove()
    {
        Vector2 currentMove = _inputProvider.Move;

        // 💡 수정됨: 값이 0일 때 무시하는 것이 아니라, "값이 이전과 다를 때만" 전송합니다.
        // 이렇게 하면 멈추는 순간(zero)은 전송되지만, 가만히 서 있을 때는 네트워크를 낭비하지 않습니다.
        if (currentMove == _lastSentMove) return;

        _lastSentMove = currentMove;
        ServerCmdSendMove(currentMove);
    }

    private void OnLocalFlapPerformed(OnWingFlappedEvent evt)
    {
        ServerRpcFlap();
    }

    // =========================================================
    // 🌐 SERVER 영역 (호스트/서버에서만 실행되는 함수)
    // =========================================================

    [ServerRpc]
    private void ServerCmdSendMove(Vector2 moveInput)
    {
        // 💡 핵심 4: 클라이언트가 보낸 입력을 서버 공간에 도착하자마자 EventBus로 방송합니다!
        // base.OwnerId는 FishNet이 보장하는 클라이언트 고유 식별 번호입니다.
        EventBus<SharedActorMoveEvent>.Publish(new SharedActorMoveEvent(OwnerId, moveInput));
    }

    [ServerRpc]
    private void ServerRpcFlap()
    {
        // 서버: "오케이, X번 클라이언트가 날갯짓을 했군."
        EventBus<SharedActorFlapEvent>.Publish(new SharedActorFlapEvent(OwnerId));
    }

    public void SetInputProvider(IPlayerInputProvider inputProvider)
    {
        if(inputProvider != null)
        {
            _inputProvider = inputProvider;
        }
    }

    
}