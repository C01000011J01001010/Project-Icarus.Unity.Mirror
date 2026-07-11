using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;




public class CharacterStateController : BaseStateController<CharacterState>, ICharacterModule,
    ILateInitialize
{
    public BaseCharacter Owner { get; private set; }

    public bool IsActive { get; private set; }

    public BaseCharacter Character => throw new NotImplementedException();

    public event Action<bool> Evnet_OnSetActive;

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Evnet_OnSetActive = null;
    }

    public override void Exit()
    {
        base.Exit();
        SetActive(false);
    }

    public IEnumerator Initialize(IModuleHub owner)
    {
        Owner = owner.AsOrThrow<BaseCharacter>();
        yield return base.Initialize();
    }

    public override IEnumerator LateInitialize()
    {
        foreach (BaseState<CharacterState> cur in stateDict.Values)
        {
            if(cur is BaseCharacterState asCharacterState)
            {
                asCharacterState.Initialize(Owner);
            }    
        }
        yield return base.LateInitialize();
    }

    public void SetActive(bool active)
    {
        IsActive = active;
        Evnet_OnSetActive?.Invoke(active);
    }

    protected override Dictionary<CharacterState, BaseState<CharacterState>> ProductState()
    {
        Dictionary<CharacterState, BaseState<CharacterState>> stateDict = new();
        stateDict.Add(CharacterState.Idle, new State_Idle());
        stateDict.Add(CharacterState.Walk, new State_Walk());
        stateDict.Add(CharacterState.Sprint, new State_Sprint());
        return stateDict;
    }
}
