using Core.EventBus;
using Core;

namespace Core.EventBus.Event
{
    public struct RegisterManagerEvent : IEvent
    {
        public IManager Manager;
        public RegisterManagerEvent(IManager manager) => Manager = manager;
    }
}
