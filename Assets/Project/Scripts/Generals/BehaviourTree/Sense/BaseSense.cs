// 감각 클래스의 기반 형식
public abstract class BaseSense
{
    public BaseBehaviourController behaviourController {  get; private set; }

    // 초기화
    public virtual void OnSenseInitilized(BaseBehaviourController behaviourController)
    {
        this.behaviourController = behaviourController;
    }

    public virtual void OnSenseUpdated() { }

#if UNITY_EDITOR
    public virtual void OnDrawGizmos() { }

    public virtual void OnDrawGizmosSelected() { }
#endif

}
