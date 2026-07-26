//using Core;
//using Core.Interface;
//using UnityEngine;

//namespace Core.LevelDesign
//{
//    public interface IPlayerPositionProvider
//    {
//        Vector3 PlayerWorldPosition { get;  }
//    }

//    public class MinimapController : BaseLeaf, ITickable
//    {
//        [Header("References")]
//        [SerializeField] private MapDataSO _currentMapData;
//        [SerializeField] private RectTransform _mapImageRect;  // 지도가 그려지는 RawImage의 RectTransform
//        [SerializeField] private RectTransform _playerIconRect; // 가운데 고정된 플레이어 화살표

//        // 플레이어 위치 제공자 (이전에 나침반에서 썼던 것과 동일!)
//        private InterfaceReceiver<IPlayerPositionProvider> _playerPositionProvider = new();

//        public TickGroup TickGroup => throw new System.NotImplementedException();

//        public void Initialize()
//        {
//            // MapDataSO의 이미지를 RawImage에 적용하는 로직 등
//        }

//        public void Tick(float deltaTime)
//        {
//            if (!_playerPositionProvider.TryGet(out var provider)) return;

//            Vector3 playerWorldPos = provider.PlayerWorldPosition; // 캐릭터 3D 위치 가져오기

//            // 1. 공통 수학 유틸리티로 0.0 ~ 1.0 비율 구하기
//            Vector2 normalizedPos = Utility.GetNormalizedMapPosition(
//                playerWorldPos,
//                _currentMapData.WorldMinBounds,
//                _currentMapData.WorldMaxBounds
//            );

//            // 2. 미니맵 전용 로직: 지도 이미지의 피벗(Pivot)을 플레이어 비율로 변경!
//            // 이렇게 하면 지도가 알아서 반대 방향으로 스크롤되며 플레이어를 중앙에 맞춥니다.
//            _mapImageRect.pivot = normalizedPos;
//            _mapImageRect.anchoredPosition = Vector2.zero; // 항상 마스크 정중앙 고정

//            // 3. 화살표 회전 로직 (선택사항: 나침반처럼 카메라 방향에 맞춰 돌리기)
//            // _playerIconRect.localEulerAngles = new Vector3(0, 0, -카메라Y회전값);
//        }
//    }
//}