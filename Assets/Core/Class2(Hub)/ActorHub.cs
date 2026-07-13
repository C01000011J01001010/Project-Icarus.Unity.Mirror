using Core.EventBus;
using Core.EventBus.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Core.Hub
{
    /// <summary>
    /// 여러타입의 Actor를 위한 등록, 해제 이벤트
    /// </summary>
    public struct ActorRegistrationEvent<TActorGroup> : IEvent
        where TActorGroup : Enum
    {
        public IActor<TActorGroup> actor;
        public bool isAdd;
        public ActorRegistrationEvent(IActor<TActorGroup> actor, bool isAdd)
        {
            this.actor = actor;
            this.isAdd = isAdd;
        }
    }

    // TGroup 열거형에 따라 동적으로 확장되는 제네릭 ActorHub 기본형
    public abstract class ActorHub<TActorGroup> : BaseHub, IActorHub, IPriority
        where TActorGroup : struct, Enum
    {
        public abstract int Priority { get; }

        // 그룹별 다중 리스트를 관리하는 최적화된 전화번호부
        protected readonly Dictionary<TActorGroup, List<IActor<TActorGroup>>> actorDict = new();

        public override void Exit()
        {
            // 메모리 누수 방지를 위한 구독 해제
            EventBus<ActorRegistrationEvent<TActorGroup>>.Unsubscribe(OnActorRegistration);
        }

        public void AwakeFromContext()
        {
            EventBus<ActorRegistrationEvent<TActorGroup>>.Subscribe(OnActorRegistration);
        }

        public override IEnumerator Initialize()
        {
            // Actor는 씬에 등장할때 OnSpawn 처리
            yield return null;
        }

        public override IEnumerator LateInitialize()
        {
            yield return null;
        }

        #region EventBus
        public void OnActorRegistration(ActorRegistrationEvent<TActorGroup> evt)
        {
            if(evt.isAdd)
            {
                OnRegisterRequest(evt.actor);
            }
            else
            {
                OnUnregisterRequest(evt.actor);
            }
        }
        public void OnRegisterRequest(IActor<TActorGroup> actor)
        {
            TActorGroup group = actor.GroupType;

            if (!actorDict.TryGetValue(group, out List<IActor<TActorGroup>> list))
            {
                list = new List<IActor<TActorGroup>>();
                actorDict[group] = list;
            }

            list.Add(actor);
        }
        public void OnUnregisterRequest(IActor<TActorGroup> actor)
        {
            TActorGroup group = actor.GroupType;

            if (actorDict.TryGetValue(group, out List<IActor<TActorGroup>> list))
            {
                list.Remove(actor);
            }
        }

        
        #endregion
    }
}