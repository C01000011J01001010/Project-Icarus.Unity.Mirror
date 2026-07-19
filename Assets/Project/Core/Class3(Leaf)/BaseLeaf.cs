using Core;
using Core.EventBus;
using UnityEngine;

public abstract class BaseLeaf : MonoBehaviour
{
    // 어느 Context 산하로 들어갈지 결정
    [SerializeField] protected ContextScope myScope;

    public void SetScope(ContextScope scope)
    {
        myScope = scope;
        OnSetScope(scope);
    }

    protected virtual void OnSetScope(ContextScope scope)
    {

    }

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


#if UNITY_EDITOR
    // 유니티 에디터에서 값이 변경되거나, 씬에 배치될 때 자동 호출되는 함수
    protected virtual void OnValidate()
    {
        // 아직 스코프가 None(미지정) 상태일 때만 자동 추론을 작동시킵니다.
        if (myScope == ContextScope.None)
        {
            // 현재 이 스크립트가 물리적으로 배치된 씬의 이름을 확인합니다.
            // 하이라키에 올려두는 순간 즉시 판별됩니다.
            string mySceneName = gameObject.scene.name;

            if (mySceneName == Constants.SCENE_GlobalScene)
            {
                myScope = ContextScope.Project;
                // 에디터 인스펙터 창의 값을 강제로 갱신하고 저장 상태로 만듭니다.
                UnityEditor.EditorUtility.SetDirty(this);
            }
            else if (!string.IsNullOrEmpty(mySceneName))
            {
                // 글로벌 씬이 아닌 일반 씬에 배치되었다면 안전하게 Scene 소속으로 고정해 줍니다.
                myScope = ContextScope.Scene;
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
    }
#endif
}