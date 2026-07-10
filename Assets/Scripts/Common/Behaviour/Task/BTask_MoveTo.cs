using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BTask_MoveTo : BaseRunnableBehaviour
{
    // 목적지를 저장하는 키
    private string Key_TargetPosition;

    public BTask_MoveTo(string Key_TargetPosition)
    {
        this.Key_TargetPosition = Key_TargetPosition;
    }

    public override IEnumerator OnBehaviorStarted()
    {
        // 길찾기를 위한 NavMeshAgent 컴포넌트
        NavMeshAgent navMeshAgent = behaviourController.navMeshAgent;

        // 목적지를 얻음
        Vector3 destination = (Vector3)behaviourController.PropertyDict[Key_TargetPosition];

        // navMeshAgent 가 비활성화 상태인 경우
        if (!navMeshAgent.enabled)
        {
            navMeshAgent.enabled = true;
        }

        // 목적지로 이동을 시작
        navMeshAgent.SetDestination(destination);

        // 현재위치에서 움직이지 않으면 종료
        //Vector3 prevPosition = destination;
        //Vector3 currentPosition;
        //while (true)
        //{
        //    currentPosition = BaseBehaviourController.transform.position;

        //    // 목적지에 닿았다면 종료해도 되고, 이전 상태에서 변하지 않는다면 종료해야함
        //    if ((prevPosition - currentPosition).sqrMagnitude <= float.Epsilon) break;
        //    else
        //    {
        //        prevPosition = currentPosition;
        //        yield return new WaitForSeconds(1.0f);
        //    }
        //}

        // 현재위치에서 움직이지 않으면 종료
        Vector3 prevPosition, currentPosition = behaviourController.transform.position;
        do
        {
            // 이전위치 갱신
            prevPosition = currentPosition;
        
            yield return new WaitForSeconds(0.1f);
        
            // 현재 위치 갱신
            currentPosition = behaviourController.transform.position;
        } while (Vector3.Distance(currentPosition, prevPosition) > float.Epsilon);


        isSucceeded = true;

        yield return null;
    }
}