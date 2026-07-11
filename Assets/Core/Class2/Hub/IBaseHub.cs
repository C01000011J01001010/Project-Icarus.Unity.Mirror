using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Core.Hub
{
    internal interface IBaseHub : IModuleHub
    {
        //// Context가 이 허브를 깨울 때 호출하는 비동기 진입점
        //public abstract IEnumerator Initialize();

        //// 허브 내부 객체들의 로직 정리가 끝난 후 호출 (선택적)
        //public abstract IEnumerator LateInitialize();

        public void AwakeFromContext();
    }
}
