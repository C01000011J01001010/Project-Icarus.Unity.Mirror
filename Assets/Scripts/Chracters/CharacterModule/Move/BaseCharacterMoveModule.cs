using System.Collections;
using UnityEngine;
using static UnityEngine.EventSystems.StandaloneInputModule;

public abstract class BaseCharacterMoveModule : BaseCharacterModule, 
    IFixedTickable
{
    
    protected float moveSpeed;
    protected bool isSprint;
    protected Vector2 moveInput;
    public float SprintMul = 2f;
    public override void Exit()
    {
        base.Exit();
        Owner.OnMoveInput -= OnMoveInput;
        Owner.OnMoveSpeedChanged -= SetMoveSpeed;
        Owner.OnSprintInput -= SetSprint;
    }

    public override IEnumerator Initialize(IModuleHub hub)
    {
        yield return base.Initialize(hub);
        Owner.OnMoveInput -= OnMoveInput;
        Owner.OnMoveInput += OnMoveInput;
        Owner.OnMoveSpeedChanged -= SetMoveSpeed;
        Owner.OnMoveSpeedChanged += SetMoveSpeed;
        Owner.OnSprintInput -= SetSprint;
        Owner.OnSprintInput += SetSprint;
    }

    public void FixedTick(float fixedDeltaTime)
    {
        Physics_Move(fixedDeltaTime);
    }

    private void Physics_Move(float fixedDeltaTime)
    {
        // 방향과 속도 계산
        Vector2 nextVec = moveInput.normalized * moveSpeed * fixedDeltaTime;
        if (isSprint) nextVec *= SprintMul;

        // 실제 움직임 처리
        ProcessRigidbody(nextVec);
    }

    protected abstract void ProcessRigidbody(Vector2 deltaMove);

    public virtual void OnMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    public virtual void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    public virtual void SetSprint(bool value)
    {
        isSprint = value;
    }
}