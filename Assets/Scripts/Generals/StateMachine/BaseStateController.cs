using Core.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class BaseStateController<TState> : MonoBehaviour, IInitialize, ILateInitialize, ITickable, IFixedTickable
    where TState : struct, Enum
{
    [SerializeField]protected TState defaultStateType;
    protected TState currentStateType;
    protected BaseState<TState> CurrentState;
    protected Dictionary<TState, BaseState<TState>> stateDict;

    public TState CurrentStateType => currentStateType;

    public TickGroup TickGroup => TickGroup.Character;

    public FixedTickGroup FixedTickGroup => FixedTickGroup.Physics;

    protected virtual void OnDestroy()
    {
        
    }

    public virtual void Exit()
    {
        // None을 전달
        CurrentState?.Exit(null);
    }

    public virtual IEnumerator Initialize()
    {
        stateDict = ProductState();
        yield break;
    }

    public virtual IEnumerator LateInitialize()
    {
        StartState();
        yield break;
    }

    protected void StartState()
    {
        currentStateType = defaultStateType;
        CurrentState = GetState(defaultStateType);

        if(CurrentState is not null)
        {
            CurrentState.Enter();
        }
        else
        {
            Debug.LogWarning("CurrentState is null, please check the stateDict");
        }
        
    }

    protected abstract Dictionary<TState, BaseState<TState>> ProductState();

    public virtual void Tick(float deltaTime)
    {
        TState? nextState = CurrentState.CheckTransitions();

        if (nextState.HasValue)
        {
            TransitionTo(nextState.Value);
            return;
        }
        CurrentState?.Update(deltaTime);
    }

    public virtual void FixedTick(float FixedDeltaTime)
    {
        CurrentState?.FixedUpdate(FixedDeltaTime);
    }

    protected virtual void TransitionTo(TState nextState)
    {
        if (currentStateType.Equals(nextState)) return;

        CurrentState.Exit(nextState);

        currentStateType = nextState;
        CurrentState = GetState(nextState);

        CurrentState.Enter();
    }

    protected virtual BaseState<TState> GetState(TState wantState)
    {
        if (stateDict.ContainsKey(wantState)) return stateDict[wantState];
        else
        {
            Debug.LogWarning($"the key({wantState.ToString()}) not contained in stateDict");
            return null;
        }
    }

    
}