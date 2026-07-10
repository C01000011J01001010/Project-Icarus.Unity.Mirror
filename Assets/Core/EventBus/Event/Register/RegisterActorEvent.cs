using Core.Hub;
using System;

namespace Core.EventBus.Event
{
    public struct RegisterActorEvent<ActorGroup> : IEvent
        where ActorGroup : struct, Enum
    {
        public IActor<ActorGroup> Actor;
        public RegisterActorEvent(IActor<ActorGroup> actor) => Actor = actor;
    }
}
