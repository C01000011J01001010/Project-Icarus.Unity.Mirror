using Core.EventBus;
using Core;

namespace Core.EventBus.Event
{
    public struct UnregisterManagerEvent : IEvent
    {
        public IManager Manager;
        public UnregisterManagerEvent(IManager manager) => Manager = manager;
    }
}
