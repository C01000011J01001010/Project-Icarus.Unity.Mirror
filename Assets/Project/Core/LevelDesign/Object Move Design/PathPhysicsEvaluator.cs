using System.Collections.Generic;
using UnityEngine;

namespace CoreEngine.LevelDesign
{
    /// <summary>
    /// 경로의 속도 및 회전량을 연산하여 적절한 물리 충돌 감지 모드(CollisionDetectionMode)를 판별하는 헬퍼 클래스 (Has-A 관계)
    /// </summary>
    public class PathPhysicsEvaluator
    {
        private readonly float _fastSpeedThreshold;
        private readonly float _fastRotationThreshold;

        public PathPhysicsEvaluator(float fastSpeedThreshold, float fastRotationThreshold)
        {
            _fastSpeedThreshold = fastSpeedThreshold;
            _fastRotationThreshold = fastRotationThreshold;
        }

        /// <summary>
        /// 설정값 및 경로 분석을 통해 최종 CollisionDetectionMode를 결정합니다.
        /// </summary>
        public CollisionDetectionMode EvaluateCollisionMode(
            CollisionDetectionOption option,
            TargetPoint rootPoint,
            List<TargetPoint> points,
            LoopMode loopMode,
            CoordinateTransformer transformer,
            Transform objectTransform,
            bool useRelative)
        {
            if (option == CollisionDetectionOption.ForceDiscrete)
                return CollisionDetectionMode.Discrete;

            if (option == CollisionDetectionOption.ForceContinuousSpeculative)
                return CollisionDetectionMode.ContinuousSpeculative;

            bool isFastPath = CheckIsFastPathExists(rootPoint, points, loopMode, transformer, objectTransform, useRelative);
            return isFastPath ? CollisionDetectionMode.ContinuousSpeculative : CollisionDetectionMode.Discrete;
        }

        /// <summary>
        /// 경로 내에 임계값을 초과하는 고속 이동/회전 구간이 존재하는지 검사합니다.
        /// </summary>
        private bool CheckIsFastPathExists(
            TargetPoint rootPoint,
            List<TargetPoint> points,
            LoopMode loopMode,
            CoordinateTransformer transformer,
            Transform objectTransform,
            bool useRelative)
        {
            if (points == null || points.Count == 0) return false;

            // 1. rootPoint -> points[0] 구간 검사
            Vector3 rootPos = transformer.GetWorldPosition(rootPoint, objectTransform, useRelative);
            Quaternion rootRot = transformer.GetWorldRotation(rootPoint, objectTransform, useRelative);

            Vector3 firstPos = transformer.GetWorldPosition(points[0], objectTransform, useRelative);
            Quaternion firstRot = transformer.GetWorldRotation(points[0], objectTransform, useRelative);

            if (IsSegmentFast(rootPos, firstPos, rootRot, firstRot, points[0].duration))
                return true;

            // 2. 일반 경유지 구간 검사
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector3 p1 = transformer.GetWorldPosition(points[i], objectTransform, useRelative);
                Vector3 p2 = transformer.GetWorldPosition(points[i + 1], objectTransform, useRelative);
                Quaternion r1 = transformer.GetWorldRotation(points[i], objectTransform, useRelative);
                Quaternion r2 = transformer.GetWorldRotation(points[i + 1], objectTransform, useRelative);

                if (IsSegmentFast(p1, p2, r1, r2, points[i + 1].duration))
                    return true;
            }

            // 3. 루프 복귀 구간 검사
            int lastIdx = points.Count - 1;
            Vector3 lastPos = transformer.GetWorldPosition(points[lastIdx], objectTransform, useRelative);
            Quaternion lastRot = transformer.GetWorldRotation(points[lastIdx], objectTransform, useRelative);

            if (loopMode == LoopMode.Restart)
            {
                if (IsSegmentFast(lastPos, rootPos, lastRot, rootRot, rootPoint.duration))
                    return true;
            }
            else if (loopMode == LoopMode.Circular && points.Count > 1)
            {
                if (IsSegmentFast(lastPos, firstPos, lastRot, firstRot, points[lastIdx].duration))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 단일 구간의 선형 속도 및 각속도가 임계값을 초과하는지 체크합니다.
        /// </summary>
        private bool IsSegmentFast(Vector3 startPos, Vector3 endPos, Quaternion startRot, Quaternion endRot, float duration)
        {
            if (duration <= 0.0001f) return true; // 시간이 0에 가까우면 순간이동 수준의 고속으로 판정

            float linearSpeed = Vector3.Distance(startPos, endPos) / duration;
            float angularSpeed = Quaternion.Angle(startRot, endRot) / duration;

            return (linearSpeed >= _fastSpeedThreshold) || (angularSpeed >= _fastRotationThreshold);
        }
    }
}