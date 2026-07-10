using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


/* BehaviourTree 기본개념
 * Root : 기본적으로 단 하나의 Task나 composite을 참조할 수 있음
 * Task(BaseRunnableBehaviour) : 하나의 행동(작업)을 나타냄
 * Composite : 분기가 실행되는 방식의 기본 규칙을 정의(Selector, Sequence)
 * Service : Task와 composite에 추가되어 해당 분기가 실행될 때 동시에 실행되며, 특정한 데이터를 제공
 */
/// <summary>
/// BehaviourTree의 Root에 접근하는 객체 / 등록된 행동들을 시작시키는 객체 <br/>
/// AI 행동을 제어하기 위한 컴포넌트
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public abstract class BaseBehaviourController: MonoBehaviour
{
    // 행동 루틴을 나타냄
    private Coroutine behaviourRoutine;

    // 행동의 루트노드를 나타냄
    private BaseRunnableBehaviour rootBehaviour;

    // 센스 객체가 하나만 존재하도록
    protected Dictionary<Type, BaseSense> senseDict = new();

    public List<string> taskStaks;

    // 행동 제어 객체에서 사용할 데이터를 기록하게 될 Dictionary
    public Dictionary<string/*property name*/, object/*property value*/> PropertyDict { get; protected set; } // 자식 클래스에서 초기화

    public NavMeshAgent navMeshAgent { get; protected set; }  // 자식 클래스에서 초기화

    public T RegisterSense<T>() where T : BaseSense, new()
    {
        // Sense 객체 생성
        T senseInst = new();

        // 초기화
        senseInst.OnSenseInitilized(this);

        // 딕셔너리에 넣기
        senseDict[typeof(T)] = senseInst;
        // 리스트에 추가
        //senseList.Add(senseInst);


        return senseInst;
    }

    public T GetSense<T>() where T :BaseSense
    {
        if (senseDict.TryGetValue(typeof(T), out BaseSense value))
        {
            if (value is T matched) return matched;
        }

        return null;
    }
    public void StartBehabiour<T>() where T :  BaseRunnableBehaviour, new()
    {
        behaviourRoutine = StartCoroutine(Run<T>());
        navMeshAgent.enabled = true;

    }

    private IEnumerator Run<T>() where T : BaseRunnableBehaviour, new()
    {
        while (true)
        {
            rootBehaviour = new T();

            // 초기화
            if (rootBehaviour.OnBehaviourInitialize(this))
            {
                // 행동을 시작시키고, 행동이 종료될 때까지 대기
                yield return rootBehaviour.OnBehaviorStarted();

                // 행동 끝
                rootBehaviour.OnBehaviourStopped();
            }
            // 초기화에 실패한 경우
            else yield return null;

            // 행동이 끝났으므로 행동객체 참조 해제
            rootBehaviour = null;
        }
    }

    // 행동을 재시작
    public void RestartBehaviour<T>() where T : BaseRunnableBehaviour, new()
    {
        StopBehaviour();

        if(!enabled) return;
        StartBehabiour<T>();
    }

    // 행동을 중단
    public void StopBehaviour()
    {
        taskStaks?.Clear();

        // 행동 종료
        if(behaviourRoutine is not null)
        {
            // 실행중인 행동이 존재한다면
            if(rootBehaviour is not null)
            {
                // 행동 종료
                rootBehaviour.OnBehaviourStopped();
                rootBehaviour = null;
            }

            StopCoroutine(behaviourRoutine);
            behaviourRoutine = null;
        }
    }

#if UNITY_EDITOR
    private void UpdateTaskStacks()
    {
        taskStaks = new();

        void AddTaskStackTrace(BaseRunnableBehaviour behaviour)
        {
            taskStaks.Add(behaviour.GetType().Name);
        }

        BaseRunnableBehaviour nextBehaviour = rootBehaviour;

        while(nextBehaviour is not null)
        {
            AddTaskStackTrace(nextBehaviour);

            BaseBehaviourComposite thisComposite = nextBehaviour as BaseBehaviourComposite;
            nextBehaviour = thisComposite?.childBehaviour;
        }

    }
#endif

    protected virtual void CALLBACK_Update()
    {
        UpdateSense();
#if UNITY_EDITOR
        UpdateTaskStacks();
#endif
    }

    protected virtual void OnDestroy()
    {
        StopBehaviour();
    }

    private void UpdateSense()
    {
        if (behaviourRoutine is null) return;

        //foreach(var sense in senseList)
        foreach(var sense in senseDict.Values)
        {
            sense.OnSenseUpdated();
        }
    }


#if UNITY_EDITOR
    protected virtual void OnDrawGizmos()
    {
        if(behaviourRoutine is null) return;

        rootBehaviour?.OnDrawGizmos();

        //foreach (var sense in senseList)
        foreach (var sense in senseDict.Values)
        {
            sense.OnDrawGizmos();
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (behaviourRoutine is null) return;

        rootBehaviour?.OnDrawGizmosSelected();

        //foreach (var sense in senseList)
        foreach (var sense in senseDict.Values)
        {
            sense.OnDrawGizmosSelected();
        }
    }
#endif
}
