using System.Collections.Generic;
using UnityEngine;
using Core.EventBus;

namespace Core.Manager.Culling
{
    #region Culling Object
    public enum CullingType
    {
        Static,         // 절대 움직이지 않는 객체 (바위, 나무 등)
        PassiveDynamic, // 플레이어에 의해 수동적으로 움직이는 객체 (발판, 상자 등)
        ActiveDynamic,  // 능동적으로 움직이는 객체 (NPC, 몬스터 등)
    }

    public interface ICullingObject
    {
        CullingType cullingType { get; }
        Transform transform { get; } // Behaviour 자동
        void SetVisualActive(bool isActive);
        void SetPhysicsActive(bool isActive);
    }

    // 유니티 가짜 널(Fake Null) 방어용 구조체
    public struct CullingReference
    {
        public ICullingObject Interface;
        public Object UnityObject;

        public CullingReference(ICullingObject cullingObj)
        {
            Interface = cullingObj;
            UnityObject = cullingObj.transform;
        }

        public bool IsValid => UnityObject != null;
    }
    #endregion

    #region Events
    public struct CullingPlayerGridChangedEvent : IEvent
    {
        public Vector3Int PlayerGrid;
        public CullingPlayerGridChangedEvent(Vector3Int grid) => PlayerGrid = grid;
    }

    public struct CullingObjectRegistrationEvent : IEvent
    {
        public ICullingObject CullingObject;
        public bool IsRegister;
        public CullingObjectRegistrationEvent(ICullingObject obj, bool isRegister)
        {
            CullingObject = obj;
            IsRegister = isRegister;
        }
    }

    public struct CullingObjectMovedEvent : IEvent
    {
        public ICullingObject CullingObject;
        public Vector3Int OldGrid;
        public Vector3Int NewGrid;
        public CullingObjectMovedEvent(ICullingObject obj, Vector3Int oldGrid, Vector3Int newGrid)
        {
            CullingObject = obj;
            OldGrid = oldGrid;
            NewGrid = newGrid;
        }
    }
    #endregion

    public enum CullingAxis { OneD_X, TwoD_XY, TwoD_XZ, ThreeD_XYZ }

    public class SpatialCullingManager : BaseManager
    {
        [Header("Grid Settings")]
        [SerializeField] private CullingAxis cullingAxis = CullingAxis.TwoD_XZ;
        [SerializeField] private float cellSize = 10f;

        [Header("Culling Thresholds")]

        [Tooltip("GameObject 렌더링 한계 격자 개수")]
        [SerializeField] private int a = 3;

        [Tooltip("Collider 물리 연산 한계 격자 개수")]
        [SerializeField] private int b = 1;

        // 통합 격자 딕셔너리 (이제 완벽하게 CullingReference만 담습니다)
        protected Dictionary<Vector3Int, List<CullingReference>> gridDictionary = new();

        private Vector3Int _currentPlayerGrid;

        protected override void OnEnable()
        {
            base.OnEnable();
            EventBus<CullingPlayerGridChangedEvent>.Subscribe(OnPlayerGridChanged);
            EventBus<CullingObjectRegistrationEvent>.Subscribe(OnCullingObjectRegistered);
            EventBus<CullingObjectMovedEvent>.Subscribe(OnCullingObjectMoved);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EventBus<CullingPlayerGridChangedEvent>.Unsubscribe(OnPlayerGridChanged);
            EventBus<CullingObjectRegistrationEvent>.Unsubscribe(OnCullingObjectRegistered);
            EventBus<CullingObjectMovedEvent>.Unsubscribe(OnCullingObjectMoved);
        }

