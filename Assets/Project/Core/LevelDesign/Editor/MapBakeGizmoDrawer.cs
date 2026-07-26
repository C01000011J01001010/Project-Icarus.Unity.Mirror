using UnityEditor;
using UnityEngine;

namespace CoreEngine.LevelDesign
{
    public static class MapBakeGizmoDrawer
    {
        // 1. 공용 수학 연산 로직 (좌표와 크기만 계산해서 넘겨줌)
        private static void CalculateDrawData(MapBakeSettingsSO settings, out Vector3 boundsCenter, out Vector3 boundsSize, out Vector3 tileCenter, out Vector3 tileSize, out Vector3 camPos, out Vector3 rayDir)
        {
            bool isXY = settings.projectionPlane == MapProjectionPlane.XY;

            boundsCenter = isXY
                ? settings.centerPosition + Vector3.forward * (settings.maxDepth * 0.5f - settings.captureOffset)
                : settings.centerPosition + Vector3.down * (settings.maxDepth * 0.5f - settings.captureOffset);
            boundsSize = isXY
                ? new Vector3(settings.totalMapSize.x, settings.totalMapSize.y, settings.maxDepth)
                : new Vector3(settings.totalMapSize.x, settings.maxDepth, settings.totalMapSize.y);

            tileCenter = isXY
                ? settings.centerPosition + Vector3.forward * (settings.maxDepth * 0.5f - settings.captureOffset)
                : settings.centerPosition + Vector3.down * (settings.maxDepth * 0.5f - settings.captureOffset);
            tileSize = isXY
                ? new Vector3(settings.tileSize.x, settings.tileSize.y, settings.maxDepth)
                : new Vector3(settings.tileSize.x, settings.maxDepth, settings.tileSize.y);

            camPos = isXY
                ? new Vector3(settings.centerPosition.x, settings.centerPosition.y, settings.centerPosition.z - settings.captureOffset)
                : new Vector3(settings.centerPosition.x, settings.centerPosition.y + settings.captureOffset, settings.centerPosition.z);
            rayDir = isXY ? Vector3.forward : Vector3.down;
        }

        public static void DrawWithHandles(MapBakeSettingsSO settings)
        {
            CalculateDrawData(settings, out var bc, out var bs, out var tc, out var ts, out var cp, out var rd);

            Handles.color = Color.red;
            Handles.DrawWireCube(bc, bs);

            Handles.color = Color.yellow;
            Handles.DrawWireCube(tc, ts);

            Handles.color = Color.cyan;
            // Handles는 Repaint 이벤트에서만 렌더링해야 안전합니다.
            if (Event.current.type == EventType.Repaint)
            {
                Handles.SphereHandleCap(0, cp, Quaternion.identity, 2f, EventType.Repaint);
            }
            Handles.DrawLine(cp, cp + rd * settings.maxDepth);
        }
    }
}
