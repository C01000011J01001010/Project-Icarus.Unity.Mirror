using UnityEditor;
using UnityEngine;

namespace CoreEngine.Environment
{
    [CustomEditor(typeof(InnerSpaceZoneGenerator))]
    public class InnerSpaceZoneGeneratorEditor : Editor
    {
        private InnerSpaceZoneGenerator _generator;

        private void OnEnable()
        {
            _generator = (InnerSpaceZoneGenerator)target;
        }

        public override void OnInspectorGUI()
        {
            Vector3 pScale = _generator.transform.localScale;

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                $"현재 부모 공간 크기 -> 가로(X): {pScale.x} | 세로(Y): {pScale.y} | 깊이(Z): {pScale.z}\n" +
                $"중앙 공간은 비워두고, Zone B는 지정된 X값부터 양쪽 끝 벽면까지 Y축 대칭으로 채워집니다.",
                MessageType.Info
            );
            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("🎮 내부 구역 수치 조절 슬라이더", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            float newAY = EditorGUILayout.Slider("Zone A 시작 Y축 위치", _generator.ZoneA_StartY, 0f, pScale.y);

            // 중앙에서부터 '비워둘' 거리를 설정합니다. (0부터 부모 절반 너비까지)
            float newBX = EditorGUILayout.Slider("Zone B 시작 X축 (빈 공간 너비)", _generator.ZoneB_StartXAbs, 0f, pScale.x * 0.5f);

            float newCY = EditorGUILayout.Slider("Zone C 끝나는 Y축 위치", _generator.ZoneC_EndY, 0f, pScale.y);

            bool newShowMesh = EditorGUILayout.Toggle("내부 구역 가이드 메쉬 활성화", _generator.ShowZoneMeshes);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_generator, "Modify Inner Space Properties");
                _generator.ZoneA_StartY = newAY;
                _generator.ZoneB_StartXAbs = newBX;
                _generator.ZoneC_EndY = newCY;
                _generator.ShowZoneMeshes = newShowMesh;

                BuildInternalSpaces();
            }

            EditorGUILayout.Space(15);

