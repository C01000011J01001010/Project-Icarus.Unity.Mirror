using UnityEditor;
using UnityEngine;

namespace CoreEngine.Environment
{
    /// <summary>
    /// SquareSpaceZone 컴포넌트의 유니티 인스펙터 UI를 커스터마이징하고 툴링을 제공하는 에디터 클래스
    /// </summary>
    [CustomEditor(typeof(SquareSpaceZone))]
    public class SquareSpaceZoneEditor : Editor
    {
        private SquareSpaceZone _zone;

        private void OnEnable()
        {
            // 인스펙터가 활성화될 때 대상 컴포넌트를 캐싱합니다.
            _zone = (SquareSpaceZone)target;
        }

        /// <summary>
        /// 인스펙터 창의 GUI를 그리는 핵심 메서드 (매 프레임 호출됨)
        /// </summary>
        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space(5);

            // 1. 유니티 표준 HelpBox를 사용한 직관적인 가이드라인
            EditorGUILayout.HelpBox(
                "Transform의 Scale 핸들(기즈모)을 드래그하여 클램핑 공간의 크기를 자유롭게 설정할 수 있습니다.\n" +
                "확정 버튼을 누르면 외곽 투명벽이 자동으로 갱신됩니다.",
                MessageType.Info
            );
            EditorGUILayout.Space(5);

            // 2. 동기화된 데이터 읽기 전용으로 표시
            EditorGUILayout.LabelField("📊 현재 공간 상태", EditorStyles.boldLabel);

            // 🌟 EditorGUI.DisabledGroupScope: 블록 안의 UI를 회색으로 잠금 처리 (Read-Only)
            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.Vector3Field("동기화된 공간 크기 (Size)", _zone.ZoneSize);
            }

            EditorGUILayout.Space(10);

            // 3. 에디터 툴링 옵션 (메쉬 토글)
            EditorGUILayout.LabelField("⚙️ 시각화 및 편집 옵션", EditorStyles.boldLabel);

            // 🌟 BeginChangeCheck ~ EndChangeCheck: 이 구간 안에서 UI 조작이 일어났는지 감지
            EditorGUI.BeginChangeCheck();
            bool toggleState = EditorGUILayout.Toggle("외부벽 가이드 메쉬 활성화", _zone.ShowWallMeshes);

            if (EditorGUI.EndChangeCheck())
            {
                // 사용자가 토글을 클릭했다면, 이 변경사항을 '실행 취소(Ctrl+Z)' 리스트에 등록
                Undo.RecordObject(_zone, "Toggle Wall Visuals");
                _zone.ShowWallMeshes = toggleState; // 프로퍼티를 통해 렌더러 즉각 갱신
            }

            EditorGUILayout.Space(12);

