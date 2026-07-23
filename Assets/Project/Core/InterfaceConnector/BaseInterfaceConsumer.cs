using Core.EventBus;

namespace Core
{
    /// <summary>
    /// 외부 Provider로부터 인터페이스(명함)를 안전하게 요청(Ping) 및 수신(Set)하여 캐싱하는 Base Consumer
    /// </summary>
    public abstract class BaseInterfaceConsumer<TSetEvent, TRequestEvent, TInterface> : BaseLeaf
        where TSetEvent : struct, IEvent
        where TRequestEvent : struct, IEvent
        where TInterface : class
    {
        // UI에서 안전하게 접근할 수 있는 캐싱된 타겟 인터페이스
        protected TInterface Target { get; private set; }

        protected virtual void Awake()
        {
            // (Pong 수신 준비) 명함이 날아오면 낚아채기 위해 구독
            EventBus<TSetEvent>.Subscribe(OnTargetSetInternal);
        }

        protected virtual void OnDestroy()
        {
            // 메모리 누수 방지
            EventBus<TSetEvent>.Unsubscribe(OnTargetSetInternal);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            // (Ping) UI가 켜질 때마다 "혹시 이미 켜져 있는 Provider 있나요?" 물어봄
            EventBus<TRequestEvent>.Publish(new TRequestEvent());
        }

        private void OnTargetSetInternal(TSetEvent evt)
        {
            Target = GetTargetFromEvent(evt);
            OnTargetSet(Target);
        }

        /// <summary>
        /// 이벤트 구조체에서 TInterface 타겟을 추출하는 로직 (자식 클래스에서 지정)
        /// </summary>
        protected abstract TInterface GetTargetFromEvent(TSetEvent evt);

        /// <summary>
        /// 타겟이 설정되는 순간 추가적인 처리(초기화 등)가 필요할 때 오버라이드할 콜백
        /// </summary>
        protected virtual void OnTargetSet(TInterface target) { }
    }
}