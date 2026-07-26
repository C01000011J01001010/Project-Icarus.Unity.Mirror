using UnityEngine;

namespace CoreEngine.Environment
{
    /// <summary>
    /// 단일 공간의 외곽 벽면과 내부 효과 구역을 관리하는 마스터 디렉터 컴포넌트.
    /// 자식 오브젝트들을 "Outer"와 "Inner"라는 직관적인 중간 컨테이너 폴더로 구조화합니다.
    /// </summary>
    [ExecuteAlways] // 에디터 모드와 런타임 양쪽 모두에서 본 생명주기 로직을 구동합니다.
    [RequireComponent(typeof(BoxCollider))]
    public class SpaceZoneEffectsDirector : MonoBehaviour
    {
        [Header("🧱 [세션 1] 외곽 투명벽 설정")]
        [HideInInspector][SerializeField] private Vector3 _zoneSize = new Vector3(10f, 10f, 10f);
        [HideInInspector][SerializeField] private bool _showOuterWalls = true;

        [Header("📐 [세션 2] 내부 구역 분할 설정 (바닥 중앙 0,0 기준)")]
        [HideInInspector][SerializeField] private float _zoneA_StartY = 6f;
        [HideInInspector][SerializeField] private float _zoneB_StartXAbs = 3f;
        [HideInInspector][SerializeField] private float _zoneC_EndY = 4f;
        [HideInInspector][SerializeField] private bool _showInnerZones = true;

        // 🌟 요구사항 반영: 하이라키 관리를 위한 중간 부모 폴더 객체명 상수 선언
        public const string OUTER_FOLDER_NAME = "Outer";
        public const string INNER_FOLDER_NAME = "Inner";

        // 말단 실질 객체들의 고유 식별 명칭 규격
        public const string ZONE_A_NAME = "_Zone_A";
        public const string ZONE_B_LEFT_NAME = "_Zone_B_Left";
        public const string ZONE_B_RIGHT_NAME = "_Zone_B_Right";
        public const string ZONE_C_NAME = "_Zone_C";

        #region Properties (캡슐화 및 에디터 이벤트 프로퍼티)
        public Vector3 ZoneSize { get => _zoneSize; set => _zoneSize = value; }

        public bool ShowOuterWalls
        {
            get => _showOuterWalls;
            set { if (_showOuterWalls != value) { _showOuterWalls = value; UpdateOuterVisuals(); } }
        }

        public float ZoneA_StartY { get => _zoneA_StartY; set => _zoneA_StartY = value; }
        public float ZoneB_StartXAbs { get => _zoneB_StartXAbs; set => _zoneB_StartXAbs = value; }
        public float ZoneC_EndY { get => _zoneC_EndY; set => _zoneC_EndY = value; }

        public bool ShowInnerZones
        {
            get => _showInnerZones;
            set { if (_showInnerZones != value) { _showInnerZones = value; UpdateInnerVisuals(); } }
        }
        #endregion

        private void Awake()
        {
            // 1. 디렉터 본체의 충돌체는 항아리를 감지하기 위한 트리거 센서로 무조건 규격화합니다.
            if (TryGetComponent(out BoxCollider mainCollider))
            {
                mainCollider.isTrigger = true;
                mainCollider.size = Vector3.one; // 부모의 scale 값 자체를 영역 두께로 투사합니다.
            }

            // 2. 인게임 빌드 실행 시 기획 작업용 반투명 가이드 큐브들을 자동으로 가려 인게임 리소스를 보호합니다.
            if (Application.isPlaying)
            {
                _showOuterWalls = false;
                _showInnerZones = false;
                UpdateOuterVisuals();
                UpdateInnerVisuals();
            }
        }

        private void Update()
        {
            // 씬 뷰 기즈모 편집에 대응하기 위한 역동기화 처리
            if (!Application.isPlaying && transform.hasChanged)
            {
                if (transform.localScale != _zoneSize)
                {
                    _zoneSize = transform.localScale;
                }
                transform.hasChanged = false;
            }
        }

        /// <summary>
        /// "Outer" 폴더 자식 아래에 있는 모든 외곽 물리 벽면들의 메시 렌더러 가시성을 일괄 On/Off 제어합니다.
        /// </summary>
        public void UpdateOuterVisuals()
        {
            Transform outerFolder = transform.Find(OUTER_FOLDER_NAME);
            if (outerFolder == null) return;

            MeshRenderer[] renderers = outerFolder.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var renderer in renderers) renderer.enabled = _showOuterWalls;
        }

        /// <summary>
        /// "Inner" 폴더 자식 아래에 있는 모든 내부 감지 트리거 구역들의 메시 렌더러 가시성을 일괄 On/Off 제어합니다.
        /// </summary>
        public void UpdateInnerVisuals()
        {
            Transform innerFolder = transform.Find(INNER_FOLDER_NAME);
            if (innerFolder == null) return;

            MeshRenderer[] renderers = innerFolder.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var renderer in renderers) renderer.enabled = _showInnerZones;
        }
    }
}