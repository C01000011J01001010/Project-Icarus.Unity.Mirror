using Core.Manager.Culling;
using UnityEngine;

namespace Core
{
    internal class CullingPassiveDynamicActor : BaseCullingDynamicActor
    {
        public override CullingType cullingType => CullingType.PassiveDynamic;

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        public override void SetVisualActive(bool isActive)
        {
            SetFlagVisualActive(isActive);

            if (gameObject.activeSelf != isActive)
            {
                gameObject.SetActive(isActive);
            }
        }

        public override void SetPhysicsActive(bool isActive)
        {
            SetFlagPhysicsActive(isActive);

            for (int i = 0; i < _colliders.Length; i++)
            {
                if (_colliders[i] != null && _colliders[i].enabled != isActive)
                {
                    _colliders[i].enabled = isActive;
                }
            }
        }
    }
}