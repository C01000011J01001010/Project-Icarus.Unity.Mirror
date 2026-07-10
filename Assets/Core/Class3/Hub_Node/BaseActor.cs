using Core.EventBus;
using Core.EventBus.Event;
using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Core
{
    public abstract class BaseActor<TActorGroup> : MonoBehaviour, IActor<TActorGroup>
        where TActorGroup : struct, Enum
    {
        public abstract TActorGroup GroupType { get; }

        protected virtual void OnEnable()
        {
            // Hub에 내가 등록됐음을 알림
            var evt = new RegisterActorEvent<TActorGroup>(this);
            EventBus<RegisterActorEvent<TActorGroup>>.Publish(evt);
        }

        protected virtual void OnDisable()
        {
            // Hub에 내가 안쓰임을 알림
            var evt = new UnregisterActorEvent<TActorGroup>(this);
            EventBus<UnregisterActorEvent<TActorGroup>>.Publish(evt);
        }


        public abstract void OnDespawn();

        public abstract void OnSpawn();
    }
}
