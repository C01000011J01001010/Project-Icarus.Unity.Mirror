using CoreEngine.Network.FishNetExtension.Manager;
using UnityEngine;

namespace Icarus.Manager
{
    public enum NetActorPoolType
    {
        SharedActor,
    }
    public class NetActorSpawnManager : BaseNetObjectSpawnManager<NetActorPoolType, NetActorPoolManager>
    {

    }
}

