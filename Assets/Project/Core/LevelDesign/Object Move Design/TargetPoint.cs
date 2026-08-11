using System;
using UnityEngine;
using UnityEngine.Events;

namespace CoreEngine.LevelDesign
{
    /// <summary>
    /// 오브젝트의 이동 보간 방식
    /// </summary>
    public enum MovementMode
    {
        Linear,
        EaseInOut,
        EaseOut,
        CustomCurve
    }

    /// <summary>
    /// 경로 순회 후 반복 이동 모드
    /// </summary>
    public enum LoopMode
    {
        None,
        Restart,
        PingPong,
        Circular
    }

    /// <summary>
    /// 오브젝트 이동 적용 방식 (Transform 직접 이동 vs Rigidbody 물리 이동)
    /// </summary>
    public enum UpdateMode
    {
        Transform,
        Rigidbody
    }

    /// <summary>
    /// Rigidbody 충돌 감지 모드 설정 옵션
    /// </summary>
    public enum CollisionDetectionOption
    {
        Auto,
        ForceDiscrete,
        ForceContinuousSpeculative
    }

    /// <summary>
    /// 경로 상의 단일 목표 지점 데이터 구조체
    /// </summary>
    [Serializable]
    public class TargetPoint
    {
        [Tooltip("목표 위치 (월드/상대 좌표)")]
        public Vector3 position;

        [Tooltip("목표 회전값 (오일러 각도)")]
        public Vector3 rotation;

        [Tooltip("해당 지점까지 이동하는 데 걸리는 시간 (t sec)")]
        public float duration = 1f;

        [Tooltip("도착 후 대기 시간 (sec)")]
        public float waitTime = 0f;

        [Tooltip("이동 보간 모드")]
        public MovementMode moveMode = MovementMode.EaseInOut;

        [Tooltip("CustomCurve 모드일 때 사용할 커브")]
        public AnimationCurve customCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Tooltip("해당 지점에 도달했을 때 실행할 이벤트")]
        public UnityEvent onPointReached;
    }
}