using System.Collections;
using UnityEngine;

namespace Core.Hub
{
    public abstract class BaseHub : MonoBehaviour, IInitialize, ILateInitialize
    {
        protected virtual void OnDestroy()
        {
            Exit();
        }
        public virtual void Exit() { }

        public virtual IEnumerator Initialize() { yield return null; }

        public virtual IEnumerator LateInitialize() { yield return null; }
    }
}

