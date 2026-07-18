using UnityEngine;

namespace Core.Environment
{
    /// <summary>
    /// 부모 공간의 내부를 A(상단), B(좌/우 외곽), C(하단) 구역으로 정밀 분할하는 컴포넌트입니다.
    /// 중앙(Center)은 비워두고, B구역이 양쪽 벽에 붙어 Y축 대칭을 이루도록 구성됩니다.
    /// </summary>
    [ExecuteAlways]
    public class InnerSpaceZoneGenerator : MonoBehaviour
    {
        [Header("📐 내부 공간 분할 설정 (기준점: 바닥 중앙 0,0)")]
        [HideInInspector][SerializeField] private float _zoneA_StartY = 6f;
        [HideInInspector][SerializeField] private float _zoneB_StartXAbs = 3f;
        [HideInInspector][SerializeField] private float _zoneC_EndY = 4f;

        [Header("⚙️ 시각화 옵션")]
        [HideInInspector][SerializeField] private bool _showZoneMeshes = true;

        // 생성된 자식 객체들을 일괄 관리하기 위한 고유 이름 (B구역이 좌우로 나뉨)
        public const string ZONE_A_NAME = "_Generated_Zone_A";
        public const string ZONE_B_LEFT_NAME = "_Generated_Zone_B_Left";
        public const string ZONE_B_RIGHT_NAME = "_Generated_Zone_B_Right";
        public const string ZONE_C_NAME = "_Generated_Zone_C";

        #region Properties
        public float ZoneA_StartY { get => _zoneA_StartY; set => _zoneA_StartY = value; }

        /// <summary>
        /// 중앙(X=0)부터 비워둘 공간의 너비(절대값)입니다. 이 값부터 외곽 끝까지 B구역이 채워집니다.
        /// </summary>
        public float ZoneB_StartXAbs { get => _zoneB_StartXAbs; set => _zoneB_StartXAbs = value; }

        public float ZoneC_EndY { get => _zoneC_EndY; set => _zoneC_EndY = value; }

        public bool ShowZoneMeshes
        {
            get => _showZoneMeshes;
            set
            {
                if (_showZoneMeshes != value)
                {
                    _showZoneMeshes = value;
                    _showWallMeshesAndTriggerSettings();
                }
            }
        }
        #endregion

        private void Awake()
        {
            if (Application.isPlaying)
            {
                _showZoneMeshes = false;
                _showWallMeshesAndTriggerSettings();
            }
        }

        private void Update()
        {
            if (!Application.isPlaying && transform.hasChanged)
            {
                transform.hasChanged = false;
            }
        }

        private void _showWallMeshesAndTriggerSettings()
        {
            // 좌, 우 2개로 나뉜 B구역 이름 모두 포함
            string[] zoneNames = { ZONE_A_NAME, ZONE_B_LEFT_NAME, ZONE_B_RIGHT_NAME, ZONE_C_NAME };
            foreach (string zoneName in zoneNames)
            {
                Transform zoneTarget = transform.Find(zoneName);
                if (zoneTarget != null && zoneTarget.TryGetComponent(out MeshRenderer mr))
                {
                    mr.enabled = _showZoneMeshes;
                }
            }
        }
    }
}