using Core;
using Core.Camera;
using Core.Director;
using Core.EventBus;
using Core.Manager;
using Core.Network;
using FishNet.Component.Transforming;
using FishNet.Managing.Server;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkTransform))]
public class SharedActor : BaseActorNetworked, IFixedTickable
{
    public FixedTickGroup FixedTickGroup => FixedTickGroup.Physics;

    protected override NetworkTickTarget networkTickTarget => NetworkTickTarget.ServerOnly;

    private Rigidbody _rigidbody;
    public float moveSpeed = 50f;
    public float rotationSpeed = 1.0f;

    [Header("🪽 날갯짓 물리 세팅")]
    public float flapForce = 8f;   // 위로 솟구치는 순간적인 힘 (Impulse)
    public float flapTorque = 5f;  // 순간적인 회전력 (Impulse)

    [Header("⚖️ 균형(오뚝이) 보정 세팅")]
    public float pGain = 20f;  // 복원력 (얼마나 강력하게 돌아올 것인가)
    public float dGain = 5f;   // 제동력 (얼마나 빠르게 멈출 것인가)
    public float timeDelay = 1f;
    private float timeCount;

    
    private readonly SyncDictionary<int, Vector2> _clientInputs = new SyncDictionary<int, Vector2>();
    public SyncDictionary<int, Vector2> ClientInputs => _clientInputs;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        // 카메라한테 내가 등장했음을 알림
        // "내 캐릭터(Transform)를 비춰줘! 단, ThirdPersonCamera만 타겟을 바꿔!"
        CameraTargetProvider targetProvider = GetComponentInChildren<CameraTargetProvider>();
        EventBus<SetCameraTargetEvent>.Publish(new SetCameraTargetEvent(targetProvider.Target, typeof(ThirdPersonCameraController)));

        // "3인칭 카메라로 시점을 바꿔줘!" (자연스러운 Blending 발생)
        EventBus<SwitchCameraEvent>.Publish(new SwitchCameraEvent(typeof(ThirdPersonCameraController)));
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        // 💡 핵심: 내가 방장(호스트/서버)이 아닌 순수 클라이언트라면?
        if (!base.IsServerInitialized)
        {
            // 클라이언트 쪽의 물리 엔진(중력, 관성 등)을 완전히 꺼버립니다.
            // 이제 클라이언트는 제멋대로 움직이지 않고, 오직 서버가 보내주는 위치로만 이동합니다!
            _rigidbody.isKinematic = true;
        }
    }

    public override void OnStartServer()
    {
        ServerManager.Spawn(gameObject);

        // 이동 및 날갯짓 이벤트 모두 구독
        // 주: 원래 OnStartServer에서 구독을 등록했습니다.
        EventBus<SharedActorMoveEvent>.Subscribe(OnSharedActorMove);
        EventBus<SharedActorFlapEvent>.Subscribe(OnSharedActorWingFlap);
    }

    public override void OnStopServer()
    {
        // 수동으로 등록한 이벤트 해제
        EventBus<SharedActorMoveEvent>.Unsubscribe(OnSharedActorMove);
        EventBus<SharedActorFlapEvent>.Unsubscribe(OnSharedActorWingFlap);
    }

    private void OnSharedActorMove(SharedActorMoveEvent evt)
    {
        _clientInputs[evt.ClientId] = evt.MoveVector;
    }

    // 날갯짓 단발성 이벤트 처리 함수
    private void OnSharedActorWingFlap(SharedActorFlapEvent evt)
    {
        if (!IsServerInitialized) return;
        
        // 위로 향하는 순간적인 힘 가하기
        _rigidbody.AddForce(transform.up * flapForce, ForceMode.Impulse);

        float torqueDirection = evt.IsLeft ? -1f : 1f;
        Vector3 appliedTorque = Vector3.forward * torqueDirection * flapTorque;

        // 회전력 적용
        _rigidbody.AddTorque(appliedTorque, ForceMode.Impulse);

        timeCount = 0;
    }

    public void FixedTick(float fixedDeltaTime)
    {
        if (!IsServerInitialized) return;
        
        Move(fixedDeltaTime);

        StabilizeRotation(fixedDeltaTime);

    }

    public void Move(float fixedDeltaTime)
    {
        Vector2 combinedInput = Vector2.zero;
        foreach (var input in _clientInputs.Values)
        {
            combinedInput += input;
        }

        if (combinedInput != Vector2.zero)
        {
            // 이동하는 방향 바라보고
            Vector3 moveDir = new Vector3(combinedInput.x, 0, combinedInput.y).normalized;
            Utility.SmoothLookAt(_rigidbody, moveDir, rotationSpeed, fixedDeltaTime);

            // 출발
            _rigidbody.AddForce(moveDir * moveSpeed, ForceMode.Force); // 지속적인 이동은 Force 모드
        }
    }

    

    private void StabilizeRotation(float fixedDeltaTime)
    {
        if(timeCount <= timeDelay)
        {
            timeCount += fixedDeltaTime;
            return;
        }
        

        // 1. 회전 오차 계산 (현재 캐릭터 up vs 월드 up)
        Vector3 error = Vector3.Cross(transform.up, Vector3.up);

        // 2. PD 제어 로직
        // (오차 * P값) - (현재 각속도 * D값)
        // - 오차가 클수록 강력하게 회전
        // - 회전 속도가 빠를수록 반대 방향으로 브레이크

        // 중력값이 상관없이 일정한 속도로 처리하도록
        Vector3 torque = (error * pGain) - (_rigidbody.angularVelocity * dGain);

        _rigidbody.AddTorque(torque, ForceMode.Force);
    }
}