        public Vector3Int GetGridKey(Vector3 worldPos)
        {
            int x = Mathf.FloorToInt(worldPos.x / cellSize);
            int y = Mathf.FloorToInt(worldPos.y / cellSize);
            int z = Mathf.FloorToInt(worldPos.z / cellSize);

            return cullingAxis switch
            {
                CullingAxis.OneD_X => new Vector3Int(x, 0, 0),
                CullingAxis.TwoD_XY => new Vector3Int(x, y, 0),
                CullingAxis.TwoD_XZ => new Vector3Int(x, 0, z),
                CullingAxis.ThreeD_XYZ => new Vector3Int(x, y, z),
                _ => new Vector3Int(x, 0, z)
            };
        }

        protected IEnumerable<Vector3Int> GetSurroundingGrids(Vector3Int center, int radius)
        {
            switch (cullingAxis)
            {
                case CullingAxis.OneD_X:
                    for (int x = -radius; x <= radius; x++) 
                        yield return new Vector3Int(center.x + x, 0, 0);
                    break;
                case CullingAxis.TwoD_XY:
                    for (int x = -radius; x <= radius; x++) 
                        for (int y = -radius; y <= radius; y++) 
                            yield return new Vector3Int(center.x + x, center.y + y, 0);
                    break;
                case CullingAxis.TwoD_XZ:
                    for (int x = -radius; x <= radius; x++) 
                        for (int z = -radius; z <= radius; z++) 
                            yield return new Vector3Int(center.x + x, 0, center.z + z);
                    break;
                case CullingAxis.ThreeD_XYZ:
                    for (int x = -radius; x <= radius; x++) 
                        for (int y = -radius; y <= radius; y++) 
                            for (int z = -radius; z <= radius; z++) 
                                yield return new Vector3Int(center.x + x, center.y + y, center.z + z);
                    break;
            }
        }

        private void OnPlayerGridChanged(CullingPlayerGridChangedEvent evt)
        {

            // 캐싱
            _currentPlayerGrid = evt.PlayerGrid;
            int maxRadius = a + 2;

            foreach (Vector3Int checkGrid in GetSurroundingGrids(_currentPlayerGrid, maxRadius))
            {
                if (gridDictionary.TryGetValue(checkGrid, out List<CullingReference> objects))
                {
                    int d = Mathf.Max(Mathf.Abs(_currentPlayerGrid.x - checkGrid.x),
                                      Mathf.Abs(_currentPlayerGrid.y - checkGrid.y),
                                      Mathf.Abs(_currentPlayerGrid.z - checkGrid.z));

                    ProcessCulling(objects, d);
                }
            }
        }

        private void OnCullingObjectRegistered(CullingObjectRegistrationEvent evt)
        {
            Vector3Int gridKey = GetGridKey(evt.CullingObject.transform.position);
            CullingReference reference = new CullingReference(evt.CullingObject);

            if (evt.IsRegister)
            {
                if (!gridDictionary.ContainsKey(gridKey))
                    gridDictionary[gridKey] = new List<CullingReference>();

                gridDictionary[gridKey].Add(reference);
            }
            else
            {
                if (gridDictionary.TryGetValue(gridKey, out List<CullingReference> list))
                {
                    list.RemoveAll(refObj => refObj.Interface == evt.CullingObject);
                    if (list.Count == 0) gridDictionary.Remove(gridKey);
                }
            }
        }

        private void OnCullingObjectMoved(CullingObjectMovedEvent evt)
        {
            if (gridDictionary.TryGetValue(evt.OldGrid, out List<CullingReference> oldList))
            {
                oldList.RemoveAll(refObj => refObj.Interface == evt.CullingObject);
                if (oldList.Count == 0) gridDictionary.Remove(evt.OldGrid);
            }

            if (!gridDictionary.ContainsKey(evt.NewGrid))
            {
                gridDictionary[evt.NewGrid] = new List<CullingReference>();
            }
            gridDictionary[evt.NewGrid].Add(new CullingReference(evt.CullingObject));

            int d = Mathf.Max(Mathf.Abs(_currentPlayerGrid.x - evt.NewGrid.x),
                              Mathf.Abs(_currentPlayerGrid.y - evt.NewGrid.y),
                              Mathf.Abs(_currentPlayerGrid.z - evt.NewGrid.z));

            List<CullingReference> singleObjList = new List<CullingReference> { new CullingReference(evt.CullingObject) };
            ProcessCulling(singleObjList, d);
        }

