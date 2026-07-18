using UnityEditor;
using UnityEngine;

namespace Core.Environment
{
    [CustomEditor(typeof(InnerZoneModule))]
    public class InnerZoneModuleEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            InnerZoneModule module = (InnerZoneModule)target;
            SpaceZoneCore core = module.GetComponent<SpaceZoneCore>();

            // 코어가 없다면 임시로 부모의 스케일을 사용 (에러 방지)
            Vector3 pScale = core != null ? core.zoneSize : module.transform.localScale;

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("📐 내부 효과 구역 분할 모듈", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "슬라이더를 조작하여 내부 A, B, C 구역의 크기를 조절합니다.\n" +
                "Zone B는 지정한 폭만큼 중앙을 비우고 양쪽 외곽을 채웁니다.",
                MessageType.None);
            EditorGUILayout.Space(5);

            // 🌟 렉 없는 실시간 조작의 핵심: 안전한 GUI 블록
            EditorGUI.BeginChangeCheck();

            float newAY = EditorGUILayout.Slider("Zone A 시작 Y축 고도", module.ZoneA_StartY, 0f, pScale.y);
            float newBX = EditorGUILayout.Slider("Zone B 중앙 공백 너비", module.ZoneB_StartXAbs, 0f, pScale.x * 0.5f);
            float newCY = EditorGUILayout.Slider("Zone C 종료 Y축 고도", module.ZoneC_EndY, 0f, pScale.y);

            EditorGUILayout.Space(5);
            bool toggle = EditorGUILayout.Toggle("내부 구역 메쉬 가이드 켜기", module.ShowInnerZones);

            if (EditorGUI.EndChangeCheck())
            {
                // 변경 사항을 저장 (Ctrl+Z 지원)
                Undo.RecordObject(module, "Modify Inner Zone Parameters");

                module.ZoneA_StartY = newAY;
                module.ZoneB_StartXAbs = newBX;
                module.ZoneC_EndY = newCY;
                module.ShowInnerZones = toggle;

                // 🌟 값 변경 후, 파괴가 아닌 '수치 덮어쓰기'만 수행하므로 매우 빠름!
                if (core != null)
                    core.TriggerRebuild();
                else
                    module.RebuildZones();
            }

            EditorGUILayout.Space(10);
            if (GUILayout.Button("수동 갱신 (에러 복구용) 🛠️", GUILayout.Height(30)))
            {
                module.RebuildZones();
            }
        }
    }
}