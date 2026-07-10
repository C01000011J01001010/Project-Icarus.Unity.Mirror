using UnityEngine;

// PhysicsExtensions 에서 디버깅을 위해 기본적인 정보를 담는 클래스
public abstract class DebugInfoBase
{
    // 충돌체를 감지하지 않았을 경우 표시될 기본 색상
    public Color defaultColor = Color.green;

    // 충돌체를 감지한 경우 표시될 색상
    public Color detectedColor = Color.red;

    // 충돌체 감지 여부
    public bool isHit;

    // 충돌체 감지를 위해 기준이 되는 위치
    public Vector3 origin;

    //충돌체가 감지된 거리
    public float hitDistance;

    // 표시할 색상
    protected Color drawColor => isHit ? detectedColor : defaultColor;

    public DebugInfoBase(bool isHit, Vector3 origin, float hitDistance)
    {
        this.isHit = isHit;
        this.origin = origin;
        this.hitDistance = hitDistance;
    }

    // 화면에 그리기
    public abstract void Draw();
}