            // 4. 실행 버튼 배정 (가시성 높은 대형 버튼)
            if (GUILayout.Button("🧱 외곽 투명벽 빌드 / 최신화", GUILayout.Height(38)))
            {
                GenerateOuterWalls();
            }
            EditorGUILayout.Space(5);
        }

        /// <summary>
        /// 수학적 계산을 통해 부모 공간의 크기에 맞춘 6개의 두께 1짜리 물리 벽을 생성합니다.
        /// </summary>
        private void GenerateOuterWalls()
        {
            // 1. 본체(부모) 콜라이더 검증 및 초기화
            BoxCollider boxCollider = _zone.GetComponent<BoxCollider>();
            if (boxCollider == null) boxCollider = _zone.gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = Vector3.one;

            _zone.transform.localScale = _zone.ZoneSize; // 스케일 동기화 보장

            // 2. 멱등성(Idempotency) 보장: 기존에 만든 벽이 있다면 완전히 삭제 후 재생성
            Transform existingContainer = _zone.transform.Find(SquareSpaceZone.WALL_CONTAINER_NAME);
            if (existingContainer != null)
            {
                // 에디터 모드에서의 삭제는 반드시 Undo.DestroyObjectImmediate를 사용해야 에러가 없습니다.
                Undo.DestroyObjectImmediate(existingContainer.gameObject);
            }

            // 3. 자식들을 담을 부모 컨테이너 객체 생성
            GameObject containerObj = new GameObject(SquareSpaceZone.WALL_CONTAINER_NAME);
            Undo.RegisterCreatedObjectUndo(containerObj, "Create Wall Container"); // Ctrl+Z 대응

            Transform container = containerObj.transform;
            container.SetParent(_zone.transform);
            container.localPosition = Vector3.zero;
            container.localRotation = Quaternion.identity;

            // 중요: 컨테이너의 로컬 스케일은 무조건 1로 고정하여 수학 연산의 꼬임을 방지합니다.
            container.localScale = Vector3.one;

            Vector3 pSize = _zone.ZoneSize;

            // 4. 6개의 면체 조립
            // [수학적 원리]
            // 부모의 크기가 pSize일 때, 로컬 좌표계에서 표면은 0.5 위치에 있습니다.
            // 벽의 두께가 1이 되려면 로컬 스케일은 (1 / pSize)가 되어야 부모의 스케일 곱을 상쇄시킵니다.
            // 위치는 표면(0.5)에서 벽 두께의 절반(0.5 / pSize)만큼 밖으로 밀어내야 완벽하게 맞닿습니다.

            BuildWallFace("Wall_Left", new Vector3(-0.5f - (0.5f / pSize.x), 0, 0), new Vector3(1f / pSize.x, 1f, 1f), container);
            BuildWallFace("Wall_Right", new Vector3(0.5f + (0.5f / pSize.x), 0, 0), new Vector3(1f / pSize.x, 1f, 1f), container);
            BuildWallFace("Wall_Bottom", new Vector3(0, -0.5f - (0.5f / pSize.y), 0), new Vector3(1f, 1f / pSize.y, 1f), container);
            BuildWallFace("Wall_Top", new Vector3(0, 0.5f + (0.5f / pSize.y), 0), new Vector3(1f, 1f / pSize.y, 1f), container);
            BuildWallFace("Wall_Back", new Vector3(0, 0, -0.5f - (0.5f / pSize.z)), new Vector3(1f, 1f, 1f / pSize.z), container);
            BuildWallFace("Wall_Front", new Vector3(0, 0, 0.5f + (0.5f / pSize.z)), new Vector3(1f, 1f, 1f / pSize.z), container);

            // 5. 씬 변경사항 마킹 (Ctrl+S를 눌렀을 때 이 변경사항이 저장되도록 엔진에 알림)
            EditorUtility.SetDirty(_zone);
            _zone.UpdateWallVisuals(); // 생성 직후 토글 상태에 맞춰 메쉬 가시성 적용

            Debug.Log($"<color=green><b>[SquareSpace]</b></color> 투명벽 물리 표면 동기화 완료: {pSize}");
        }

        /// <summary>
        /// 단일 외곽벽(Cube) 오브젝트를 생성하고 설정하는 헬퍼 메서드
        /// </summary>
        private void BuildWallFace(string faceName, Vector3 localPos, Vector3 localScale, Transform parent)
        {
            // 유니티 내장 Cube 메쉬와 BoxCollider를 기본 탑재한 오브젝트 생성
            GameObject faceObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            faceObj.name = faceName;

            Transform t = faceObj.transform;
            t.SetParent(parent);
            t.localPosition = localPos;
            t.localRotation = Quaternion.identity;
            t.localScale = localScale;

            // 플레이어가 밖으로 나가지 못하게 막는 진짜 "벽"이므로 Trigger가 꺼져있어야 함
            BoxCollider bc = faceObj.GetComponent<BoxCollider>();
            if (bc != null) bc.isTrigger = false;

            // 현재 가이드 토글 상태에 따라 메쉬 렌더러를 On/Off 세팅
            MeshRenderer mr = faceObj.GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = _zone.ShowWallMeshes;
        }
    }
}