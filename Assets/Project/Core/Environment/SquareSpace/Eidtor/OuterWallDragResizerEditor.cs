using UnityEditor;
using UnityEngine;

namespace Core.Environment
{
    /// <summary>
    /// OuterFolder 산하의 실질 외벽 자식들의 표면에 접근하여 단일 방향 조작 화살표를 배치하고,
    /// 유니티 표준 색상(XYZ-RGB)을 바인딩하여 시각적 직관성을 극대화한 커스텀 에디터 클래스입니다.
    /// </summary>
    [CustomEditor(typeof(OuterWallDragResizer))]
    public class OuterWallDragResizerEditor : Editor
    {
        private OuterWallDragResizer _resizer;

        private void OnEnable()
        {
            _resizer = (OuterWallDragResizer)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("🖱️ 외벽 표면 단일 화살표 드래그 제어기", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "씬 뷰(Scene View)에서 직사각형의 각 표면 정중앙을 보세요.\n" +
                "오직 바깥쪽 직각 방향으로 향하는 '단 1개의 깨끗한 화살표'만 활성화됩니다.\n" +
                "유니티 표준 색상(X-빨강, Y-초록, Z-파랑)이 적용되어 직관적인 편집이 가능합니다.",
                MessageType.Info
            );
        }

        /// <summary>
        /// 씬 뷰의 렌더링 파이프라인에서 마우스 피킹 및 6면 색상 Slider 핸들을 제어하는 핵심 함수
        /// </summary>
        private void OnSceneGUI()
        {
            if (_resizer == null) return;

            // 외벽 모듈 상수를 활용해 정적으로 컨테이너 폴더를 다이렉트 탐색 (GetComponent 오버헤드 제로)
            Transform outerFolder = _resizer.transform.Find(OuterWallModule.CONTAINER_NAME);
            if (outerFolder == null) return;

            // 하위 6개 면체를 순회하며 표면에 단방향 슬라이더 인터랙션 그립 부착
            for (int i = 0; i < outerFolder.childCount; i++)
            {
                Transform wallFace = outerFolder.GetChild(i);
                if (!wallFace.gameObject.activeSelf) continue;

                // 🌟 요구사항 반영: 각 면의 이름에 맞는 '고유 직각 로컬 벡터 축'과 '표준 색상'을 정의합니다.
                Vector3 localNormal = Vector3.zero;
                Color handleColor = Color.white; // 기본값

                switch (wallFace.name)
                {
                    case "OuterWall_Left":
                    case "OuterWall_Right":
                        localNormal = (wallFace.name == "OuterWall_Left") ? Vector3.left : Vector3.right;
                        handleColor = Color.red; // X축 관련 -> 빨강
                        break;
                    case "OuterWall_Bottom":
                    case "OuterWall_Top":
                        localNormal = (wallFace.name == "OuterWall_Bottom") ? Vector3.down : Vector3.up;
                        handleColor = Color.green; // Y축 관련 -> 초록
                        break;
                    case "OuterWall_Back":
                    case "OuterWall_Front":
                        localNormal = (wallFace.name == "OuterWall_Back") ? Vector3.back : Vector3.forward;
                        handleColor = Color.blue; // Z축 관련 -> 파랑
                        break;
                }

                // 부모의 회전 행렬을 반영한 진짜 월드 직각 조작 방향 추출
                Vector3 worldPos = wallFace.position;
                Vector3 worldDir = _resizer.transform.TransformDirection(localNormal);

                // 카메라 거리에 상관없이 인지하기 좋은 실무 표준 가시성 배율 크기 처리
                float handleSize = HandleUtility.GetHandleSize(worldPos) * 0.7f;

                // 🌟 핵심: 유니티 렌더링 파이프라인의 색상 상태를 우리가 정의한 축 색상으로 일시 변경합니다.
                Color originalColor = Handles.color;
                Handles.color = handleColor;

                EditorGUI.BeginChangeCheck();

                // 표면 정중앙에서 바깥쪽으로만 뻗어나가는 '단 한 개의 깨끗한 색상 화살표 슬라이더' 렌더링
                Vector3 newWorldPos = Handles.Slider(worldPos, worldDir, handleSize, Handles.ArrowHandleCap, 0.1f);

                if (EditorGUI.EndChangeCheck())
                {
                    // 사용자가 화살표를 드래그한 최종 월드 벡터에서 우리가 지정한 직각 정방향 축 벡터만 내적(Dot Product) 전포 시킵니다.
                    // 이 처리를 통해 다른 축으로 유령 마우스 오차가 스며들어 기하 구조가 깨지는 현상을 원천 차단합니다.
                    float deltaWorld = Vector3.Dot(newWorldPos - worldPos, worldDir);

                    // 유니티 씬 편집 되돌리기(Ctrl+Z) 스냅샷 세션에 변동 행렬 버퍼 등록
                    Undo.RecordObject(_resizer.transform, "Drag Face Move Delta");
                    Undo.RecordObject(_resizer.gameObject.transform, "Anchor Base Shift Vector");

                    // 수식 기반 고무줄 동기화 파이프라인 집행
                    _resizer.ApplyAxisDelta(wallFace.name, deltaWorld);
                }

                // 🌟 중요: 다른 기즈모 렌더링에 영향을 주지 않도록 색상 상태를 원복합니다.
                Handles.color = originalColor;
            }
        }
    }
}