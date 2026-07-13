using FishNet.Object;
using Core.Interface;
using System;
using Core.EventBus;

namespace Core.Network
{
    // 멀티플레이 객체용 3계층 Leaf 기본 클래스

    public abstract class BaseActorNetworked<TActorGroup> : EventListenerNetworked, IActor<TActorGroup>
        where TActorGroup : struct, Enum
    {
        public abstract TActorGroup GroupType { get; }

        // NetworkBehaviour는 OnSpawn/OnDespawn 대신 FishNet 콜백을 사용하지만, 
        // 인터페이스 규격을 맞추기 위해 래핑해줍니다.
        public virtual void OnSpawn() { }
        public virtual void OnDespawn() { }
    }
}

