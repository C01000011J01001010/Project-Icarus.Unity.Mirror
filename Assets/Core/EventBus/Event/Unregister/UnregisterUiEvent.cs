using Core.EventBus;
using Core;

namespace Core.EventBus.Event
{
    public struct UnregisterUiEvent : IEvent
    {
        public IUi Ui;
        public UnregisterUiEvent(IUi ui) => Ui = ui;
    }
}
