
namespace Core.EventBus.Event
{
    public struct RegisterUiEvent : IEvent
    {
        public IUi Ui;
        public RegisterUiEvent(IUi ui) => Ui = ui;
    }
}
