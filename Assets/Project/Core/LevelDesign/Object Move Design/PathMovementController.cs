using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreEngine.LevelDesign
{
    /// <summary>
    /// 에디터 지정 경로를 따라 오브젝트를 순차적/반복적으로 이동시키는 메인 컨트롤러 컴포넌트
    /// </summary>
    public class PathMovementController : MonoBehaviour
    {
        [Header("System Settings")]
        [Tooltip("이동 연산 방식 (Transform 직접 이동 vs Rigidbody 물리 이동)")]
        public UpdateMode updateMode = UpdateMode.Transform;

        [Tooltip("경로 순회 후 반복 모드")]
        public LoopMode loopMode = LoopMode.Restart;

        [Header("Coordinate Settings")]
        [Tooltip("상대좌표 사용 여부. 토글 변경 시 월드 위치는 유지되며 수치만 보정됩니다.")]
        public bool useRelativeCoordinates = false;

        [HideInInspector, SerializeField]
        private bool _lastRelativeState = false;

        [Header("Physics Detection Automation")]
        [Tooltip("충돌 감지 모드 자동 설정 옵션")]
        public CollisionDetectionOption collisionOption = CollisionDetectionOption.Auto;

        [Tooltip("ContinuousSpeculative로 자동 승격될 최고 이동 속도 임계값 (m/s)")]
        [SerializeField] private float _fastSpeedThreshold = 12f;

        [Tooltip("ContinuousSpeculative로 자동 승격될 최고 회전 속도 임계값 (deg/s)")]
        [SerializeField] private float _fastRotationThreshold = 180f;

        [Header("Origin Root Point")]
        [Tooltip("시작 및 복귀 기준점 (오브젝트 트랜스폼을 자동 추적)")]
        public TargetPoint rootPoint = new TargetPoint();

        [Header("Path Points")]
        [Tooltip("순회할 경유 지점 리스트")]
        public List<TargetPoint> points = new List<TargetPoint>();

        // --- Has-A Helper Classes ---
        private readonly CoordinateTransformer _coordTransformer = new CoordinateTransformer();
        private PathPhysicsEvaluator _physicsEvaluator;

        private Rigidbody _rb;

        private void Awake()
        {
            // 런타임 시작 시 RootPoint를 현재 Transform과 동기화
            SyncRootPointWithTransform();

            if (updateMode == UpdateMode.Rigidbody)
            {
                InitRigidbodyPhysics();
            }
        }

        private void Start()
        {
            if (points != null && points.Count > 0)
            {
                StartCoroutine(MoveSequenceRoutine());
            }
        }

        #region Physics Initialization
        /// <summary>
        /// Rigidbody 세팅 및 지능형 CollisionDetectionMode 자동 설정을 수행합니다.
        /// </summary>
        private void InitRigidbodyPhysics()
        {
            _rb = gameObject.GetOrAddComponent<Rigidbody>();
            _rb.isKinematic = true;
            _rb.useGravity = false;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;

            _physicsEvaluator = new PathPhysicsEvaluator(_fastSpeedThreshold, _fastRotationThreshold);
            _rb.collisionDetectionMode = _physicsEvaluator.EvaluateCollisionMode(
                collisionOption, rootPoint, points, loopMode, _coordTransformer, transform, useRelativeCoordinates
            );
        }
        #endregion

        #region Movement Sequence Routines
        /// <summary>
        /// 리스트에 등록된 목표 지점들을 순차적 및 LoopMode 조건에 맞춰 이동하는 시퀀스 코루틴
        /// </summary>
        private IEnumerator MoveSequenceRoutine()
        {
            int currentIndex = 0;
            int pingPongDirection = 1;

            while (true)
            {
                if (points.Count == 0) yield break;

                TargetPoint target = points[currentIndex];
                Vector3 targetWorldPos = GetWorldPosition(target);
                Quaternion targetWorldRot = GetWorldRotation(target);

                // 1. 해당 지점으로 이동
                yield return StartCoroutine(MoveToPointRoutine(targetWorldPos, targetWorldRot, target.duration, target.moveMode, target.customCurve));

                // 2. 도착 이벤트 실행
                target.onPointReached?.Invoke();

                // 3. 대기 시간 적용
                if (target.waitTime > 0f)
                    yield return new WaitForSeconds(target.waitTime);

                // 4. 다음 지점 인덱스 연산 (LoopMode에 따른 분기)
                if (loopMode == LoopMode.PingPong)
                {
                    if (currentIndex == points.Count - 1) pingPongDirection = -1;
                    else if (currentIndex == 0 && pingPongDirection == -1) pingPongDirection = 1;

                    currentIndex += pingPongDirection;
                }
                else
                {
                    currentIndex++;
                    if (currentIndex >= points.Count)
                    {
                        if (loopMode == LoopMode.None)
                            yield break;

                        if (loopMode == LoopMode.Restart)
                        {
                            // rootPoint(시작 원점) 위치로 복귀 (returnDuration 시간 동안)
                            Vector3 rootWorldPos = GetWorldPosition(rootPoint);
                            Quaternion rootWorldRot = GetWorldRotation(rootPoint);
                            yield return StartCoroutine(MoveToPointRoutine(rootWorldPos, rootWorldRot, rootPoint.duration, MovementMode.EaseInOut, null));

                            currentIndex = 0;
                        }
                        else if (loopMode == LoopMode.Circular)
                        {
                            currentIndex = 0;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 단일 목표 지점까지 지정된 보간 공식 및 시간에 맞춰 이동하는 단위 코루틴
        /// </summary>
        private IEnumerator MoveToPointRoutine(Vector3 endPos, Quaternion endRot, float duration, MovementMode mode, AnimationCurve curve)
        {
            Vector3 startPos = updateMode == UpdateMode.Rigidbody ? _rb.position : transform.position;
            Quaternion startRot = updateMode == UpdateMode.Rigidbody ? _rb.rotation : transform.rotation;

            float t = 0f;
            while (t < duration)
            {
                t += updateMode == UpdateMode.Rigidbody ? Time.fixedDeltaTime : Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(t / duration);
                float curveValue = GetCurveValue(normalizedTime, mode, curve);

                Vector3 nextPos = Vector3.Lerp(startPos, endPos, curveValue);
                Quaternion nextRot = Quaternion.Slerp(startRot, endRot, curveValue);

                if (updateMode == UpdateMode.Rigidbody && _rb != null)
                {
                    _rb.MovePosition(nextPos);
                    _rb.MoveRotation(nextRot);
                    yield return new WaitForFixedUpdate();
                }
                else
                {
                    transform.position = nextPos;
                    transform.rotation = nextRot;
                    yield return null;
                }
            }
        }

        /// <summary>
        /// MovementMode에 따른 보간 비율(0~1) 계산
        /// </summary>
        private float GetCurveValue(float t, MovementMode mode, AnimationCurve curve)
        {
            switch (mode)
            {
                case MovementMode.EaseInOut: return Mathf.SmoothStep(0f, 1f, t);
                case MovementMode.EaseOut: return 1f - Mathf.Pow(1f - t, 2f);
                case MovementMode.CustomCurve: return curve != null ? curve.Evaluate(t) : t;
                case MovementMode.Linear:
                default: return t;
            }
        }
        #endregion

        #region Coordinate & Helper Wrapper Methods
#if UNITY_EDITOR
        private void Reset()
        {
            SyncRootPointWithTransform();

            if (points == null) points = new List<TargetPoint>();
            if (points.Count == 0)
            {
                TargetPoint defaultPt = new TargetPoint
                {
                    duration = 1f
                };
                SetWorldPositionAndRotation(defaultPt, transform.position + transform.forward * 3f, transform.rotation);
                points.Add(defaultPt);
            }
        }
#endif

        private void OnValidate()
        {
            // 1. 상대/절대 좌표계 변경 시 기존 월드위치 유지 보정
            if (_lastRelativeState != useRelativeCoordinates)
            {
                _coordTransformer.ConvertCoordinates(rootPoint, points, transform, useRelativeCoordinates);
                _lastRelativeState = useRelativeCoordinates;

            }

            // 3. 기획자 수치 입력 오류 검증 (duration <= 0, waitTime < 0 방지)
            ValidateInputValues();
        }

        /// <summary>
        /// 입력값의 유효성을 검사하여 0 나누기 오류 및 음수 시간을 방지합니다.
        /// </summary>
        private void ValidateInputValues()
        {
            if (rootPoint != null)
            {
                if (rootPoint.duration <= 0f) rootPoint.duration = 0.1f;
                if (rootPoint.waitTime < 0f) rootPoint.waitTime = 0f;
            }

            if (points != null)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    if (points[i].duration <= 0f) points[i].duration = 0.1f;
                    if (points[i].waitTime < 0f) points[i].waitTime = 0f;
                }
            }
        }

        /// <summary>
        /// RootPoint 위치/회전을 현재 Transform 상태와 동기화합니다.
        /// </summary>
        public void SyncRootPointWithTransform()
        {
            if (rootPoint == null) rootPoint = new TargetPoint();
            SetWorldPositionAndRotation(rootPoint, transform.position, transform.rotation);
        }

        public Vector3 GetWorldPosition(TargetPoint pt)
        {
            return _coordTransformer.GetWorldPosition(pt, transform, useRelativeCoordinates);
        }

        public Quaternion GetWorldRotation(TargetPoint pt)
        {
            return _coordTransformer.GetWorldRotation(pt, transform, useRelativeCoordinates);
        }

        public void SetWorldPositionAndRotation(TargetPoint pt, Vector3 wPos, Quaternion wRot)
        {
            _coordTransformer.SetWorldPositionAndRotation(pt, wPos, wRot, transform, useRelativeCoordinates);
        }
        #endregion
    }
}