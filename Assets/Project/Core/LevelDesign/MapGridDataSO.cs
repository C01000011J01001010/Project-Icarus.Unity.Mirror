using UnityEngine;

namespace CoreEngine.LevelDesign
{
    [CreateAssetMenu(fileName = "NewMapGridData", menuName = MenuNamesSO.DefaultMenu + "/LevelDesign/Map Grid Data")]
    public class MapGridDataSO : ScriptableObject
    {
        [Header("Map Info")]
        public string sceneName; // 로드할 때 폴더 경로를 찾기 위한 이름

        [Header("Grid System")]
        public int totalCols;    // X축 타일 개수
        public int totalRows;    // Z축 타일 개수
        public Vector2 tileSize; // 타일 1장당 실제 3D 크기 (예: 1000x1000)

        [Header("World Bounds")]
        public Vector2 worldMinBounds; // 전체 맵의 가장 왼쪽 아래 좌표
        public Vector2 worldMaxBounds; // 전체 맵의 가장 오른쪽 위 좌표

        [Header("LOD (전체 맵)")]
        [Tooltip("M키를 눌렀을 때 보여줄 저해상도 전체 맵 이미지")]
        public Texture2D fullMapLOD;
    }
}