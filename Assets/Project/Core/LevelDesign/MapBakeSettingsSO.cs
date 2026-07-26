using System.Collections.Generic;
using UnityEngine;

namespace CoreEngine.LevelDesign
{
    public enum MapDimension
    {
        _2D,
        _3D,
    }

    public enum MapProjectionPlane
    {
        XZ, // Top-Down (X, Z 평면)
        XY  // Side/Front (X, Y 평면)
    }

    public enum MapDepthSteps
    {
        None,
        Step_2 = 2,
        Step_4 = 4,
        Step_8 = 8,
        Step_12 = 12,
        Step_16 = 16,
        Step_24 = 24,
        Step_32 = 32,
        //Step_48 = 48,
        //Step_64 = 64,
        //Step_92 = 92,
        //Step_128 = 128,
        //Step_256 = 256,
        //Step_512 = 512,
        //Step_1024 = 1024,
    }

    public enum MapResolution
    {
        Res_512 = 512,
        Res_1024 = 1024,
        Res_2048 = 2048,
        Res_4096 = 4096
    }

    [System.Serializable]
    public struct LayerColorPair
    {
        public string layerName;
        public Color color;
        public bool useOutline;
    }

    [System.Serializable]
    public class LayerOutlineSetting
    {
        // 유니티 202X 이상에서는 Layer 팝업을 위해 별도 Drawer를 쓰거나,
        // [Layer] 어트리뷰트를 만들어 드롭다운으로 노출시키는 것이 일반적입니다.
        public string layerName;
        public Color outlineColor = Color.black;
        [Range(1, 5)] public int outlineThickness = 2;

        // bool을 지우고 LayerMask로 교체 (기본값은 Everything)
        [Tooltip("이 레이어를 가려서 외곽선을 자를(Occlude) 장애물 레이어들")]
        public LayerMask occluderMask = -1;

        // 내 외곽선 위를 덮고 있어서, 외곽선을 가려줄 레이어들
        [Tooltip("이 레이어보다 앞에 있어서 외곽선을 덮어 가려줄 레이어들")]
        public LayerMask coverMask = 0;
    }

    [CreateAssetMenu(fileName = "NewMapBakeSettings", menuName = MenuNamesSO.DefaultMenu + "/LevelDesign/Map Bake Settings")]
    public class MapBakeSettingsSO : ScriptableObject
    {

        [Header("Game Dimension")]
        [SerializeField, HideInInspector] private MapDimension _lastMapDimension;
        public MapDimension mapDimension = MapDimension._3D;

        [Header("Projection & Depth Settings")]
        public MapProjectionPlane projectionPlane = MapProjectionPlane.XZ;
        public MapDepthSteps depthSteps = MapDepthSteps.Step_8;

        [Header("Global Settings")]
        public Vector3 centerPosition;
        public Vector2 totalMapSize = new Vector2(3000, 3000);
        public float captureOffset = 500f;
        public float maxDepth = 1000f;

        [Header("Tile Settings (Grid)")]
        public Vector2 tileSize = new Vector2(1024, 1024);

        [Header("Render Settings")]
        public MapResolution resolution = MapResolution.Res_1024;
        public LayerMask renderMask = -1;
        public Color backgroundColor = Color.black;
        public bool useLayerColor;

        public List<LayerColorPair> layerColors = new List<LayerColorPair>();

        [Header("Outline Settings")]
        public List<LayerOutlineSetting> outlineSettings = new List<LayerOutlineSetting>();

        public int Cols => tileSize.x > 0 ? Mathf.CeilToInt(totalMapSize.x / tileSize.x) : 1;
        public int Rows => tileSize.y > 0 ? Mathf.CeilToInt(totalMapSize.y / tileSize.y) : 1;

        private void Awake()
        {
            _lastMapDimension = mapDimension;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            DimensionPreset();
            PreventDuplication();
        }

        public void DimensionPreset()
        {
            // 차원 변경시 기본값 제공
            if (mapDimension != _lastMapDimension)
            {
                // 캐싱
                _lastMapDimension = mapDimension;

                // 2D
                if (mapDimension == MapDimension._2D)
                {
                    projectionPlane = MapProjectionPlane.XY;
                    depthSteps = MapDepthSteps.None;
                    useLayerColor = true;
                }

                // 아마 2d 공간?
                if (mapDimension == MapDimension._3D)
                {
                    projectionPlane = MapProjectionPlane.XZ;
                    depthSteps = MapDepthSteps.Step_8;
                    useLayerColor = true;
                }
            }
        }
        public void PreventDuplication()
        {
            if (outlineSettings == null || outlineSettings.Count <= 1) return;

            HashSet<string> usedLayers = new HashSet<string>();

            // 역순으로 검사하여 중복된 항목을 처리
            for (int i = outlineSettings.Count - 1; i >= 0; i--)
            {
                string currentLayer = outlineSettings[i].layerName;

                // 이름이 비어있으면 일단 패스 (유저가 아직 선택 안 한 상태)
                if (string.IsNullOrEmpty(currentLayer)) continue;

                // 이미 등록된 레이어라면 중복!
                if (usedLayers.Contains(currentLayer))
                {
                    Debug.LogWarning($"[MapBaker] '{currentLayer}' 레이어의 외곽선 설정이 이미 존재합니다. 중복을 방지하기 위해 초기화됩니다.");

                    // 중복된 항목의 레이어 이름을 비워버리거나, 리스트에서 아예 제거할 수 있습니다.
                    // 여기서는 이름을 비워서 유저가 다시 선택하게 유도합니다.
                    outlineSettings[i].layerName = "";
                }
                else
                {
                    usedLayers.Add(currentLayer);
                }
            }

            // 알파 0 방지 안전장치
            for (int i = 0; i < outlineSettings.Count; i++)
            {
                if (outlineSettings[i].outlineColor.a <= 0.01f) // 알파가 0에 가깝다면
                {
                    Color correctedColor = outlineSettings[i].outlineColor;
                    correctedColor.a = 1f; // 알파를 강제로 1(100%)로 보정!
                    outlineSettings[i].outlineColor = correctedColor;
                }
            }
        }
    }
#endif
}