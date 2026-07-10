using UnityEngine;

// Raycast 디버깅을 위한 정보를 담는 클래스
public class RaycastDebugInfo : DebugInfoBase
{
    // 레이캐스트 최대 거리
    public float maxDistance;

    // 레이캐스트 끝 위치
    public Vector3 end;

    public Vector3 direction => (end - origin).normalized;

    public RaycastDebugInfo(bool isHit, Vector3 origin, Vector3 end, float hitDistance, float maxDistance) : base(isHit, origin, hitDistance)
    {
        this.maxDistance = maxDistance;
        this.end = end;
    }

    public override void Draw()
    {
#if UNITY_EDITOR
        // 그리기 색상 지정
        Gizmos.color = drawColor;

        Vector3 hitPoint = isHit ? origin + direction * hitDistance : end;
        Gizmos.DrawLine(origin, hitPoint);
        //if (isHit)
        //{
        //    // 충돌체가 감지된 위치
        //    Vector3 hitLocation = origin + (direction * hitDistance);

        //    // 충돌체가 감지된 위치까지 선을 그림
        //    Gizmos.DrawLine(origin, hitLocation);
        //}
        //else
        //{
        //    Gizmos.DrawLine(origin, end);
        //}
#endif
    }
}
