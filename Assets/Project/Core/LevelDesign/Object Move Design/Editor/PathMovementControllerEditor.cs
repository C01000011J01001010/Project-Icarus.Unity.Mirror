#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace CoreEngine.LevelDesign.Editor
{
    /// <summary>
    /// PathMovementController의 씬 뷰 시각화 및 인터랙티브 편집을 담당하는 커스텀 에디터 스크립트
    /// </summary>
    [CustomEditor(typeof(PathMovementController))]
    public class PathMovementControllerEditor : UnityEditor.Editor
    {
        private PathMovementController _controller;
        private Material _blueGhostMat;
        private Material _redGhostMat;

        // 선택된 포인트 인덱스 (-1은 선택 안됨, 0 이상은 points 리스트 인덱스)
        private int _selectedIndex = -1;

        private void OnEnable()
        {
            _controller = (PathMovementController)target;

            _blueGhostMat = CreateGhostMaterial(new Color(0.2f, 0.6f, 1f, 0.35f)); // 일반 경유지 반투명 파란색
            _redGhostMat = CreateGhostMaterial(new Color(1f, 0.2f, 0.2f, 0.4f));   // RootPoint 원점 반투명 빨간색
        }

        /// <summary>
        /// 고스트 렌더링에 사용할 반투명(Transparent) 스탠다드 재질을 동적으로 생성합니다.
        /// </summary>
        private Material CreateGhostMaterial(Color color)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.SetFloat("_Mode", 3); // Transparent blend mode
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            mat.color = color;
            return mat;
        }

        private void OnSceneGUI()
        {
            if (_controller == null || _controller.rootPoint == null) return;


            // 🌟 [이곳에 추가] 씬 뷰에서 오브젝트를 직접 이동시킬 때 실시간으로 RootPoint 동기화
            if (!Application.isPlaying && _controller.transform.hasChanged)
            {
                Undo.RecordObject(_controller, "Move Root Point"); // Ctrl+Z(실행취소) 지원
                _controller.SyncRootPointWithTransform();
                _controller.transform.hasChanged = false; // 플래그 초기화 (무한루프 방지)
            }

            MeshFilter[] childMeshes = _controller.GetComponentsInChildren<MeshFilter>();

            // 1. Root Point (시작 원점) 고스트 상시 시각화 (핸들 없음)
            Vector3 rootPos = DrawRootPoint(childMeshes);

            // 2. Target Points 경유지 리스트 시각화 (고스트/선 상시 표시, 클릭 선택 시 핸들 노출)
            Vector3 lastPos = DrawPathPoints(rootPos, childMeshes);

            // 3. LoopMode 조건에 따른 루프 연결선 시각화
            DrawLoopConnections(lastPos, rootPos);
        }

        #region Scene Drawing Sub-Routines
        /// <summary>
        /// RootPoint 위치에 빨간색 고스트 메쉬를 그리고 해당 월드 좌표를 반환합니다.
        /// </summary>
        private Vector3 DrawRootPoint(MeshFilter[] childMeshes)
        {
            Vector3 rootPos = _controller.GetWorldPosition(_controller.rootPoint);
            Quaternion rootRot = _controller.GetWorldRotation(_controller.rootPoint);

            // RootPoint 반투명 빨간색 고스트 상시 표시
            RenderGhostMesh(rootPos, rootRot, _redGhostMat, childMeshes);

            return rootPos;
        }

        /// <summary>
        /// 등록된 모든 경유 지점들의 고스트 메쉬, 경로선, 방향 원뿔을 상시 표시하고 선택된 지점의 핸들을 렌더링합니다.
        /// </summary>
        private Vector3 DrawPathPoints(Vector3 rootPos, MeshFilter[] childMeshes)
        {
            Vector3 prevPos = rootPos;
            bool isPingPong = _controller.loopMode == LoopMode.PingPong;

            if (_controller.points == null) return prevPos;

            for (int i = 0; i < _controller.points.Count; i++)
            {
                TargetPoint pt = _controller.points[i];
                Vector3 worldPos = _controller.GetWorldPosition(pt);
                Quaternion worldRot = _controller.GetWorldRotation(pt);

                // A. 경로 연결선 및 진행 방향 원뿔(Cone) 상시 그리기
                Handles.color = Color.yellow;
                Handles.DrawDottedLine(prevPos, worldPos, 4f);
                DrawDirectionCone(prevPos, worldPos, isPingPong);

                // B. 파란색 반투명 고스트 메쉬 상시 렌더링
                RenderGhostMesh(worldPos, worldRot, _blueGhostMat, childMeshes);

                // C. 선택용 구체 핫스팟 버튼
                float btnSize = HandleUtility.GetHandleSize(worldPos) * 0.15f;
                Handles.color = (_selectedIndex == i) ? Color.yellow : Color.cyan;
                if (Handles.Button(worldPos, worldRot, btnSize, btnSize, Handles.SphereHandleCap))
                {
                    _selectedIndex = i;
                    Repaint();
                }

                // D. 지점 라벨 표기
                Handles.Label(worldPos + Vector3.up * 0.5f, $"Point [{i}]");

                // E. 선택된 지점에 대해서만 이동 + 회전 핸들 동시에 표시
                if (_selectedIndex == i)
                {
                    DrawSelectionHandles(pt, worldPos, worldRot, i);
                }

                prevPos = worldPos;
            }

            return prevPos;
        }

        /// <summary>
        /// 선택된 지점에 대하여 PositionHandle 및 RotationHandle을 동시에 표기하고 값 변경을 처리합니다.
        /// </summary>
        private void DrawSelectionHandles(TargetPoint pt, Vector3 worldPos, Quaternion worldRot, int index)
        {
            EditorGUI.BeginChangeCheck();

            Vector3 newPos = worldPos;
            Quaternion newRot = worldRot;

            // 1. Position Handle (Relative 여부에 따라 Local / Global 축 분기)
            Quaternion posHandleRot = _controller.useRelativeCoordinates ? worldRot : Quaternion.identity;
            newPos = Handles.PositionHandle(worldPos, posHandleRot);

            // 2. Rotation Handle (Relative 여부에 따라 Local / Global 축 및 연산 분기)
            if (_controller.useRelativeCoordinates)
            {
                // [Local 모드] 핸들 링이 오브젝트 기울기(worldRot)에 맞춰짐
                newRot = Handles.RotationHandle(worldRot, newPos);
            }
            else
            {
                // [Global 모드] 핸들 링이 월드 정방향 축(Quaternion.identity)에 정렬됨
                Quaternion rawResult = Handles.RotationHandle(Quaternion.identity, newPos);

                // 월드 기준 변화량(Delta)을 기존 worldRot의 왼쪽에 곱해서 월드 축 기준 회전 적용
                Quaternion deltaRot = rawResult; // Quaternion.identity 기준 변화량이므로 rawResult가 곧 deltaRot
                newRot = deltaRot * worldRot;
            }


            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_controller, $"Modify Point [{index}]");
                _controller.SetWorldPositionAndRotation(pt, newPos, newRot);
            }
        }

        /// <summary>
        /// LoopMode(Restart, Circular) 설정에 맞는 루프 복귀선을 렌더링합니다.
        /// </summary>
        private void DrawLoopConnections(Vector3 lastPos, Vector3 rootPos)
        {
            if (_controller.points == null || _controller.points.Count == 0) return;

            if (_controller.loopMode == LoopMode.Restart)
            {
                // 마지막 지점 -> RootPoint 복귀 선 (빨간색)
                Handles.color = Color.red;
                Handles.DrawDottedLine(lastPos, rootPos, 4f);
                DrawDirectionCone(lastPos, rootPos, false);
            }
            else if (_controller.loopMode == LoopMode.Circular && _controller.points.Count > 1)
            {
                // 마지막 지점 -> 0번 지점 순환 선 (초록색)
                Handles.color = Color.green;
                Vector3 firstPos = _controller.GetWorldPosition(_controller.points[0]);
                Handles.DrawDottedLine(lastPos, firstPos, 4f);
                DrawDirectionCone(lastPos, firstPos, false);
            }
        }

        /// <summary>
        /// 자식 메쉬들의 상대적 오프셋을 완벽히 유지하면서 지정된 목표 위치/회전에 고스트 메쉬를 렌더링합니다.
        /// </summary>
        private void RenderGhostMesh(Vector3 targetWorldPos, Quaternion targetWorldRot, Material ghostMat, MeshFilter[] childMeshes)
        {
            if (Event.current.type != EventType.Repaint || childMeshes == null) return;

            ghostMat.SetPass(0);

            Transform rootTransform = _controller.transform;

            // 1. 고스트 부모(Root)의 새로운 월드 변환 행렬 생성 (목표 위치, 목표 회전, 원본 부모 스케일)
            Matrix4x4 ghostRootMatrix = Matrix4x4.TRS(targetWorldPos, targetWorldRot, rootTransform.lossyScale);

            // 2. 현재 부모의 WorldToLocal 행렬 (M_root^-1)
            Matrix4x4 rootWorldToLocal = rootTransform.worldToLocalMatrix;

            foreach (var mf in childMeshes)
            {
                if (mf == null || mf.sharedMesh == null) continue;

                // 3. 부모 대비 자식의 상대 변환 행렬 산출 (M_rel = M_root^-1 * M_child)
                Matrix4x4 childRelativeMatrix = rootWorldToLocal * mf.transform.localToWorldMatrix;

                // 4. 고스트 부모 행렬에 상대 행렬을 곱하여 고스트 자식의 최종 월드 변환 행렬 생성
                // (M_ghostChild = M_ghostRoot * M_rel)
                Matrix4x4 ghostChildMatrix = ghostRootMatrix * childRelativeMatrix;

                // 5. 고스트 자식 메쉬 렌더링 (서브메쉬가 여러 개여도 모두 정상 렌더링)
                for (int s = 0; s < mf.sharedMesh.subMeshCount; s++)
                {
                    Graphics.DrawMeshNow(mf.sharedMesh, ghostChildMatrix, s);
                }
            }
        }

        /// <summary>
        /// 두 지점 사이의 진행 방향을 나타내는 원뿔(Cone) 기즈모를 그립니다. (PingPong 모드일 경우 양방향)
        /// </summary>
        private void DrawDirectionCone(Vector3 start, Vector3 end, bool isPingPong)
        {
            Vector3 dir = (end - start).normalized;
            if (dir.sqrMagnitude < 0.001f) return;

            Vector3 midPoint = Vector3.Lerp(start, end, 0.5f);
            float coneSize = HandleUtility.GetHandleSize(midPoint) * 0.2f;

            // 정방향 화살표 원뿔
            Handles.ConeHandleCap(0, midPoint, Quaternion.LookRotation(dir), coneSize, EventType.Repaint);

            // 핑퐁 모드일 경우 역방향 화살표 원뿔 추가
            if (isPingPong)
            {
                Handles.ConeHandleCap(0, midPoint, Quaternion.LookRotation(-dir), coneSize, EventType.Repaint);
            }
        }
        #endregion
    }
}
#endif