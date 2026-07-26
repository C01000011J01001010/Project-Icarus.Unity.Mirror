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
        Step_16 = 16,
        Step_24 = 24
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
        public Color backgroundColor = new Color(0, 0, 0, 1);
        public bool useLayerColor;

        public List<LayerColorPair> layerColors = new List<LayerColorPair>();

        public int Cols => tileSize.x > 0 ? Mathf.CeilToInt(totalMapSize.x / tileSize.x) : 1;
        public int Rows => tileSize.y > 0 ? Mathf.CeilToInt(totalMapSize.y / tileSize.y) : 1;

        private void Awake()
        {
            _lastMapDimension = mapDimension;
        }

        private void OnValidate()
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
    }
}