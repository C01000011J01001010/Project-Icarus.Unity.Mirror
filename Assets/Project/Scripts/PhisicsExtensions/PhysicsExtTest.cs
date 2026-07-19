using UnityEngine;

public class PhysicsExtTest : MonoBehaviour
{
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        RaycastHit hitInfo;

        PhysicsExtensions.Raycast(
            out var rayDebugInfo,
            new Ray(transform.position, transform.forward),
            out hitInfo,
            5.0f,
            1
            );
        rayDebugInfo?.Draw();

        PhysicsExtensions.SphereCast(
            out var sphereDebugInfo,
            transform.position,
            0.5f,
            transform.forward,
            out hitInfo,
            5.0f,
            1);
        sphereDebugInfo?.Draw();
    }
#endif
}
