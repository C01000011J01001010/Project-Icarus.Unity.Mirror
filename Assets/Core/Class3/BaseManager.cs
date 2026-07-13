using Core.EventBus;
using Core.EventBus.Event;
using System.Collections;

namespace Core
{
    public abstract class BaseManager : BaseModule, IManager
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            var evt = new RegisterManagerEvent(this);
            EventBus<RegisterManagerEvent>.Publish(evt);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            var evt = new UnregisterManagerEvent(this);
            EventBus<UnregisterManagerEvent>.Publish(evt);
        }

        public override void Exit() { }

        public override IEnumerator Initialize(IModuleHub hub) { yield return null; }
    }
}
