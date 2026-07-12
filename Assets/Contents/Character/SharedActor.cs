using Core.Network;
using FishNet.Component.Transforming;
using FishNet.Managing.Server;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkTransform))]
public class SharedActor : BaseNetworkActor<CharacterType>, IFixedTickable
{
    public override CharacterType GroupType => CharacterType.CapsuleMan;

    private Rigidbody _rigidbody;
    public float moveSpeed = 50f;

    [Header("🪽 날갯짓 물리 세팅")]
    public float flapForce = 8f;   // 위로 솟구치는 순간적인 힘 (Impulse)
    public float flapTorque = 5f;  // 순간적인 회전력 (Impulse)

    private Dictionary<int, Vector2> _clientInputs = new Dictionary<int, Vector2>();

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public override void OnStartServer()
    {
        ServerManager.Spawn(gameObject);

        // 이동 및 날갯짓 이벤트 모두 구독
        SubscribeTo<SharedActorMoveEvent>(OnSharedActorMove);
        SubscribeTo<SharedActorFlapEvent>(OnSharedActorFlap);

        UpdateManager.UPDATE_Physics -= FixedTick;
        UpdateManager.UPDATE_Physics += FixedTick;
    }

    public override void OnStopServer()
    {
        UnsubscribeAll();
        UpdateManager.UPDATE_Physics -= FixedTick;
    }

    private void OnSharedActorMove(SharedActorMoveEvent evt)
    {
        _clientInputs[evt.ClientId] = evt.MoveVector;
    }

    // ✨ 날갯짓 단발성 이벤트 처리 함수
    private void OnSharedActorFlap(SharedActorFlapEvent evt)
    {
        if (!IsServerInitialized) return;

        // 1. 🚀 위로 향하는 순간적인 힘 가하기 (Impulse 모드는 질량을 고려해 툭 쳐줍니다)
        _rigidbody.AddForce(Vector3.up * flapForce, ForceMode.Impulse);

        // 2. 🔄 짝수/홀수 ClientId에 따른 토크(회전력) 방향 계산
        // 💡 Unity 3D 좌표계 규칙 (Y축 기준)
        // (+) 값 = 위에서 내려다봤을 때 '반시계 방향' 회전 (Counter-Clockwise) -> 홀수
        // (-) 값 = 위에서 내려다봤을 때 '시계 방향' 회전 (Clockwise) -> 짝수

        float torqueDirection = (evt.ClientId % 2 == 0) ? -1f : 1f;
        Vector3 appliedTorque = Vector3.forward * torqueDirection * flapTorque;

        // 3. 회전력 적용 (Impulse)
        _rigidbody.AddTorque(appliedTorque, ForceMode.Impulse);
    }

    public void FixedTick(float fixedDeltaTime)
    {
        if (!IsServerInitialized) return;

        Vector2 combinedInput = Vector2.zero;
        foreach (var input in _clientInputs.Values)
        {
            combinedInput += input;
        }

        if (combinedInput != Vector2.zero)
        {
            combinedInput = combinedInput.normalized;
            Vector3 movement = new Vector3(combinedInput.x, 0, combinedInput.y) * moveSpeed;
            _rigidbody.AddForce(movement, ForceMode.Force); // 지속적인 이동은 Force 모드
        }
    }
}