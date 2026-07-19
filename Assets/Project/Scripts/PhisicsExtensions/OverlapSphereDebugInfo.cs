using UnityEngine;

public class OverlapSphereDebugInfo : DebugInfoBase
{
    public float radius;

    public OverlapSphereDebugInfo(bool isHit, Vector3 origin, float hitDistance, float radius) : base(isHit, origin, hitDistance)
    {
        this.radius = radius;
    }

    public override void Draw()
    {
#if UNITY_EDITOR
        Color drawColor = this.drawColor;
        drawColor.a = 0.7f;
        Gizmos.color = drawColor;

        Gizmos.DrawWireSphere(origin, radius);
#endif
    }
}