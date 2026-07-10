using Core.EventBus;
using Core.EventBus.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Core.Hub
{
    // TGroup 열거형에 따라 동적으로 확장되는 제네릭 ActorHub 기본형
    public abstract class ActorHub<TGroup> : BaseHub, IActorHub, IPriority
        where TGroup : struct, Enum
    {
        public abstract int Priority { get; }

        // 그룹별 다중 리스트를 관리하는 최적화된 전화번호부
        protected readonly Dictionary<TGroup, List<IActor<TGroup>>> actorDict = new();

        public override void Exit()
        {
            // 메모리 누수 방지를 위한 구독 해제
            EventBus<RegisterActorEvent<TGroup>>.Unsubscribe(OnRegisterRequest);
            EventBus<UnregisterActorEvent<TGroup>>.Unsubscribe(OnUnregisterRequest);
        }

        private void Awake()
        {
            EventBus<RegisterActorEvent<TGroup>>.Subscribe(OnRegisterRequest);
            EventBus<UnregisterActorEvent<TGroup>>.Subscribe(OnUnregisterRequest);
        }

        public override IEnumerator Initialize()
        {
            yield return null;
        }

        public override IEnumerator LateInitialize()
        {
            yield return null;
        }

        #region EventBus
        public void OnRegisterRequest(RegisterActorEvent<TGroup> evt)
        {
            var actor = evt.Actor;
            TGroup group = actor.GroupType;

            if (!actorDict.TryGetValue(group, out List<IActor<TGroup>> list))
            {
                list = new List<IActor<TGroup>>();
                actorDict[group] = list;
            }

            list.Add(actor);
        }
        public void OnUnregisterRequest(UnregisterActorEvent<TGroup> evt)
        {
            var actor = evt.Actor;
            TGroup group = actor.GroupType;

            if (actorDict.TryGetValue(group, out List<IActor<TGroup>> list))
            {
                list.Remove(actor);
            }
        }
        #endregion
    }
}