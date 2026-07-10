using UnityEngine;

public static class PhysicsExtensions
{
    public static bool Raycast(
        out RaycastDebugInfo debugInfo,
        Ray ray,
        out RaycastHit hitInfo,
        float maxDistance,
        int layerMask,
        QueryTriggerInteraction QTI = QueryTriggerInteraction.Ignore
        )
    {
        
        bool result = Physics.Raycast(ray, out hitInfo, maxDistance, layerMask, QTI);
        debugInfo = new RaycastDebugInfo(
            result,
            ray.origin,
            ray.origin + ray.direction * maxDistance,
            result ? hitInfo.distance : maxDistance,
            maxDistance);

        return result;
    }

    public static bool SphereCast(
        out DebugInfo_Spherecast debugInfo,
        Vector3 origin,
        float radius,
        Vector3 direction,
        out RaycastHit hitInfo,
        float maxDistance,
        int layerMask,
        QueryTriggerInteraction QTI = QueryTriggerInteraction.Ignore)
    {
        bool result = Physics.SphereCast(origin, radius, direction, out hitInfo, maxDistance, layerMask, QTI);

        debugInfo = new DebugInfo_Spherecast(
            result,
            origin,
            origin + direction * maxDistance, //( result ? hitInfo.distance : maxDistance),
            result ? hitInfo.distance : maxDistance,
            maxDistance,
            radius);

        return result;
    }

    public static Collider[] OverlapSphere(out OverlapSphereDebugInfo debugInfo, Vector3 center, float radius, int layer, QueryTriggerInteraction QTI = QueryTriggerInteraction.Ignore)
    {
        Collider[] detectedColliders = Physics.OverlapSphere(center, radius, layer, QTI);

        debugInfo = new OverlapSphereDebugInfo(detectedColliders.Length > 0, center, 0.0f, radius);

        return detectedColliders;
    }
}
