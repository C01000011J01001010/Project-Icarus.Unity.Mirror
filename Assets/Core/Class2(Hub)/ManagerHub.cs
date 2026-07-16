using Core.EventBus;
using Core.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Core.Hub
{
    internal class ManagerHub : BaseModuleHub<IManager>
    {
        protected override bool moduleEnabled => true;
    }
}
