using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Core
{
    public abstract class BaseModule : BaseLeaf, IModule
    {
        private bool isActive;
        public bool IsActive => isActive;

        public abstract void Exit();

        public abstract IEnumerator Initialize(IModuleHub hub);

        public virtual void SetActive(bool active)
        {
            gameObject.SetActive(active);
            isActive = active;
        }
    }
}
