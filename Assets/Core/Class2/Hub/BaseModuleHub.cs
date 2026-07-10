using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Hub
{
    internal abstract class BaseModuleHub<TModule> : BaseHub, IBaseHub
        where TModule : IModule
    {
        // 단일 매니저들을 담아두는 딕셔너리
        protected Dictionary<Type, TModule> moduleDict = new();

        public override void Exit()
        {
            foreach (var module in moduleDict.Values)
            {
                module?.Exit();
            }
        }

        public override IEnumerator Initialize()
        {
            foreach(var module in moduleDict.Values)
            {
                yield return module?.Initialize(this);
            }
        }

        public override IEnumerator LateInitialize()
        {
            foreach (var module in moduleDict.Values)
            {
                if(module is ILateInitialize lateModule)
                {
                    yield return lateModule?.LateInitialize();
                }
            }
        }

    }
}

