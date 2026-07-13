using Core.EventBus;
using Core.EventBus.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Core.Hub
{
    /// <summary>
    /// Manager, Ui 등 단일객체 모듈을 위한 등록, 해제 이벤트
    /// </summary>
    public struct ModuleRegistrationEvent : IEvent
    {
        public IModule module;
        public bool isAdd;
        public ModuleRegistrationEvent(IModule module, bool isAdd)
        {
            this.module = module;
            this.isAdd = isAdd;
        }
    }

    internal abstract class BaseModuleHub<TModule> : BaseHub, IBaseHub
        where TModule : class, IModule
    {
        // 단일 매니저들을 담아두는 딕셔너리
        protected Dictionary<Type, TModule> moduleDict = new();
        protected bool _isInitComplete = false; // Hub의 초기화 완료 상태

        public virtual void AwakeFromContext()
        {
            // 모듈이 등록할 수 있도록 Context로부터 시작하는 가장 빠른 Awake 사용하여 구독
            EventBus<ModuleRegistrationEvent>.Subscribe(OnModuleRegistration);
        }

        public override void Exit()
        {
            foreach (var module in moduleDict.Values)
            {
                
                module?.Exit();
            }
            // 모듈들이 등록취소하길 기다린 후 나도 구독취소
            EventBus<ModuleRegistrationEvent>.Unsubscribe(OnModuleRegistration);
        }

        public override IEnumerator Initialize()
        {
            yield return base.Initialize();
            foreach(var module in moduleDict.Values)
            {
                yield return module?.Initialize(this);
                yield return null;
            }
            
        }

        public override IEnumerator LateInitialize()
        {
            yield return base.LateInitialize();
            foreach (var module in moduleDict.Values)
            {
                if(module is ILateInitialize lateModule)
                {
                    yield return lateModule?.LateInitialize();
                    yield return null;
                }
            }

            // Context로 인한 초기화 완료
            _isInitComplete = true;
        }

        private void OnModuleRegistration(ModuleRegistrationEvent evt)
        {
            if(evt.module is TModule module)
            {
                if (evt.isAdd)
                {
                    RegisterModule(module);
                }
                else
                {
                    UnregisterModule(module);
                }
            }
        }

        private void RegisterModule(TModule module)
        {
            // 등록한적이 없거나 등록했던 객체가 페이크 널일때
            Type typeKey = module.GetType();
            if (!moduleDict.TryGetValue(typeKey, out TModule old) ||
                (old as UnityEngine.Object) == null)
            {
                moduleDict[typeKey] = module;
            }

            // Context 주도의 초기화가 이미 끝났는데 등장한 모듈의 경우
            if (_isInitComplete)
            {
                CatchUpModule(module);
            }
        }

        private void UnregisterModule(TModule module)
        {
            Type typeKey = module.GetType();
            if (moduleDict.ContainsKey(typeKey))
            {
                moduleDict.Remove(typeKey);
            }
        }

        public void CatchUpModule(TModule tardyModule)
        {
            // 이미 전체 초기화가 끝난 상태에서 들어온 '지각생'이라면?
            // 리스트에 넣는 것에 그치지 않고, 그 즉시 개별 초기화를 진행
            if (_isInitComplete)
            {
                // 코루틴으로 단일 객체만 즉시 초기화 실행
                StartCoroutine(tardyModule.Initialize(this));

                // LateInitialize가 있다면 이어서 호출
                if (tardyModule is ILateInitialize lateModule)
                {
                    lateModule.LateInitialize();
                }
            }
        }

        // [수정된 부분] 
        // 1. T에 class 제약 조건을 추가하여 'as T' 캐스팅이 가능하게 만듭니다.
        // 2. 딕셔너리에 없을 경우를 대비한 return default(또는 null) 구문을 추가합니다.
        public virtual T GetModule<T>() where T : class, IModule // 또는 TModule
        {
            if (moduleDict.TryGetValue(typeof(T), out var module))
            {
                return module as T;
            }
            // 모듈을 찾지 못했을 때의 안전장치 및 경고
            Debug.LogWarning($"[BaseModuleHub] {typeof(T).Name} 모듈이 등록되어 있지 않습니다.");
            return null;
        }

    }
}

