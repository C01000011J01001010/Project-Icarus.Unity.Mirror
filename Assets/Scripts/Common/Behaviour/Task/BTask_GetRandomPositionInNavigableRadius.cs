using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class BTask_GetRandomPositionInNavigableRadius : BaseRunnableBehaviour
{
    
    private string KEY_TargetPosition; // 뽑은 랜덤한 위치를 저장할 키
    private string KEY_OriginPosition; // 중심 위치를 저장할 키
    private float maxMoveDistance; //최대 거리를 나타내기 위한 필드

    public BTask_GetRandomPositionInNavigableRadius(string KEY_TargetPosition, string KEY_OriginPosition, float maxMoveDistance)
    {
        this.KEY_TargetPosition = KEY_TargetPosition;
        this.KEY_OriginPosition = KEY_OriginPosition;
        this.maxMoveDistance = maxMoveDistance;
    }

    public override IEnumerator OnBehaviorStarted()
    {
        // 중심 위치
        Vector3 origin = (Vector3)behaviourController.PropertyDict[KEY_OriginPosition];

        // 목표 위치를 설정
        if(TryGetRandomPositionInNavMesh(origin, maxMoveDistance, out Vector3 targetPosition))
        {
            isSucceeded = true;
            behaviourController.PropertyDict[KEY_TargetPosition] = targetPosition;
        }
        else
        {
            isSucceeded = false;
        }

        yield return null;
    }

    private bool TryGetRandomPositionInNavMesh(Vector3 origin, float range, out Vector3 result)
    {
        // 최대 30번
        for(int i =0; i < 30; i++)
        {
            // 랜덤한 거리
            float distance = Random.Range(0.0f, maxMoveDistance);

            // 랜덤한 방향
            Vector3 direction = Random.insideUnitSphere;

            Vector3 pos = origin + distance * direction;
            if(NavMesh.SamplePosition(pos, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }

        }

        result = origin;
        return false;
    }
}
