using Core.EventBus;
using Core.Update;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Manager
{
    #region Tcik구별을 위한 enum
    // enum 순서를 Update 순서에 사용
    public enum TickGroup 
    { 
        Initial, 
        OnController, 
        OnCharacter, 
        Object 
    }
    public enum LateTickGroup 
    { 
        Camera,
        Post 
    }
    public enum FixedTickGroup 
    { 
        Physics 
    }
    #endregion

    #region Tick 등록 이벤트
    public struct RegisterTickEvent : IEvent
    {
        public ITickable Target;
        /// <summary>
        /// 자신의 업데이트 순서 결정
        /// </summary>
        public TickGroup Group;
        /// <summary>
        /// true : 구독추가
        /// <para>false: 구독취소</para> 
        /// </summary>
        public bool IsAdd;
        public RegisterTickEvent(ITickable target, TickGroup group, bool isAdd) { Target = target; Group = group; IsAdd = isAdd; }
    }
    public struct RegisterLateTickEvent : IEvent
    {
        public ILateTickable Target;
        /// <summary>
        /// 자신의 업데이트 순서 결정
        /// </summary>
        public LateTickGroup Group;
        /// <summary>
        /// true : 구독추가
        /// <para>false: 구독취소</para> 
        /// </summary>
        public bool IsAdd;
        public RegisterLateTickEvent(ILateTickable target, LateTickGroup group, bool isAdd) { Target = target; Group = group; IsAdd = isAdd; }
    }
    public struct RegisterFixedTickEvent : IEvent
    {
        public IFixedTickable Target;
        /// <summary>
        /// 자신의 업데이트 순서 결정
        /// </summary>
        public FixedTickGroup Group;
        /// <summary>
        /// true : 구독추가
        /// <para>false: 구독취소</para> 
        /// </summary>
        public bool IsAdd;
        public RegisterFixedTickEvent(IFixedTickable target, FixedTickGroup group, bool isAdd) { Target = target; Group = group; IsAdd = isAdd; }
    }
    #endregion

    public class UpdateManager : MonoBehaviour
    {
        #region Runner
        private abstract class BaseRunner<TInterface>
        {
            protected readonly HashSet<TInterface> _active = new HashSet<TInterface>(100);
            private readonly List<TInterface> _pendingAdds = new List<TInterface>(20);
            private readonly List<TInterface> _pendingRemoves = new List<TInterface>(20);
            private bool _isUpdating = false;

            public void Register(TInterface target, bool isAdd)
            {
                // 업데이트 중이면 예약 걸어놓기
                if (_isUpdating)
                {
                    if (isAdd) _pendingAdds.Add(target);
                    else _pendingRemoves.Add(target);
                }
                else
                {
                    if (isAdd) _active.Add(target);
                    else _active.Remove(target);
                }
            }

            protected abstract void Execute(float dt);

            public void Run(float dt)
            {
                _isUpdating = true;
                //foreach (var target in _active)
                //{
                //    if (target != null) target.Tick(dt); // 안전망
                //}
                Execute(dt);
                _isUpdating = false;

                // 밀린 작업 일괄 처리 (GC 발생 안함)
                if (_pendingAdds.Count > 0) { foreach (var t in _pendingAdds) _active.Add(t); _pendingAdds.Clear(); }
                if (_pendingRemoves.Count > 0) { foreach (var t in _pendingRemoves) _active.Remove(t); _pendingRemoves.Clear(); }
            }
        }
        private class TickRunner : BaseRunner<ITickable>
        {
            protected override void Execute(float dt)
            {
                foreach (var target in _active)
                {
                    if (target != null) target.Tick(dt);
                }
            }
        }
        private class FixedTickRunner : BaseRunner<IFixedTickable>
        {
            protected override void Execute(float dt)
            {
                foreach (var target in _active)
                {
                    if (target != null) target.FixedTick(dt);
                }
            }
        }
        private class LateTickRunner : BaseRunner<ILateTickable>
        {
            protected override void Execute(float dt)
            {
                foreach (var target in _active)
                    if (target != null) target.LateTick(dt);
            }
        }
        #endregion

        // 💡 핵심 2: Dictionary 대신 Enum을 정수(int)로 변환해 배열 인덱스로 사용 (압도적인 성능)
        private TickRunner[] _tickRunners;
        private LateTickRunner[] _lateTickRunners;
        private FixedTickRunner[] _fixedTickRunners;

        private void Awake()
        {
            _tickRunners = CreateRunners<TickGroup, TickRunner, ITickable>();
            _lateTickRunners = CreateRunners<LateTickGroup, LateTickRunner, ILateTickable>();
            _fixedTickRunners = CreateRunners<FixedTickGroup, FixedTickRunner, IFixedTickable>();
        }

        private TRunner[] CreateRunners<TGroup, TRunner, TInterface>()
            where TGroup : struct, Enum
            where TRunner : BaseRunner<TInterface>, new()
        {
            int groupCount = Enum.GetNames(typeof(TGroup)).Length;

            // 러너 수만큼 배열 할당
            TRunner[] runners = new TRunner[groupCount];

            // 각 러너 생성
            for (int i = 0; i < groupCount; i++) runners[i] = new TRunner();

            return runners;
        }

        private void OnEnable()
        {
            EventBus<RegisterTickEvent>.Subscribe(OnRegisterTick);
            EventBus<RegisterFixedTickEvent>.Subscribe(OnRegisterFixedTick);
        }

        private void OnDisable()
        {
            EventBus<RegisterTickEvent>.Unsubscribe(OnRegisterTick);
            EventBus<RegisterFixedTickEvent>.Unsubscribe(OnRegisterFixedTick);
        }

        // 이벤트 수신 시 해당 그룹의 인덱스를 찾아 Runner에게 위임
        private void OnRegisterTick(RegisterTickEvent evt)
            => _tickRunners[(int)evt.Group].Register(evt.Target, evt.IsAdd);

        private void OnRegisterFixedTick(RegisterFixedTickEvent evt)
            => _fixedTickRunners[(int)evt.Group].Register(evt.Target, evt.IsAdd);

        // ==========================================
        // 유니티 생명주기 제어부 (순서 보장)
        // ==========================================

        private void Update()
        {
            float dt = Time.deltaTime;
            for (int i = 0; i < _tickRunners.Length; i++)
                _tickRunners[i].Run(dt);
        }

        private void LateUpdate()
        {
            float dt = Time.deltaTime;
            for (int i = 0; i < _lateTickRunners.Length; i++)
                _lateTickRunners[i].Run(dt);
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            for (int i = 0; i < _fixedTickRunners.Length; i++)
                _fixedTickRunners[i].Run(dt);
        }
    }
}