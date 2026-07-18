using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Core.Environment
{
    [ExecuteAlways]
    [RequireComponent(typeof(SpaceZoneCore))]
    public class InnerZoneModule : MonoBehaviour
    {
        [HideInInspector][SerializeField] private float _zoneA_StartY = 6f;
        [HideInInspector][SerializeField] private float _zoneB_StartXAbs = 3f;
        [HideInInspector][SerializeField] private float _zoneC_EndY = 4f;
        [HideInInspector][SerializeField] private bool _showInnerZones = true;

        private SpaceZoneCore _core;
        public const string INNER_FOLDER_NAME = "Inner";

        #region Properties
        public float ZoneA_StartY { get => _zoneA_StartY; set => _zoneA_StartY = value; }
        public float ZoneB_StartXAbs { get => _zoneB_StartXAbs; set => _zoneB_StartXAbs = value; }
        public float ZoneC_EndY { get => _zoneC_EndY; set => _zoneC_EndY = value; }
        public bool ShowInnerZones
        {
            get => _showInnerZones;
            set { if (_showInnerZones != value) { _showInnerZones = value; UpdateZoneVisuals(); } }
        }
        #endregion

        private void OnEnable()
        {
            _core = GetComponent<SpaceZoneCore>();
            _core.OnZoneModified += RebuildZones;
        }

        private void OnDisable()
        {
            if (_core != null) _core.OnZoneModified -= RebuildZones;
        }

        public void RebuildZones()
        {
            if (_core == null) _core = GetComponent<SpaceZoneCore>();
            Vector3 pScale = _core.zoneSize;
            if (pScale.x <= 0 || pScale.y <= 0 || pScale.z <= 0) return;

            Transform folderTr = GetOrCreateContainer(INNER_FOLDER_NAME);

            // [Zone A]
            float sizeYA = pScale.y - _zoneA_StartY;
            Vector3 centerA = new Vector3(0f, (-pScale.y * 0.5f) + _zoneA_StartY + (sizeYA * 0.5f), 0f);
            UpdateZoneFace("_Zone_A", centerA, new Vector3(pScale.x, sizeYA, pScale.z), Color.red, folderTr, true);

            // [Zone B - 좌/우 대칭]
            float sizeXB = (pScale.x * 0.5f) - _zoneB_StartXAbs;
            bool isZoneBActive = sizeXB > 0f;

            Vector3 centerB_Left = new Vector3((-pScale.x * 0.5f - _zoneB_StartXAbs) * 0.5f, 0f, 0f);
            UpdateZoneFace("_Zone_B_Left", centerB_Left, new Vector3(sizeXB, pScale.y, pScale.z), Color.green, folderTr, isZoneBActive);

            Vector3 centerB_Right = new Vector3((pScale.x * 0.5f + _zoneB_StartXAbs) * 0.5f, 0f, 0f);
            UpdateZoneFace("_Zone_B_Right", centerB_Right, new Vector3(sizeXB, pScale.y, pScale.z), Color.green, folderTr, isZoneBActive);

            // [Zone C]
            Vector3 centerC = new Vector3(0f, (-pScale.y * 0.5f) + (_zoneC_EndY * 0.5f), 0f);
            UpdateZoneFace("_Zone_C", centerC, new Vector3(pScale.x, _zoneC_EndY, pScale.z), Color.blue, folderTr, true);

            UpdateZoneVisuals();
        }

        private Transform GetOrCreateContainer(string containerName)
        {
            Transform container = transform.Find(containerName);
            if (container == null)
            {
                GameObject obj = new GameObject(containerName);
#if UNITY_EDITOR
                Undo.RegisterCreatedObjectUndo(obj, $"Create {containerName}");
#endif
                container = obj.transform;
                container.SetParent(transform);
            }
            container.localPosition = Vector3.zero;
            container.localRotation = Quaternion.identity;
            container.localScale = Vector3.one;
            return container;
        }

        private void UpdateZoneFace(string zoneName, Vector3 localCenter, Vector3 localSize, Color zoneColor, Transform parent, bool isVisibleAndActive)
        {
            Transform zoneTr = parent.Find(zoneName);
            GameObject zoneObj;

            if (zoneTr == null)
            {
                zoneObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                zoneObj.name = zoneName;
                zoneObj.transform.SetParent(parent);
                if (zoneObj.TryGetComponent(out BoxCollider bc)) bc.isTrigger = true;

                // 디버그용 머티리얼 세팅 (1회만 수행)
                if (zoneObj.TryGetComponent(out MeshRenderer mr))
                {
                    Material tempMat = new Material(Shader.Find("Sprites/Default"));
                    tempMat.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 0.25f);
                    mr.sharedMaterial = tempMat;
                }

#if UNITY_EDITOR
                Undo.RegisterCreatedObjectUndo(zoneObj, $"Create {zoneName}");
#endif
            }
            else
            {
                zoneObj = zoneTr.gameObject;
#if UNITY_EDITOR
                Undo.RecordObject(zoneObj.transform, "Update Inner Transform");
#endif
            }

            // 🌟 파괴(Destroy) 대신 꺼두기(SetActive) 사용!
            zoneObj.SetActive(isVisibleAndActive);

            if (isVisibleAndActive)
            {
                Vector3 pScale = _core.zoneSize;
                zoneObj.transform.localPosition = new Vector3(localCenter.x / pScale.x, localCenter.y / pScale.y, localCenter.z / pScale.z);
                zoneObj.transform.localRotation = Quaternion.identity;
                zoneObj.transform.localScale = new Vector3(localSize.x / pScale.x, localSize.y / pScale.y, localSize.z / pScale.z);
            }
        }

        public void UpdateZoneVisuals()
        {
            if (Application.isPlaying) _showInnerZones = false;

            Transform folder = transform.Find(INNER_FOLDER_NAME);
            if (folder == null) return;
            foreach (var renderer in folder.GetComponentsInChildren<MeshRenderer>(true))
            {
                renderer.enabled = _showInnerZones;
            }
        }
    }
}