// example -> 주고받을 이벤트 명세서
using Core.EventBus;

namespace Core.EventBus.Example
{
    public struct ExampleEvent_PlayerDamaged : IEvent
    {
        public int PlayerId;
        public int CurrentHp;
        public int MaxHp;
    }
}
