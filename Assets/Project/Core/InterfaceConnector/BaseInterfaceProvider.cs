using UnityEngine;
using Core.EventBus;

namespace Core.Interface
{
    // 💡 이벤트 구조체 2개를 받던 것을 버리고, "어떤 인터페이스를 제공할 것인가" 하나만 받습니다.
    public abstract class BaseInterfaceProvider<TInterface> : MonoBehaviour // (또는 기존 상속 Base)
        where TInterface : class
    {
        protected virtual void OnEnable()
        {
            // 💡 마법이 일어나는 곳: 나 자신(this)을 TInterface로 캐스팅합니다.
            TInterface provider = this as TInterface;

            if (provider != null)
            {
                // 제네릭 이벤트를 생성하여 EventBus에 태워 보냅니다.
                EventBus<SetProviderEvent<TInterface>>.Publish(new SetProviderEvent<TInterface>(provider));
            }
            else
            {
                Debug.LogError($"[Provider Error] {gameObject.name} 객체가 {typeof(TInterface).Name} 인터페이스를 상속받지 않았습니다!");
            }

            // 요청(Request) 이벤트 구독
            EventBus<RequestProviderEvent<TInterface>>.Subscribe(OnProviderRequested);
        }

        protected virtual void OnDisable()
        {
            EventBus<RequestProviderEvent<TInterface>>.Unsubscribe(OnProviderRequested);
            // 필요하다면 null을 보내 해제하는 로직 추가
        }

        private void OnProviderRequested(RequestProviderEvent<TInterface> evt)
        {
            TInterface provider = this as TInterface;
            if (provider != null)
            {
                EventBus<SetProviderEvent<TInterface>>.Publish(new SetProviderEvent<TInterface>(provider));
            }
        }
    }
}