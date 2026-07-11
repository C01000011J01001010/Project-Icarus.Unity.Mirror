using Core.EventBus;
using Core.EventBus.Event;
using UnityEngine;

namespace Core.Hub
{
     class UiHub : BaseModuleHub<IUi>
    {
        public override void AwakeFromContext()
        {
            EventBus<RegisterUiEvent>.Subscribe(RegisterManager);
        }

        public override void Exit()
        {
            base.Exit();
            EventBus<RegisterUiEvent>.Unsubscribe(RegisterManager);
        }

        private void RegisterManager(RegisterUiEvent evt)
        {
            IUi ui = evt.Ui;
            if (!moduleDict.ContainsKey(ui.GetType()))
            {
                moduleDict[ui.GetType()] = ui;
            }
            else
            {
                Destroy(ui as MonoBehaviour);
            }
        }
    }
}
