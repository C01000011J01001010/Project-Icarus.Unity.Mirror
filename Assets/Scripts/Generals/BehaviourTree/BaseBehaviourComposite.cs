using System.Collections.Generic;



// 행동의 실행 방식을 정의하기 위한 composite 노드를 나타내는 클래스
public abstract class BaseBehaviourComposite : BaseRunnableBehaviour
{
    // 순차적으로 실행시킬 행동 클래스를 정의
    protected List<System.Func<BaseRunnableBehaviour>> runnables = new();

    // 실행중인 하위 노드를 나타냄
    public BaseRunnableBehaviour childBehaviour { get; protected set; }

    // 행동을 등록하기 위한 메서드
    // 매개변수가 존재하지 않는 행동을 등록하기 위한 메서드
    // ex) AddTask<SampleTask>();
    public void AddTask<T>() where T : BaseRunnableBehaviour, new()
    {
        runnables.Add(() => new T());
    }

    // 행동을 등록하기 위한 메서드
    // 매개변수가 존재하는 행동을 등록하기 위한 메서드
    // ex) AddTask(()=> new SampleTask(10, 20));
    public void AddTask(System.Func<BaseRunnableBehaviour> getTask)
    {
        runnables.Add(getTask);
    }


    public override void OnBehaviourStopped()
    {
        base.OnBehaviourStopped();
        childBehaviour?.OnBehaviourStopped();
        // 하위 노드 종료처리
    }

#if UNITY_EDITOR
    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        childBehaviour?.OnDrawGizmos();
    }

    public override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        childBehaviour?.OnDrawGizmosSelected();
    }
#endif
}
