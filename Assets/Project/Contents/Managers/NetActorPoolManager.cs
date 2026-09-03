using CoreEngine;
using CoreEngine.Manager;
using CoreEngine.Network.FishNetExtension.Manager;

namespace Icarus.Manager
{
    public class NetActorPoolManager : BaseNetObjectPoolManager<NetActorPoolType>, IPriority
    {
        public int Priority => (int)ManagerPriority.Infrastructure;
    }
}
