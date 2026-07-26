//using UnityEditor;
//using UnityEngine;
//using System.IO;

//namespace CoreEngine.LevelDesign.Editor
//{
//    [CustomEditor(typeof(MapBaker))]
//    public class MapBakerEditor : UnityEditor.Editor
//    {
//        private SerializedObject _profileSO;

//        private MapBaker baker;

//        private void OnEnable()
//        {
//            baker = (MapBaker)target;
//        }



//        public override void OnInspectorGUI()
//        {
//            // 🌟 공용 UI 호출 (생성 시 바인딩될 동작을 람다식으로 전달)
//            MapBakeEditorUI.DrawDefaultGUI(serializedObject, ref _profileSO, ref baker.settingsProfile, (newSettings) =>
//            {
//                baker.settingsProfile = newSettings;
//                EditorUtility.SetDirty(baker);
//            });

//            if (baker.settingsProfile == null) return;

//            MapBakeEditorUI.DrawSharedGUI(_profileSO, baker.settingsProfile);
//        }

//        private void OnSceneGUI()
//        {
//            if (baker.settingsProfile == null) return;

//            MapBakeGizmoDrawer.DrawWithHandles(baker.settingsProfile);
//        }
//    }
//}
