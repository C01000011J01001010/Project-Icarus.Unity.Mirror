using UnityEditor;
using UnityEngine;

namespace Core.Environment
{
    /// <summary>
    /// SpaceZoneEffectsDirector의 인스펙터 레이아웃을 탭 메뉴로 가공하고,
    /// 요구된 계층 폴더 구조("Outer", "Inner")에 맞춰 기하 연산을 집행하는 에디터 스크립트.
    /// </summary>
    [CustomEditor(typeof(SpaceZoneEffectsDirector))]
    public class SpaceZoneEffectsDirectorEditor : Editor
    {
        private SpaceZoneEffectsDirector _director;
        private int _currentTab = 0; // 세션 구분을 위한 탭 상태 저장 인덱스

        private void OnEnable()
        {
            _director = (SpaceZoneEffectsDirector)target;
        }

        public override void OnInspectorGUI()
        {
            Vector3 pScale = _director.transform.localScale;

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                $"📐 마스터 공간 볼륨 -> 가로(X): {pScale.x} | 세로(Y): {pScale.y} | 깊이(Z): {pScale.z}\n" +
                $"모든 생성물은 본체 산하의 'Outer' 및 'Inner' 빈 오브젝트 폴더 내부로 깔끔히 격리 자동 빌드됩니다.",
                MessageType.Info
            );
            EditorGUILayout.Space(5);

            // 가독성 높은 상단 세션 전환 탭 바 렌더링
            _currentTab = GUILayout.Toolbar(_currentTab, new string[] { "🧱 외곽 투명 고체벽", "📐 내부 효과 트리거 구역" });
            EditorGUILayout.Space(10);

            // 실시간 슬라이더 변동 상태 감지 파이프라인 개방
            EditorGUI.BeginChangeCheck();

            if (_currentTab == 0)
            {
                EditorGUILayout.LabelField("🧱 [Outer] 외곽 가둠 고체벽 제어 프로퍼티", EditorStyles.boldLabel);
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUILayout.Vector3Field("동기화된 원본 공간 크기(Scale)", _director.ZoneSize);
                }
                _director.ShowOuterWalls = EditorGUILayout.Toggle("외곽 물리 벽면 메쉬 가이드 보기", _director.ShowOuterWalls);
            }
            else
            {
                EditorGUILayout.LabelField("📐 [Inner] 내부 영역 감지 트리거 슬라이더", EditorStyles.boldLabel);
                _director.ZoneA_StartY = EditorGUILayout.Slider("Zone A 시작 Y축 고도", _director.ZoneA_StartY, 0f, pScale.y);
                _director.ZoneB_StartXAbs = EditorGUILayout.Slider("Zone B X축 대칭 안전지대 폭", _director.ZoneB_StartXAbs, 0f, pScale.x * 0.5f);
                _director.ZoneC_EndY = EditorGUILayout.Slider("Zone C 종료 Y축 고도", _director.ZoneC_EndY, 0f, pScale.y);

                EditorGUILayout.Space(5);
                _director.ShowInnerZones = EditorGUILayout.Toggle("내부 감지 구역 메쉬 가이드 보기", _director.ShowInnerZones);
            }

