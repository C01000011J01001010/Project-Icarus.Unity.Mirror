using Core;
using Core.EventBus;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using UnityEngine;
//using static Core.Utility; // Log 사용을 위한 네임스페이스 추가

namespace Icarus.Character
{
    // (Ping) 늦게 켜진 다중 화살표 UI가 "입력 제공자 있나요?" 하고 묻는 이벤트
    public struct RequestClientInputProviderEvent : IEvent { }

    // (Pong/Push) 입력 제공자가 "내 인터페이스(명함) 받아라" 하고 쏘는 이벤트
    public struct SetClientInputProviderEvent : IEvent
    {
        public IClientInputProvider Provider;
        public SetClientInputProviderEvent(IClientInputProvider provider)
        {
            Provider = provider;
        }
    }

    // 다중 클라이언트 입력값을 가져오기 위한 인터페이스
    public interface IClientInputProvider
    {
        Vector2 GetPlayerInput(int clientId);
        int GetPlayerInputCount();
    }

    // 안전장치
    [RequireComponent(typeof(SharedActor))]
    public class ClientMoveInfoProvider : BaseInterfaceProvider<RequestClientInputProviderEvent, SetClientInputProviderEvent>, IClientInputProvider
    {
        private SharedActor _sharedActor;

        protected override void Awake()
        {
            base.Awake();
            _sharedActor = GetComponent<SharedActor>();
        }

        protected override SetClientInputProviderEvent GetPublishEvent()
        {
            Utility.Log("[ClientMoveInfoProvider] UI에 다중 플레이어 입력 제공자(명함)를 퍼블리시합니다.", LogColor.Cyan);
            return new SetClientInputProviderEvent(this);
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