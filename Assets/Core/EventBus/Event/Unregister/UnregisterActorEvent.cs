using Core.Hub;
using System;

namespace Core.EventBus.Event
{
    public struct UnregisterActorEvent<ActorGroup> : IEvent
        where ActorGroup : struct, Enum
    {
        public IActor<ActorGroup> Actor;
        public UnregisterActorEvent(IActor<ActorGroup> actor) => Actor = actor;
    }
}