            // 슬라이더 조작 감지 시 실시간 완전 파괴 후 재조립 집행
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_director, "Space Parameter Realtime Modification");
                RebuildStructuralHierarchy();
            }

            EditorGUILayout.Space(20);

            if (GUILayout.Button("⚡ 공간 전체 시스템 확정 빌드 / 갱신", GUILayout.Height(40)))
            {
                RebuildStructuralHierarchy();
                EditorUtility.SetDirty(_director); // 변경 데이터를 디스크에 직렬화 저장 유도
            }
        }

        /// <summary>
        /// 요구사항에 명시된 {조부모(디렉터) ➔ 부모(Outer/Inner 폴더) ➔ 자식(실질 6면체)} 계층을 확립하고 갱신합니다.
        /// </summary>
        private void RebuildStructuralHierarchy()
        {
            Vector3 pScale = _director.transform.localScale;
            if (pScale.x <= 0 || pScale.y <= 0 || pScale.z <= 0) return;

            // 1. 마스터 스케일 보정 선행
            _director.transform.localScale = _director.ZoneSize;

            // ====================================================================
            // 🌟 [STEP 1] 요구사항: 외부벽을 묶어둘 "Outer" 부모 오브젝트 생성 및 격리 정리
            // ====================================================================
            Transform existingOuterFolder = _director.transform.Find(SpaceZoneEffectsDirector.OUTER_FOLDER_NAME);
            if (existingOuterFolder != null) Undo.DestroyObjectImmediate(existingOuterFolder.gameObject);

            GameObject outerFolderObj = new GameObject(SpaceZoneEffectsDirector.OUTER_FOLDER_NAME);
            Undo.RegisterCreatedObjectUndo(outerFolderObj, "Generate Outer Folder");
            outerFolderObj.transform.SetParent(_director.transform);

            // 🌟 중요: 중간 컨테이너의 Transform 변형 오차를 막기 위해 위치/회전은 영점, 스케일은 1로 수렴 고정합니다.
            ResetLocalTransform(outerFolderObj.transform);

            // 외곽 투명 물리벽면 6방향 조립 집행
            BuildWallFace("OuterWall_Left", new Vector3(-0.5f - (0.5f / pScale.x), 0, 0), new Vector3(1f / pScale.x, 1f, 1f), outerFolderObj.transform);
            BuildWallFace("OuterWall_Right", new Vector3(0.5f + (0.5f / pScale.x), 0, 0), new Vector3(1f / pScale.x, 1f, 1f), outerFolderObj.transform);
            BuildWallFace("OuterWall_Bottom", new Vector3(0, -0.5f - (0.5f / pScale.y), 0), new Vector3(1f, 1f / pScale.y, 1f), outerFolderObj.transform);
            BuildWallFace("OuterWall_Top", new Vector3(0, 0.5f + (0.5f / pScale.y), 0), new Vector3(1f, 1f / pScale.y, 1f), outerFolderObj.transform);
            BuildWallFace("OuterWall_Back", new Vector3(0, 0, -0.5f - (0.5f / pScale.z)), new Vector3(1f, 1f, 1f / pScale.z), outerFolderObj.transform);
            BuildWallFace("OuterWall_Front", new Vector3(0, 0, 0.5f + (0.5f / pScale.z)), new Vector3(1f, 1f, 1f / pScale.z), outerFolderObj.transform);


            // ====================================================================
            // 🌟 [STEP 2] 요구사항: 내부 구역을 묶어둘 "Inner" 부모 오브젝트 생성 및 격리 정리
            // ====================================================================
            Transform existingInnerFolder = _director.transform.Find(SpaceZoneEffectsDirector.INNER_FOLDER_NAME);
            if (existingInnerFolder != null) Undo.DestroyObjectImmediate(existingInnerFolder.gameObject);

            GameObject innerFolderObj = new GameObject(SpaceZoneEffectsDirector.INNER_FOLDER_NAME);
            Undo.RegisterCreatedObjectUndo(innerFolderObj, "Generate Inner Folder");
            innerFolderObj.transform.SetParent(_director.transform);
            ResetLocalTransform(innerFolderObj.transform);

            // [Zone A 수식 빌드]
            float sizeYA = pScale.y - _director.ZoneA_StartY;
            Vector3 centerA = new Vector3(0f, (-pScale.y * 0.5f) + _director.ZoneA_StartY + (sizeYA * 0.5f), 0f);
            BuildZoneFace(SpaceZoneEffectsDirector.ZONE_A_NAME, centerA, new Vector3(pScale.x, sizeYA, pScale.z), Color.red, innerFolderObj.transform);

            // [Zone B 수식 빌드 - 좌/우 대칭 외곽 채우기]
            float sizeXB = (pScale.x * 0.5f) - _director.ZoneB_StartXAbs;
            if (sizeXB > 0f)
            {
                Vector3 centerB_Left = new Vector3((-pScale.x * 0.5f - _director.ZoneB_StartXAbs) * 0.5f, 0f, 0f);
                BuildZoneFace(SpaceZoneEffectsDirector.ZONE_B_LEFT_NAME, centerB_Left, new Vector3(sizeXB, pScale.y, pScale.z), Color.green, innerFolderObj.transform);

                Vector3 centerB_Right = new Vector3((pScale.x * 0.5f + _director.ZoneB_StartXAbs) * 0.5f, 0f, 0f);
                BuildZoneFace(SpaceZoneEffectsDirector.ZONE_B_RIGHT_NAME, centerB_Right, new Vector3(sizeXB, pScale.y, pScale.z), Color.green, innerFolderObj.transform);
            }

            // [Zone C 수식 빌드]
            Vector3 centerC = new Vector3(0f, (-pScale.y * 0.5f) + (_director.ZoneC_EndY * 0.5f), 0f);
            BuildZoneFace(SpaceZoneEffectsDirector.ZONE_C_NAME, centerC, new Vector3(pScale.x, _director.ZoneC_EndY, pScale.z), Color.blue, innerFolderObj.transform);

            // 최종 동기화 렌더러 피드백 강제 트리거
            _director.UpdateOuterVisuals();
            _director.UpdateInnerVisuals();
        }

        /// <summary>
        /// 외곽 투명 물리 충돌 벽면을 하이라키에 인스턴싱하는 헬퍼 함수
        /// </summary>
        private void BuildWallFace(string faceName, Vector3 localPos, Vector3 localScale, Transform parentFolder)
        {
            GameObject faceObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            faceObj.name = faceName;
            faceObj.transform.SetParent(parentFolder);

            // 🌟 부모 컨테이너(Outer Folder)의 스케일이 1이므로, 
            // 실질 6면체 자식 오브젝트는 마스터 조부모의 스케일을 직접 나누어 곱의 확대를 상쇄 연산해야 월드 크기 두께 1을 유지합니다.
            faceObj.transform.localPosition = localPos;
            faceObj.transform.localScale = localScale;

            if (faceObj.TryGetComponent(out BoxCollider bc)) bc.isTrigger = false; // 플레이어를 가두는 벽이므로 트리거 OFF
            if (faceObj.TryGetComponent(out MeshRenderer mr)) mr.enabled = _director.ShowOuterWalls;
        }

        /// <summary>
        /// 내부 디버프 및 환경 효과 트리거 구역을 하이라키에 인스턴싱하는 헬퍼 함수
        /// </summary>
        private void BuildZoneFace(string zoneName, Vector3 targetLocalCenter, Vector3 targetLocalSize, Color zoneColor, Transform parentFolder)
        {
            GameObject zoneObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            zoneObj.name = zoneName;
            zoneObj.transform.SetParent(parentFolder);

            Vector3 pScale = _director.transform.localScale;

            // 조부모(디렉터 본체)의 변형 수치 곱을 우아하게 역산 상쇄하는 공식 매핑
            zoneObj.transform.localPosition = new Vector3(targetLocalCenter.x / pScale.x, targetLocalCenter.y / pScale.y, targetLocalCenter.z / pScale.z);
            zoneObj.transform.localScale = new Vector3(targetLocalSize.x / pScale.x, targetLocalSize.y / pScale.y, targetLocalSize.z / pScale.z);

            if (zoneObj.TryGetComponent(out BoxCollider bc)) bc.isTrigger = true; // 플레이어 진입 체크 센서이므로 트리거 ON
            if (zoneObj.TryGetComponent(out MeshRenderer mr))
            {
                mr.enabled = _director.ShowInnerZones;
                Material tempMaterial = new Material(Shader.Find("Sprites/Default"));
                tempMaterial.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 0.25f);
                mr.sharedMaterial = tempMaterial;
            }
        }

        private void ResetLocalTransform(Transform target)
        {
            target.localPosition = Vector3.zero;
            target.localRotation = Quaternion.identity;
            target.localScale = Vector3.one;
        }
    }
}