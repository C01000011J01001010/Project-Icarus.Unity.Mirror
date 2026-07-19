using Core.EventBus;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Core.Hub
{
    internal sealed class UiHub : BaseModuleHub<IUi>
    {
        protected override bool moduleEnabled => false;

        public override IEnumerator Initialize()
        {
            Utility.LogFunctionCallStart(this);
            return base.Initialize();
        }
        public override IEnumerator LateInitialize()
        {
            Utility.LogFunctionCallStart(this);
            yield return base.LateInitialize();

            //TODO: 씬마다 HUD만 활성화시키는 기능 추가해야함
        }
    }
}
