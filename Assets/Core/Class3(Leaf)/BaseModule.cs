using Core.EventBus;
using Core.Hub;
using System.Collections;

namespace Core
{
    public abstract class BaseModule : BaseLeaf, IModule
    {
        private bool isActive;
        public bool IsActive => isActive;

        public virtual void Exit() { }

        public virtual IEnumerator Initialize(IModuleHub hub) { yield return null; }

        public virtual void SetActive(bool active)
        {
            gameObject.SetActive(active);
            isActive = active;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            var evt = new ModuleRegistrationEvent(this, true);
            EventBus<ModuleRegistrationEvent>.Publish(evt);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            var evt = new ModuleRegistrationEvent(this, false);
            EventBus<ModuleRegistrationEvent>.Publish(evt);
        }
    }
}