        private void ProcessCulling(List<CullingReference> objects, int d)
        {
            if (d <= b) SetCollidersActive(objects, true);
            else if (d > b + 1) SetCollidersActive(objects, false);

            if (d <= a) SetGameObjectsActive(objects, true);
            else if (d > a + 1) SetGameObjectsActive(objects, false);
        }

        private void SetGameObjectsActive(List<CullingReference> objects, bool isActive)
        {
            CullingReference[] snapshot = objects.ToArray();
            foreach (var refObj in snapshot)
            {
                if (!refObj.IsValid) continue;
                refObj.Interface.SetVisualActive(isActive);
            }
        }

        private void SetCollidersActive(List<CullingReference> objects, bool isActive)
        {
            CullingReference[] snapshot = objects.ToArray();
            foreach (var refObj in snapshot)
            {
                if (!refObj.IsValid) continue;
                refObj.Interface.SetPhysicsActive(isActive);
            }
        }

#if UNITY_EDITOR
        // ... (유저님께서 작성하신 아름다운 Gizmo 코드는 동일하게 유지!)
        [Header("Debug Visualization")]
        [SerializeField] private bool showDebugGrid = true;
        [SerializeField] private bool IsDrawSelected = false;
        [SerializeField] private float debugPlaneHeight = 0f;

        private void OnDrawGizmos() { if (!IsDrawSelected) DrawGrid(); }
        private void OnDrawGizmosSelected() { if (IsDrawSelected) DrawGrid(); }

        private void DrawGrid()
        {
            if (!showDebugGrid || !Application.isPlaying) return;

            int maxRadius = a + 2;
            foreach (Vector3Int checkGrid in GetSurroundingGrids(_currentPlayerGrid, maxRadius))
            {
                int d = Mathf.Max(Mathf.Abs(_currentPlayerGrid.x - checkGrid.x),
                                  Mathf.Abs(_currentPlayerGrid.y - checkGrid.y),
                                  Mathf.Abs(_currentPlayerGrid.z - checkGrid.z));

                if (d == 0) Gizmos.color = Color.cyan;
                else if (d <= b) Gizmos.color = Color.green;
                else if (d <= a) Gizmos.color = Color.yellow;
                else if (d == a + 1 || d == a + 2) Gizmos.color = Color.red;

                DrawGridCellGizmo(checkGrid);
            }
        }

        private void DrawGridCellGizmo(Vector3Int gridKey)
        {
            float halfCell = cellSize * 0.5f;
            Vector3 center = new Vector3(gridKey.x * cellSize + halfCell, gridKey.y * cellSize + halfCell, gridKey.z * cellSize + halfCell);
            Vector3 size = Vector3.one * cellSize;

            switch (cullingAxis)
            {
                case CullingAxis.OneD_X: center.y = debugPlaneHeight; center.z = debugPlaneHeight; size.y = 1000f; size.z = 0f; break;
                case CullingAxis.TwoD_XY: center.z = debugPlaneHeight; size.z = 0f; break;
                case CullingAxis.TwoD_XZ: center.y = debugPlaneHeight; size.y = 0f; break;
            }

            Gizmos.DrawWireCube(center, size);
            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.1f);
            Gizmos.DrawCube(center, size);
        }
#endif


#if UNITY_EDITOR
        // 인스펙터에서 값이 바뀔 때마다 자동으로 호출되어 논리적 오류를 원천 차단합니다.
        protected override void OnValidate()
        {
            base.OnValidate();
            // 1. b는 항상 0보다 커야 합니다. (최소값 1)
            b = Mathf.Max(1, b);

            // 2. a는 항상 b보다 커야 합니다. (최소값 b + 1)
            a = Mathf.Max(b + 1, a);
        }
#endif
    }
}