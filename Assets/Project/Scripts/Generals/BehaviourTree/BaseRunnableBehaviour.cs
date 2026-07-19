using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Task
// Behaviour controller를 통해 실행될 수 있는 기본적인 형태
public abstract class BaseRunnableBehaviour
{
    // 행동 제어 객체
    public BaseBehaviourController behaviourController { get; private set; }

    // 이 행동의 실행 성공 여부
    public bool isSucceeded { get; protected set; } = true;

    // 사용할 Service를 생성하는 함수를 저장하는 리스트 -> 실제 사용할때 객체를 만듬
    private List<System.Func<BaseBehaviourService>> serviceGeneraterList = new();

    // 실제 Service 객체들이 저장될 리스트
    private List<BaseBehaviourService> serviceInstanceList;

    private Coroutine serviceRoutine;

    // 이 행동에 대한 기본 내용을 초기화하기 위해 사용
    public virtual bool OnBehaviourInitialize(BaseBehaviourController behaviourController)
    {
        this.behaviourController = behaviourController;

        // 만들 서비스객체가 있다면
        if(serviceGeneraterList.Count > 0 )
        {
            serviceInstanceList = new();

            // 서비스 객체를 만들어서 리스트로 관리
            foreach(var getService in serviceGeneraterList)
            {
                // 생성 후 초기화
                BaseBehaviourService service = getService();
                service.Initialize(behaviourController);

                // 리스트에 삽입
                serviceInstanceList.Add(service);
            }

            // 생성된 모든 서비스에 대한 루틴 시작
            serviceRoutine = behaviourController.StartCoroutine(RunService());
        }

        // 행동 초기화 성공
        return true;
    }

    public virtual IEnumerator RunService()
    {
        while(serviceInstanceList is not null)
        {
            foreach(BaseBehaviourService service in serviceInstanceList)
            {
                service.Tick();
                yield return null;
            }
        }
    }

    // 행동을 구현하기 위한 메서드
    // 하위 클래스에서 메서드를 구현하여 동작 방식을 정의함
    public abstract IEnumerator OnBehaviorStarted();

    // 행동이 중단되었을 때 호출되는 메서드
    public virtual void OnBehaviourStopped() 
    {
        if(serviceRoutine is not null)
        {
            behaviourController.StopCoroutine(serviceRoutine);
            serviceRoutine = null;

            foreach(BaseBehaviourService service in serviceInstanceList)
            {
                service.OnServiceFinish();
            }

            serviceInstanceList.Clear();
        }
    }

    public void RegisterService(System.Func<BaseBehaviourService> getService)
    {
        serviceGeneraterList.Add(getService);
    }

    public virtual void OnDrawGizmos()
    {
        if(serviceInstanceList is not null)
        {
            foreach(var service in serviceInstanceList)
            {
                service.OnDrawGizmos();
            }
        }
    }

    public virtual void OnDrawGizmosSelected()
    {
        if (serviceInstanceList is not null)
        {
            foreach (var service in serviceInstanceList)
            {
                service.OnDrawGizmosSelected();
            }
        }
    }
}

