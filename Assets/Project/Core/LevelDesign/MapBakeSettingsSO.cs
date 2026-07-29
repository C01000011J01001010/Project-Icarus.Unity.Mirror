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
        None = 0,    // *요청 반영: 깊이에 따른 명도 양자화 하지 않고 씬 내 원본 매터리얼/이미지 그대로 사용
        Step_1 = 1,  // *요청 반영: 깊이에 따른 명도 양자화 없이 단색(Layer 색상)으로 렌더링
        Step_2 = 2,  // *요청 반영: 깊이에 따른 명도 양자화 시작 (2단계)
        Step_4 = 4,
        Step_8 = 8,
        Step_12 = 12,
        Step_16 = 16,
        Step_24 = 24,
        Step_32 = 32,
        Step_48 = 48,
        Step_64 = 64,
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
    }

    [System.Serializable]
    public class LayerOutlineSetting
    {
        public string layerName;

        // *요청 반영: 외곽선 사용 여부 토글
        public bool isUse;

        public Color outlineColor = Color.black;
        [Range(1, 5)] public int outlineThickness = 2;

        [Range(0f, 0.05f)] public float depthThreshold = 0.001f;

        [Tooltip("이 레이어들에 가려져도 무시하고 그 위에 외곽선을 그립니다. (예: 물에 잠겨도 선을 그리고 싶다면 Water 체크)")]
        public LayerMask forceEdgeMask = 0;
    }

    [CreateAssetMenu(fileName = "NewMapBakeSettings", menuName = "CoreEngine/LevelDesign/Map Bake Settings")]
    public class MapBakeSettingsSO : ScriptableObject
    {
        // 기즈모 토글 변수
        public bool showInteractiveGizmo = true;

        // 카메라 방향 기즈모 토글
        public bool showCameraGizmo = true; 

        [Header("File Path")]
        [Tooltip("이 맵의 이미지 타일들이 저장된 폴더 경로입니다.")]
        public string saveDirectory; // 🌟 추가됨

        [Header("Game Dimension")]
        [SerializeField, HideInInspector] private MapDimension _lastMapDimension;
        public MapDimension mapDimension = MapDimension._3D;

        [Header("Projection & Depth Settings")]
        public MapProjectionPlane projectionPlane = MapProjectionPlane.XZ;

        public MapDepthSteps depthSteps = MapDepthSteps.Step_8;

        // *요청 반영: 가장 바닥에 있을 때의 최종 밝기(하한선) 지정 변수 추가
        [Range(0, 1)]
        [Tooltip("깊이(Depth)가 가장 깊은 바닥의 밝기를 결정합니다. (0=완전 검정, 1=기본색상)")]
        public float finalDepthBrightness = 0.5f;

        [Header("Global Settings")]
        public Vector3 centerPosition;
        public Vector2 totalMapSize = new Vector2(1024, 1024);
        public float captureOffset = 500f;
        public float maxDepth = 1000f;

        [Header("Tile Settings (Grid)")]
        public Vector2 tileSize = new Vector2(720, 480);

        [Header("Render Settings")]
        public MapResolution resolution = MapResolution.Res_512;
        public LayerMask renderMask = -1;
        public Color backgroundColor = Color.black;
        public bool useLayerColor;

        // 🌟 항상 32개의 데이터를 고정으로 가질 리스트들
        [HideInInspector] public List<LayerColorPair> layerColors = new List<LayerColorPair>();
        [HideInInspector] public List<LayerOutlineSetting> outlineSettings = new List<LayerOutlineSetting>();

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
            PreventAlpha0();
        }

        public void DimensionPreset()
        {
            if (mapDimension != _lastMapDimension)
            {
                _lastMapDimension = mapDimension;

                if (mapDimension == MapDimension._2D)
                {
                    projectionPlane = MapProjectionPlane.XY;
                    depthSteps = MapDepthSteps.None;
                    useLayerColor = false;
                }

                if (mapDimension == MapDimension._3D)
                {
                    projectionPlane = MapProjectionPlane.XZ;
                    depthSteps = MapDepthSteps.Step_8;
                    useLayerColor = true;
                }
            }
        }

        public void PreventAlpha0()
        {
            if (outlineSettings == null) return;
            for (int i = 0; i < outlineSettings.Count; i++)
            {
                if (outlineSettings[i].outlineColor.a <= 0.01f)
                {
                    Color correctedColor = outlineSettings[i].outlineColor;
                    correctedColor.a = 1f;
                    outlineSettings[i].outlineColor = correctedColor;
                }
            }
        }
#endif
    }
}