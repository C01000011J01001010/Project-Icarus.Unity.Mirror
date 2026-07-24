using Core;
using Core.EventBus;
using Core.Interface;
using UnityEngine;

namespace Icarus.Character
{
    // 다중 클라이언트 입력값을 가져오기 위한 인터페이스
    public interface IClientInputProvider
    {
        Vector2 GetPlayerInput(int clientId);
        int GetPlayerInputCount();
    }

    // 안전장치
    [RequireComponent(typeof(SharedActor))]
    public class ClientMoveInfoProvider : BaseSinglePublisher<IClientInputProvider>, IClientInputProvider
    {
        private SharedActor _sharedActor;

        protected override void Awake()
        {
            base.Awake();
            _sharedActor = GetComponent<SharedActor>();
        }

        public Vector2 GetPlayerInput(int clientId)
        {
            // ClientInputs가 null일 경우를 대비한 안전한 널 체크 방어막 추가
            if (_sharedActor.ClientInputs.TryGetValue(clientId, out Vector2 input))
            {
                return input;
            }

            return Vector2.zero; // 값이 없으면 (0, 0) 반환
        }

        public int GetPlayerInputCount() => _sharedActor.ClientInputs.Count;
    }
}