using CoreEngine;
using CoreEngine.Interface;
using CoreEngine.LevelDesign;
using CoreEngine.UI; // AutoGridCellSize가 있는 네임스페이스
using UnityEngine;
using UnityEngine.UI;

namespace CoreEngine.Ui
{
    /// <summary>
    /// 화면 회전 없이, 플레이어의 월드 좌표만 추적하여 3x3 그리드 타일을 이동/교체하는 고정형 미니맵 UI
    /// </summary>
    [RequireComponent(typeof(GridLayoutGroup), typeof(RectTransform))]
    public class MinimapController : CoreMonoBehaviour, ILateTickable
    {
        [Header("Map Data")]
        [SerializeField] private MapGridDataSO _mapData;

        [Header("UI Hierarchy")]
        [Tooltip("가로(Width)는 고정하고, 맵 비율에 맞춰 세로(Height)를 조절할 부모 캔버스")]
        [SerializeField] private RectTransform _heightSetTarget;

        [Header("3x3 Tile RawImages (Top-Left to Bottom-Right)")]
        [SerializeField] private RawImage[] _tileImages = new RawImage[9];

        [Header("UI Tile Config")]
        [SerializeField] private Vector2 _uiTileSize = new Vector2(200f, 200f);

        // 다중 상속 우회용 수신기 (Composition)
        private readonly InterfaceReceiver<IMapTargetProvider> _receiver = new();

        private RectTransform _rectTransform;
        private Vector2Int _currentCenterGridIndex;

        public LateTickGroup LateTickGroup => LateTickGroup.Ui;

        protected override void OnEnable()
        {
            base.OnEnable();
            _receiver.Bind();
            _currentCenterGridIndex = new Vector2Int(-1, -1);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _receiver.Unbind();
        }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();

            // 자식 RawImage 9개 자동 할당 (null 예방)
            if (_tileImages == null || _tileImages.Length < 9) _tileImages = new RawImage[9];
            var tiles = GetComponentsInChildren<RawImage>();
            for (int i = 0; i < _tileImages.Length && i < tiles.Length; i++)
            {
                if (_tileImages[i] == null && tiles[i] != null)
                {
                    _tileImages[i] = tiles[i];
                }
            }
        }

        private void Start()
        {
            // MinimapSizeFitter의 Awake 이후
            AdjustDrawCanvasHeight();
        }

        

        public void LateTick(float dt)
        {
            if (!_receiver.TryGet(out var targetProvider) || _mapData == null) return;

            Vector3 worldPos = targetProvider.WorldPosition;

            UpdateMapGridTiles(worldPos);
            UpdateMapPosition(worldPos);
        }

        // =========================================================
        // 🌟 맵 비율 동기화 및 자동 레이아웃 갱신 로직
        // =========================================================

        /// <summary>
        /// 실제 3D 맵의 비율(가로:세로)을 계산하여, Draw Canvas의 가로 너비에 비례하게 세로 높이를 조절합니다.
        /// </summary>
        private void AdjustDrawCanvasHeight()
        {
            if (_mapData == null || _heightSetTarget == null) return;

            // 1. 실제 맵의 전체 가로/세로 길이 산출
            float totalMapWidth = _mapData.totalCols * _mapData.tileSize.x;
            float totalMapHeight = _mapData.totalRows * _mapData.tileSize.y;

            if (totalMapWidth <= 0) return;

            // 2. 가로 대비 세로 비율 계산
            float ratio = totalMapHeight / totalMapWidth;

            // 3. Draw Canvas의 가로(Width)는 유지한 채, 비율에 맞춰 세로(Height)만 덮어쓰기
            float currentWidth = _heightSetTarget.rect.width;
            _heightSetTarget.sizeDelta = new Vector2(_heightSetTarget.sizeDelta.x, currentWidth * ratio);

            // 4. 부모(최상단)에 있는 MinimapSizeFitter를 자동으로 찾아 전체 배경 UI 크기까지 조절
            var sizeFitter = GetComponentInParent<MinimapSizeFitter>();
            if (sizeFitter != null)
            {
                sizeFitter.AdjustHeight();
            }
            else
            {
                UtilityLog.Log("[MinimapController] 부모 구조에서 MinimapSizeFitter를 찾을 수 없습니다.", LogColor.Yellow);
            }
        }

        /// <summary>
        /// 부모(Draw Canvas)의 크기가 변하면 이 컨테이너도 StretchAll에 의해 자동으로 변합니다.
        /// 이때 AutoGridCellSize가 계산해 둔 최신 타일 크기를 받아와 스케일 오프셋에 반영합니다.
        /// </summary>
        private void OnRectTransformDimensionsChange()
        {
            if (gameObject.TryGetComponent(out AutoGridCellSize autoGrid))
            {
                _uiTileSize = autoGrid.LastSize;
            }
        }

        // =========================================================
        // 🌟 타일 교체 및 이동 로직 (기존 유저님 작성 코드 유지)
        // =========================================================

        private void UpdateMapGridTiles(Vector3 worldPos)
        {
            Vector2Int centerGrid = _mapData.GetGridIndex(worldPos);
            if (centerGrid == _currentCenterGridIndex) return;

            _currentCenterGridIndex = centerGrid;

            int tileIndex = 0;
            for (int rowOffset = 1; rowOffset >= -1; rowOffset--)
            {
                for (int colOffset = -1; colOffset <= 1; colOffset++)
                {
                    int targetCol = centerGrid.x + colOffset;
                    int targetRow = centerGrid.y + rowOffset;

                    bool isValidTile = targetCol >= 0 && targetCol < _mapData.totalCols &&
                                       targetRow >= 0 && targetRow < _mapData.totalRows;

                    if (isValidTile && tileIndex < _tileImages.Length && _tileImages[tileIndex] != null)
                    {
                        _tileImages[tileIndex].enabled = true;
                        // TODO: Addressables 연동 로직
                    }
                    else if (tileIndex < _tileImages.Length && _tileImages[tileIndex] != null)
                    {
                        _tileImages[tileIndex].enabled = false;
                    }

                    tileIndex++;
                }
            }
        }

        private void UpdateMapPosition(Vector3 worldPos)
        {
            if (_currentCenterGridIndex.x < 0) return;

            Vector2 centerTileWorldCenter = _mapData.worldMinBounds + new Vector2(
                (_currentCenterGridIndex.x + 0.5f) * _mapData.tileSize.x,
                (_currentCenterGridIndex.y + 0.5f) * _mapData.tileSize.y
            );

            float offsetX = worldPos.x - centerTileWorldCenter.x;
            float offsetZ = worldPos.z - centerTileWorldCenter.y;

            float scaleX = _uiTileSize.x / _mapData.tileSize.x;
            float scaleY = _uiTileSize.y / _mapData.tileSize.y;

            float uiOffsetX = offsetX * scaleX;
            float uiOffsetY = offsetZ * scaleY;

            _rectTransform.anchoredPosition = new Vector2(-uiOffsetX, -uiOffsetY);
        }
    }
}