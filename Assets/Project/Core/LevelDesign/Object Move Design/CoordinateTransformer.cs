using System.Collections.Generic;
using UnityEngine;

namespace CoreEngine.LevelDesign
{
    /// <summary>
    /// TargetPoint의 월드/상대 좌표 변환 및 영역 연산을 전담하는 헬퍼 클래스 (Has-A 관계)
    /// </summary>
    public class CoordinateTransformer
    {

        /// <summary>
        /// 상대좌표 <-> 절대좌표 상태 변경 시 기존 물리적 월드 위치를 유지하도록 데이터를 재계산합니다.
        /// </summary>
        public void ConvertCoordinates(TargetPoint rootPoint, List<TargetPoint> points, Transform objectTransform, bool toRelative)
        {
            Transform refTransform = objectTransform.parent;
            if (refTransform == null) return;

            if (rootPoint != null)
                ConvertSinglePoint(rootPoint, refTransform, toRelative);

            if (points != null)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    ConvertSinglePoint(points[i], refTransform, toRelative);
                }
            }
        }

        /// <summary>
        /// 단일 TargetPoint의 좌표계를 변환합니다.
        /// </summary>
        private void ConvertSinglePoint(TargetPoint pt, Transform refTransform, bool toRelative)
        {
            if (pt == null) return;

            if (toRelative)
            {
                pt.position = refTransform.InverseTransformPoint(pt.position);
                pt.rotation = (Quaternion.Inverse(refTransform.rotation) * Quaternion.Euler(pt.rotation)).eulerAngles;
            }
            else
            {
                pt.position = refTransform.TransformPoint(pt.position);
                pt.rotation = (refTransform.rotation * Quaternion.Euler(pt.rotation)).eulerAngles;
            }
        }

        /// <summary>
        /// TargetPoint의 실제 월드 위치를 반환합니다.
        /// </summary>
        public Vector3 GetWorldPosition(TargetPoint pt, Transform objectTransform, bool useRelative)
        {
            if (pt == null) return objectTransform.position;

            Transform refTransform = objectTransform.parent;
            if (useRelative && refTransform != null)
            {
                return refTransform.TransformPoint(pt.position);
            }
            return pt.position;
        }

        /// <summary>
        /// TargetPoint의 실제 월드 회전(Quaternion)을 반환합니다.
        /// </summary>
        public Quaternion GetWorldRotation(TargetPoint pt, Transform objectTransform, bool useRelative)
        {
            if (pt == null) return objectTransform.rotation;

            Transform refTransform = objectTransform.parent;
            if (useRelative && refTransform != null)
            {
                return refTransform.rotation * Quaternion.Euler(pt.rotation);
            }
            return Quaternion.Euler(pt.rotation);
        }

        /// <summary>
        /// 월드 위치/회전 값을 받아 TargetPoint 데이터에 반영합니다.
        /// </summary>
        public void SetWorldPositionAndRotation(TargetPoint pt, Vector3 wPos, Quaternion wRot, Transform objectTransform, bool useRelative)
        {
            if (pt == null) return;

            Transform refTransform = objectTransform.parent;
            if (useRelative && refTransform != null)
            {
                pt.position = refTransform.InverseTransformPoint(wPos);
                pt.rotation = (Quaternion.Inverse(refTransform.rotation) * wRot).eulerAngles;
            }
            else
            {
                pt.position = wPos;
                pt.rotation = wRot.eulerAngles;
            }
        }
    }
}