            if (GUILayout.Button("🧱 내부 공간 구역 확정 및 초기화 생성", GUILayout.Height(40)))
            {
                BuildInternalSpaces();
                EditorUtility.SetDirty(_generator);
            }
        }

        private void BuildInternalSpaces()
        {
            Vector3 pScale = _generator.transform.localScale;
            if (pScale.x <= 0 || pScale.y <= 0 || pScale.z <= 0) return;

            // 만약 이전 구조(중앙 기둥형 B)에서 쓰던 구형 객체가 남아있다면 깨끗이 청소합니다.
            Transform legacyZoneB = _generator.transform.Find("_Generated_Zone_B");
            if (legacyZoneB != null) Undo.DestroyObjectImmediate(legacyZoneB.gameObject);

            // ==========================================
            // 1. ZONE A 생성 (상단 영역)
            // ==========================================
            float sizeYA = pScale.y - _generator.ZoneA_StartY;
            Vector3 centerA = new Vector3(
                0f,
                (-pScale.y * 0.5f) + _generator.ZoneA_StartY + (sizeYA * 0.5f),
                0f
            );
            Vector3 sizeVectorA = new Vector3(pScale.x, sizeYA, pScale.z);
            SetupZoneObject(InnerSpaceZoneGenerator.ZONE_A_NAME, centerA, sizeVectorA, Color.red);

            // ==========================================
            // 2. ZONE B 생성 (좌/우 양쪽 대칭 영역) - 중앙은 비워짐
            // ==========================================
            // B공간 하나당 가질 실제 X 너비: 부모 절반 너비 - 비워둘 너비(StartXAbs)
            float sizeXB = (pScale.x * 0.5f) - _generator.ZoneB_StartXAbs;

            if (sizeXB > 0f)
            {
                // [좌측 B 공간]
                // 중앙 좌표: 왼쪽 끝(-pScale.x/2)과 빈공간 끝점(-StartXAbs)의 한가운데
                Vector3 centerB_Left = new Vector3(
                    (-pScale.x * 0.5f - _generator.ZoneB_StartXAbs) * 0.5f,
                    0f, // Y 중심은 0 (부모 높이 전체 사용)
                    0f
                );
                Vector3 sizeVectorB_Left = new Vector3(sizeXB, pScale.y, pScale.z);
                SetupZoneObject(InnerSpaceZoneGenerator.ZONE_B_LEFT_NAME, centerB_Left, sizeVectorB_Left, Color.green);

                // [우측 B 공간]
                // 중앙 좌표: 오른쪽 끝(+pScale.x/2)과 빈공간 끝점(+StartXAbs)의 한가운데
                Vector3 centerB_Right = new Vector3(
                    (pScale.x * 0.5f + _generator.ZoneB_StartXAbs) * 0.5f,
                    0f,
                    0f
                );
                Vector3 sizeVectorB_Right = new Vector3(sizeXB, pScale.y, pScale.z);
                SetupZoneObject(InnerSpaceZoneGenerator.ZONE_B_RIGHT_NAME, centerB_Right, sizeVectorB_Right, Color.green);
            }
            else
            {
                // 만약 빈 공간(StartXAbs) 슬라이더를 부모 끝까지 늘려서 B구역의 너비가 0이 되었다면 오브젝트를 없앱니다.
                Transform leftB = _generator.transform.Find(InnerSpaceZoneGenerator.ZONE_B_LEFT_NAME);
                Transform rightB = _generator.transform.Find(InnerSpaceZoneGenerator.ZONE_B_RIGHT_NAME);
                if (leftB != null) Undo.DestroyObjectImmediate(leftB.gameObject);
                if (rightB != null) Undo.DestroyObjectImmediate(rightB.gameObject);
            }

            // ==========================================
            // 3. ZONE C 생성 (하단 영역)
            // ==========================================
            Vector3 centerC = new Vector3(
                0f,
                (-pScale.y * 0.5f) + (_generator.ZoneC_EndY * 0.5f),
                0f
            );
            Vector3 sizeVectorC = new Vector3(pScale.x, _generator.ZoneC_EndY, pScale.z);
            SetupZoneObject(InnerSpaceZoneGenerator.ZONE_C_NAME, centerC, sizeVectorC, Color.blue);
        }

        private void SetupZoneObject(string zoneName, Vector3 targetLocalCenter, Vector3 targetLocalSize, Color zoneColor)
        {
            Transform zoneTransform = _generator.transform.Find(zoneName);
            GameObject zoneObj;

            if (zoneTransform == null)
            {
                zoneObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Undo.RegisterCreatedObjectUndo(zoneObj, $"Create {zoneName}");
                zoneObj.name = zoneName;
                zoneObj.transform.SetParent(_generator.transform);
                zoneObj.transform.localRotation = Quaternion.identity;
            }
            else
            {
                zoneObj = zoneTransform.gameObject;
            }

            Vector3 pScale = _generator.transform.localScale;

            // 부모의 크기에 영향을 받지 않도록 비율 상쇄 (이전과 동일한 플랫폼 수학 패턴)
            zoneObj.transform.localPosition = new Vector3(
                targetLocalCenter.x / pScale.x,
                targetLocalCenter.y / pScale.y,
                targetLocalCenter.z / pScale.z
            );

            zoneObj.transform.localScale = new Vector3(
                targetLocalSize.x / pScale.x,
                targetLocalSize.y / pScale.y,
                targetLocalSize.z / pScale.z
            );

            if (zoneObj.TryGetComponent(out BoxCollider bc))
            {
                bc.isTrigger = true;
            }

            if (zoneObj.TryGetComponent(out MeshRenderer mr))
            {
                mr.enabled = _generator.ShowZoneMeshes;
                Material tempMaterial = new Material(Shader.Find("Sprites/Default"));
                tempMaterial.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 0.25f);
                mr.sharedMaterial = tempMaterial;
            }
        }
    }
}