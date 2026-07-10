using UnityEngine;

// Spherecast 디버깅을 위한 정보를 담는 클래스
public class DebugInfo_Spherecast : RaycastDebugInfo
{
    public float radius;

    public DebugInfo_Spherecast(bool isHit, Vector3 origin, Vector3 end, float hitDistance, float maxDistance, float radius) : base(isHit, origin, end, hitDistance, maxDistance)
    {
        this.radius = radius;
    }

    public override void Draw()
    {
#if UNITY_EDITOR
        Color color = drawColor;
        color.a = 0.3f;
        Gizmos.color = color;
        Vector3 hitPoint = isHit ? origin + direction * hitDistance : end;

        // 시작 위치에 구체 그리기
        Gizmos.DrawWireSphere(origin, radius);

        // 충돌체가 감지된 위치에 구체 그리기
        Gizmos.DrawWireSphere(hitPoint, radius);

        //  시작 위치부터 충돌체가 감지된 위치까지 선 그리기
        Gizmos.DrawLine(origin, hitPoint);
#endif
    }
}
