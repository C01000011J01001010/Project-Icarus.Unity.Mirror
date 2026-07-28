using CoreEngine;
using CoreEngine.EventBus;
using CoreEngine.Interface;
using UnityEngine;

namespace Icarus.Ui
{
    /// <summary>
    /// Y축 회전 인터페이스(IYRotationProvider)를 받아 RectTransform의 Z축 회전으로 표현하는 UI 기본 클래스
    /// </summary>
    public abstract class BaseYRotationUI<TInterface> : CoreMonoBehaviour
        where TInterface : class, IYRotationProvider
    {
        private RectTransform _rectTransform;

        private InterfaceReceiver<TInterface> _receiver = new();
        protected TInterface Target => _receiver.Target;

        protected virtual void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _receiver.Bind();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _receiver.Unbind();
        }

        protected virtual void OnTick(float deltaTime)
        {
            if (Target == null) return;

            // 자식 클래스가 정해준 수학 공식에 따라 Z 각도 연산 후 적용
            float zRotation = CalculateZRotation(Target.WorldYRotation);
            _rectTransform.localEulerAngles = new Vector3(0f, 0f, zRotation);
        }

        /// <summary>
        /// 전달받은 World Y 각도를 바탕으로 UI의 Z 각도를 연산 (원판: Y, 화살표: -Y 등)
        /// </summary>
        protected abstract float CalculateZRotation(float worldYRotation);
    }
}