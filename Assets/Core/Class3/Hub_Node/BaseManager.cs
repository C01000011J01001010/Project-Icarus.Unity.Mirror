using Core.EventBus;
using Core.EventBus.Event;

namespace Core
{
    public abstract class BaseManager : BaseModule, IManager
    {
        protected virtual void OnEnable()
        {
            var evt = new RegisterManagerEvent(this);
            EventBus<RegisterManagerEvent>.Publish(evt);
        }

        protected virtual void OnDisable()
        {
            var evt = new UnregisterManagerEvent(this);
            EventBus<UnregisterManagerEvent>.Publish(evt);
        }
    }
}
