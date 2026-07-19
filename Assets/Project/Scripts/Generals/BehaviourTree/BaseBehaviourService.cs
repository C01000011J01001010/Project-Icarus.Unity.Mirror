
// 서비스를 나타내기 위한 클래스
using System.Numerics;

public abstract class BaseBehaviourService
{
    public BaseBehaviourController behaviourController {  get; private set; }

    public virtual void Initialize(BaseBehaviourController behaviourController)
    {
        this.behaviourController = behaviourController;
    }

    public abstract void Tick();


    public virtual void OnServiceFinish() { }
    public virtual void OnDrawGizmos() { }
    public virtual void OnDrawGizmosSelected() { }


    public static implicit operator bool(BaseBehaviourService target) => target is not null;
}
