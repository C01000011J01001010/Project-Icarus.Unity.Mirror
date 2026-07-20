using Core.EventBus;
using Core.Manager.Culling;
using UnityEngine;

namespace Core
{
    // 장애물(정적 객체)에 부착될 초경량 액터
    public abstract class BaseCullingObjectActor : BaseActor, ICullingObject
    {
        public abstract CullingType cullingType { get; }

        // 🌟 자식 클래스에서 접근할 수 있도록 protected로 변경!
        protected Renderer[] _renderers;
        protected Collider[] _colliders;

        protected virtual void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
            _colliders = GetComponentsInChildren<Collider>(true);
        }

        protected virtual void Start()
        {
            // "저 태어났어요! 위치 기억해주세요!" 
            EventBus<CullingObjectRegistrationEvent>.Publish(new CullingObjectRegistrationEvent(this, true));
        }

        protected virtual void OnDestroy()
        {
            // 파괴 중일 때는 딕셔너리에서 안전하게 제거
            EventBus<CullingObjectRegistrationEvent>.Publish(new CullingObjectRegistrationEvent(this, false));
        }

        #region 기본적인 Culling 메서드
        public virtual void SetVisualActive(bool isActive)
        {
            // 모든 컬링 대상은 비주얼적 요소를 어떻게 할지 결정해야함
            Utility.Log($"Culling: {gameObject.name}를 Visual:{isActive}", isActive? LogColor.Green: LogColor.Red);
        }

        public virtual void SetPhysicsActive(bool isActive)
        {
            // 물리연산의 예외 존재
            Utility.Log(cullingType != CullingType.ActiveDynamic ?
                $"Culling: {gameObject.name}를 Physics : {isActive}" : "",
                isActive ? LogColor.Green : LogColor.Red);
        }
        #endregion
    }
}