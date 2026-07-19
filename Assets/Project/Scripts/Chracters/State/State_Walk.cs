using UnityEngine;

public class State_Walk : BaseCharacterState
{
    baseCharacterAnim anim;

    public override void Initialize(BaseCharacter owner)
    {
        base.Initialize(owner);
        anim = owner.anim;
    }

    public override CharacterState? CheckTransitions()
    {
        if (!owner.isMove) return CharacterState.Idle;
        else if (owner.isSprint) return CharacterState.Sprint;
        return null;
    }

    public override void Enter()
    {
        anim.SetIsMove(true);
        anim.SetIsSprint(false);
    }

    public override void Exit(CharacterState? nextState)
    {

    }

    public override void Update(float deltaTime)
    {
        anim.SetInputMove(owner.inputMove);
    }
}