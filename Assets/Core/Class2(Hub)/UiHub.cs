using Core.EventBus;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Core.Hub
{
     class UiHub : BaseModuleHub<IUi>
    {
        protected override bool moduleEnabled => false;
        public override IEnumerator LateInitialize()
        {
            yield return base.LateInitialize();

            //TODO: 씬마다 HUD만 활성화시키는 기능 추가해야함
        }
    }
}
