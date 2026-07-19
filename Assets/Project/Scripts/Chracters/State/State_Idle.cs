using UnityEngine.TextCore.Text;

public class State_Idle : BaseCharacterState
{
    baseCharacterAnim anim;

    public override void Initialize(BaseCharacter owner)
    {
        anim = owner.anim;
        base.Initialize(owner);
    }

    public override CharacterState? CheckTransitions()
    {
        if (owner.isMove)
        {
            if(owner.isSprint)return CharacterState.Sprint;
            else return CharacterState.Walk;
        }
        return null;
    }

    public override void Enter()
    {
        anim.SetIsMove(false);
    }

    public override void Exit(CharacterState? nextState)
    {
        
    }
}