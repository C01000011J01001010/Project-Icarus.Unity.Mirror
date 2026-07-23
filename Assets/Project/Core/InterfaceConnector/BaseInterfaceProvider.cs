using Core.EventBus;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// 인터페이스를 주고받기 위한 객체
    /// </summary>
    public abstract class BaseInterfaceProvider<TRequestEvent, TPublishEvent> : MonoBehaviour
        where TRequestEvent : struct, IEvent
        where TPublishEvent : struct, IEvent
    {
        private void Awake()
        {
            // (핑) 나의 인터페이스를 필요한 이에게 줄 수 있도록 먼저 구독 
            EventBus<TRequestEvent>.Subscribe(OnRequestReceived);
        }

        private void OnDestroy()
        {
            // 메모리 누수 방지를 위한 구독 해제
            EventBus<TRequestEvent>.Unsubscribe(OnRequestReceived);
        }

        private void OnEnable()
        {
            // (퐁)
            // 내가 일어났으니 내 인터페이스 필요한 친구 있나 물어보기
            PublishInterface();
        }

        private void OnRequestReceived(TRequestEvent evt)
        {

            if (isActiveAndEnabled)
            {
                PublishInterface();
            }
        }

        private void PublishInterface()
        {
            EventBus<TPublishEvent>.Publish(GetPublishEvent());
        }

        protected abstract TPublishEvent GetPublishEvent();
    }
}
