using UnityEngine;
using FishNet.Object;
using Core.EventBus;
using Core.EventBus.Event;
using System.Collections.Generic;
using Core.Network;

[RequireComponent(typeof(Rigidbody))]
public class SharedActor : BaseNetworkActor<CharacterType>
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
        base.OnStartServer();
        // 서버가 켜질 때, 허공에 떠도는 '이동 이벤트'를 구독합니다.
        SubscribeTo<SharedActorMoveEvent>(OnSharedActorMove);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        // 서버가 꺼질 때 안전하게 구독을 해제합니다.
        UnsubscribeAll();
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

    private void FixedUpdate()
    {
        // 💡 핵심: 물리 연산은 "절대로" 클라이언트에서 실행되면 안 됩니다.
        if (!base.IsServerInitialized) return;

        Vector2 combinedInput = Vector2.zero;

        // 장바구니에 담긴 4명의 입력을 모두 더합니다. (벡터의 합산)
        foreach (var input in _clientInputs.Values)
        {
            combinedInput += input;
        }

        // 누군가 밀고 있다면 힘을 가합니다.
        if (combinedInput != Vector2.zero)
        {
            // 4명이 한 방향으로 밀면 4배 빨라지게 할지, 최대 속도를 제한할지는 기획에 따라 결정
            // (여기서는 단순 합산으로 4명이 합심하면 엄청 빨라지는 구조입니다)

            // 2D 입력(Vector2)을 3D 월드의 XZ 평면 힘(Vector3)으로 변환
            Vector3 force = new Vector3(combinedInput.x, 0, combinedInput.y) * moveSpeed;

            _rigidbody.AddForce(force, ForceMode.Force);
        }
    }
}