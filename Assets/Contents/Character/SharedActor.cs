using UnityEngine;
using FishNet.Object;
using Core.EventBus;
using Core.EventBus.Event;
using System.Collections.Generic;
using Core.Network;
using FishNet.Component.Transforming;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkTransform))]
public class SharedActor : BaseNetworkActor<CharacterType>, IFixedTickable
{
    public override CharacterType GroupType => CharacterType.CapsuleMan;


    private Rigidbody _rigidbody;
    public float moveSpeed = 50f;

    // 💡 4명의 입력을 모아둘 장바구니 (Key: 클라이언트ID, Value: 입력 벡터)
    private Dictionary<int, Vector2> _clientInputs = new Dictionary<int, Vector2>();


    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    // =========================================================
    // 🌐 SERVER 영역 (호스트/서버에서만 실행되는 생명주기)
    // =========================================================

    public override void OnStartServer()
    {
        ServerManager.Spawn(gameObject);
        // 서버가 켜질 때, 허공에 떠도는 '이동 이벤트'를 구독합니다.
        SubscribeTo<SharedActorMoveEvent>(OnSharedActorMove);
        UpdateManager.UPDATE_Physics -= FixedTick;
        UpdateManager.UPDATE_Physics += FixedTick;
    }

    public override void OnStopServer()
    {
        // 서버가 꺼질 때 안전하게 구독을 해제합니다.
        UnsubscribeAll();
        UpdateManager.UPDATE_Physics -= FixedTick;
    }

    // 클라이언트들이 ServerRpc로 쏜 패킷이 EventBus를 타고 여기로 들어옵니다.
    private void OnSharedActorMove(SharedActorMoveEvent evt)
    {
        // 누가(evt.ClientId) 어느 방향(evt.MoveVector)으로 밀었는지 캐싱(덮어쓰기)합니다.
        // ex) 1번 유저가 W 누르면 (0,1), 2번 유저가 S 누르면 (0,-1)
        _clientInputs[evt.ClientId] = evt.MoveVector;
    }

    // =========================================================
    // ⚙️ 물리 연산 영역
    // =========================================================

    public void FixedTick(float fixedDeltaTime)
    {
        // 💡 핵심: 물리 연산은 "절대로" 클라이언트에서 실행되면 안 됩니다.
        if (!IsServerInitialized) return;

        Vector2 combinedInput = Vector2.zero;

        // 장바구니에 담긴 4명의 입력을 모두 더합니다. (벡터의 합산)
        foreach (var input in _clientInputs.Values)
        {
            combinedInput += input;
        }

        // 누군가 밀고 있다면 힘을 가합니다.
        if (combinedInput != Vector2.zero)
        {
            combinedInput = combinedInput.normalized; // 크기 1로 정규화

            // 2D 입력(Vector2)을 3D 월드의 XZ 평면 힘(Vector3)으로 변환
            Vector3 movement = new Vector3(combinedInput.x, 0, combinedInput.y) * moveSpeed;
            
            _rigidbody.AddForce(movement, ForceMode.Force);
            // NetworkTransform이 객체 이동을 자동 판단후 클라이언트에 적용시킴
        }
    }
}