using System.Collections;
using UnityEngine;

namespace Core.Hub
{
    public abstract class BaseHub : MonoBehaviour, IInitialize, ILateInitialize
    {
        private void OnDestroy()
        {
            Exit();
        }
        public abstract void Exit();

        public abstract IEnumerator Initialize();

        public abstract IEnumerator LateInitialize();
    }
}

