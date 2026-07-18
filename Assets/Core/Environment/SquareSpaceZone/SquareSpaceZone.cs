using UnityEngine;

namespace Core.Environment
{
    /// <summary>
    /// 환경 효과(Environment Effect)가 적용될 논리적 공간을 정의하는 컴포넌트.
    /// 에디터 타임에서는 외곽 투명벽 생성의 기준이 되며, 런타임에서는 효과를 부여하는 트리거로 작동합니다.
    /// </summary>
    [ExecuteAlways] // 🌟 에디터 씬 뷰에서도 Update()와 Awake()가 실행되도록 강제하는 속성
    [RequireComponent(typeof(BoxCollider))]
    public class SquareSpaceZone : MonoBehaviour
    {
        [Header("공간 데이터")]

        // 인스펙터에서는 숨기지만 데이터는 씬에 저장(직렬화)되도록 [SerializeField] 사용
        [HideInInspector]
        [SerializeField] private Vector3 _zoneSize = new Vector3(10f, 10f, 10f);

        [HideInInspector]
        [SerializeField] private bool _showWallMeshes = true;

        // 자식으로 생성될 외곽 투명벽 묶음 객체의 고유 이름
        public const string WALL_CONTAINER_NAME = "_GeneratedWalls_";

        #region Properties (프로퍼티 캡슐화)

        /// <summary>
        /// 외부 에디터 클래스에서 안전하게 접근하기 위한 공간 크기 프로퍼티
        /// </summary>
        public Vector3 ZoneSize
        {
            get => _zoneSize;
            set => _zoneSize = value;
        }

        /// <summary>
        /// 가이드 메쉬 활성화 여부. 값이 바뀔 때마다 즉시 렌더러를 갱신합니다.
        /// </summary>
        public bool ShowWallMeshes
        {
            get => _showWallMeshes;
            set
            {
                if (_showWallMeshes != value) // 값이 실제로 변했을 때만 실행 (최적화)
                {
                    _showWallMeshes = value;
                    UpdateWallVisuals(); // Setter 내부에서 시각적 업데이트 즉각 수행
                }
            }
        }
        #endregion

        private void Awake()
        {
            // 1. 자기 자신의 콜라이더를 런타임 환경에 맞게 강제 보정
            var mainCollider = GetComponent<BoxCollider>();
            if (mainCollider != null)
            {
                mainCollider.isTrigger = true; // 내부 공간은 물리 충돌이 아닌 감지(Trigger)용
                mainCollider.size = Vector3.one; // 부모의 Scale을 100% 사용하므로 사이즈는 1로 고정
            }

            // 2. 런타임 진입 시 처리 (게임이 실제로 시작되었을 때)
            if (Application.isPlaying)
            {
                // 플레이어가 가이드 벽(하얀 큐브)을 볼 수 없도록 렌더러를 전부 끕니다.
                _showWallMeshes = false;
                UpdateWallVisuals();
            }
        }

        private void Update()
        {
            // 🌟 역동기화(Reverse Sync) 로직
            // 에디터(씬 뷰)에서 기즈모(스케일 툴)를 잡아당겨 크기를 조절했을 때, 
            // 그 변화량을 캐치해서 _zoneSize 변수에 실시간으로 덮어씌웁니다.
            if (!Application.isPlaying && transform.hasChanged)
            {
                if (transform.localScale != _zoneSize)
                {
                    _zoneSize = transform.localScale;
                }
                transform.hasChanged = false; // 플래그 초기화
            }
        }

        /// <summary>
        /// 생성된 6개의 외곽 투명벽을 찾아 MeshRenderer를 켜거나 끕니다.
        /// </summary>
        public void UpdateWallVisuals()
        {
            // 컨테이너 이름으로 자식 객체 검색
            Transform container = transform.Find(WALL_CONTAINER_NAME);
            if (container == null) return;

            // 자식 객체들에 포함된 모든 렌더러를 찾아 On/Off
            MeshRenderer[] renderers = container.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var renderer in renderers)
            {
                renderer.enabled = _showWallMeshes;
            }
        }
    }
}