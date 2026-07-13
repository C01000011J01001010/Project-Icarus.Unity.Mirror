


using System;

namespace Core
{
    // 1. 제네릭 Actor 인터페이스 (TGroup은 열거형으로 제한)
    public interface IActor<TGroup> where TGroup : Enum
    {
        TGroup GroupType { get; }
        void OnSpawn();
        void OnDespawn();
    }
}