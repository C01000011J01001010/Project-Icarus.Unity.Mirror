using Core.EventBus;

namespace Core.Interface
{
    /// <summary>
    /// 외부 Provider로부터 인터페이스(명함)를 안전하게 요청(Ping) 및 수신(Set)하여 캐싱하는 Base Consumer
    /// </summary>
    public abstract class BaseInterfaceConsumer<TInterface> : BaseLeaf
        where TInterface : class
    {
        // UI/소비자 측에서 안전하게 접근할 수 있는 캐싱된 타겟 인터페이스
        protected TInterface Target { get; private set; }

        protected virtual void Awake()
        {
            // (Pong 수신 준비) 명함이 날아오면 낚아채기 위해 제네릭 이벤트 구독
            EventBus<SetProviderEvent<TInterface>>.Subscribe(OnTargetSetInternal);
        }

        protected virtual void OnDestroy()
        {
            // 메모리 누수 방지
            EventBus<SetProviderEvent<TInterface>>.Unsubscribe(OnTargetSetInternal);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            // (Ping) Consumer가 켜질 때마다 "혹시 이미 켜져 있는 Provider 있나요?" 물어봄
            EventBus<RequestProviderEvent<TInterface>>.Publish(new RequestProviderEvent<TInterface>());
        }

        private void OnTargetSetInternal(SetProviderEvent<TInterface> evt)
        {
            // 💡 evt.Provider가 이미 TInterface 형식이므로 캐스팅이나 추상 메서드가 필요 없습니다!
            Target = evt.Provider;
            OnTargetSet(Target);
        }

        /// <summary>
        /// 타겟이 설정되는 순간 추가적인 처리(초기화, UI 갱신 등)가 필요할 때 오버라이드할 콜백
        /// </summary>
        protected virtual void OnTargetSet(TInterface target) { }
    }
}