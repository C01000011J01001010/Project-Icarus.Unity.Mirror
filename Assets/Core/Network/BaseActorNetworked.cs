using System;
using Core.EventBus;
using Core.Hub;

namespace Core.Network
{
    // 멀티플레이 객체용 3계층 Leaf 기본 클래스

    public abstract class BaseActorNetworked : EventListenerNetworked, IActor
    {

        // NetworkBehaviour는 OnSpawn/OnDespawn 대신 FishNet 콜백을 사용하지만, 
        // 인터페이스 규격을 맞추기 위해 래핑해줍니다.
        //public virtual void OnSpawn() { }

        //public virtual void OnDespawn() { }
        protected override void OnEnable()
        {
            base.OnEnable();
            var evt = new ActorRegistrationEvent(this, true, myScope);
            EventBus<ActorRegistrationEvent>.Publish(evt);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            // Hub에 내가 안쓰임을 알림
            var evt = new ActorRegistrationEvent(this, false, myScope);
            EventBus<ActorRegistrationEvent>.Publish(evt);
        }
    }
}

