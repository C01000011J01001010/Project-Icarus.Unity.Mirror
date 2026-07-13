using Core.EventBus;
using Core.Manager;
using Core.Update;
using UnityEngine;

public abstract class BaseLeaf : MonoBehaviour
{
    protected virtual void OnEnable() => RegisterTick();
    protected virtual void OnDisable() => UnregisterTick();

    protected void RegisterTick()
    {
        // 이제 인스펙터 필드 체크가 아닌 인터페이스의 구현된 그룹값을 사용합니다.
        if (this is ITickable tickable)
        {
            EventBus<R_TickEvent>.Publish(new R_TickEvent(tickable, tickable.TickGroup, true));
        }

        if (this is ILateTickable lateTickable)
        {
            EventBus<R_LateTickEvent>.Publish(new R_LateTickEvent(lateTickable, lateTickable.LateTickGroup, true));
        }

        if (this is IFixedTickable fixedTickable)
        {
            EventBus<R_FixedTickEvent>.Publish(new R_FixedTickEvent(fixedTickable, fixedTickable.FixedTickGroup, true));
        }
    }

    protected void UnregisterTick()
    {
        if (this is ITickable tickable)
            EventBus<R_TickEvent>.Publish(new R_TickEvent(tickable, tickable.TickGroup, false));

        if (this is ILateTickable lateTickable)
            EventBus<R_LateTickEvent>.Publish(new R_LateTickEvent(lateTickable, lateTickable.LateTickGroup, false));

        if (this is IFixedTickable fixedTickable)
            EventBus<R_FixedTickEvent>.Publish(new R_FixedTickEvent(fixedTickable, fixedTickable.FixedTickGroup, false));
    }
}