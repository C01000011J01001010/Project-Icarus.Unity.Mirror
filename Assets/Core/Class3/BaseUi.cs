using Core.EventBus;
using Core.EventBus.Event;

namespace Core
{
    public abstract class BaseUi : BaseModule, IUi
    {
        protected virtual void OnEnable()
        {
            var evt = new RegisterUiEvent(this);
            EventBus<RegisterUiEvent>.Publish(evt);
        }

        protected virtual void OnDisable()
        {
            var evt = new UnregisterUiEvent(this);
            EventBus<UnregisterUiEvent>.Publish(evt);
        }
    }
}
