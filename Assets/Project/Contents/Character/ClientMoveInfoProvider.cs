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
            Utility.Log("버그 발생! 자세한 내용은 주석 참조!", LogColor.Red);
            // SharedActor에서 Dictionary로 입력을 보관하기 때문에
            // 클라이언트id가 앞순서인 유저의 입력이 없는 이상
            // 나의 클라이언트 id가 유효해도 제대로 된 값을 받아갈 수 없음
            // 때문에 ServerManager.OnRemoteConnectionState를 사용하여 접속 유저가 변경될때마다 EventBus로 알려서
            // 접속중인 유저의 숫자가 중요한 객체에 알려줘야함
            return Vector2.zero; // 값이 없으면 (0, 0) 반환
        }

        public int GetPlayerInputCount() => _sharedActor.ClientInputs.Count;
    }
}