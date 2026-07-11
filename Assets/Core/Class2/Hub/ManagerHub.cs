using Core.EventBus;
using Core.EventBus.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Core.Hub
{
    internal class ManagerHub : BaseModuleHub<IManager>
    {
        public override void AwakeFromContext()
        {
            EventBus<RegisterManagerEvent>.Subscribe(RegisterManager);
        }

        public override void Exit()
        {
            base.Exit();
            EventBus<RegisterManagerEvent>.Unsubscribe(RegisterManager);
        }

        private void RegisterManager(RegisterManagerEvent evt)
        {
            IManager manager = evt.Manager;
            if (!moduleDict.ContainsKey(manager.GetType()))
            {
                moduleDict[manager.GetType()] = manager;
            }
            else
            {
                Destroy(manager as MonoBehaviour);
            }
        }
    }
}
