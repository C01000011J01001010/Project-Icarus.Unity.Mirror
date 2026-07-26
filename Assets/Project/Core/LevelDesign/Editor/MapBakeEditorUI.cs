#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CoreEngine.LevelDesign.Editor
{
    public static class MapBakeEditorUI
    {
        public static void DrawDefaultGUI(SerializedObject viewObject, ref SerializedObject profileSO, ref MapBakeSettingsSO settings, Action<MapBakeSettingsSO> onProfileCreated)
        {
            viewObject.Update();
            EditorGUILayout.PropertyField(viewObject.FindProperty("settingsProfile"), new GUIContent("세팅 프로필 (Profile SO)"));
            viewObject.ApplyModifiedProperties();

            // 최신 세팅 값 동기화
            settings = viewObject.FindProperty("settingsProfile").objectReferenceValue as MapBakeSettingsSO;

            if (settings == null)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox("할당된 세팅 프로필 에셋이 없습니다. 새로 생성하시겠습니까?", MessageType.Warning);

                GUI.backgroundColor = new Color(0.4f, 0.8f, 1f);
                if (GUILayout.Button("✨ 새로운 세팅 프로필 에셋 생성 (Create Profile)", GUILayout.Height(35)))
                {
                    MapBakeSettingsSO newSettings = CreateNewProfileAsset();
                    if (newSettings != null)
                    {
                        // 생성된 에셋을 뷰에 전달하여 할당 및 저장 처리
                        onProfileCreated?.Invoke(newSettings);
                    }
                }
                GUI.backgroundColor = Color.white;
                return;
            }

            if (profileSO == null || profileSO.targetObject != settings)
            {
                profileSO = new SerializedObject(settings);
            }
        }

        private static MapBakeSettingsSO CreateNewProfileAsset()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(sceneName)) sceneName = "UntitledScene";

            string dirPath = $"Assets/GameData/Maps/{sceneName}";

            // 유틸리티 함수 호출로 에셋 생성, 저장, 핑(Ping) 수행
            MapBakeSettingsSO newSettings = CoreEditor.Utility.CreateAssetAtFolder<MapBakeSettingsSO>(dirPath, $"{sceneName}_BakeSettings");

            Debug.Log($"[MapBaker] 새 세팅 프로필 에셋이 생성되었습니다.");
            return newSettings;
        }

        public static void DrawSharedGUI(SerializedObject profileSO, MapBakeSettingsSO settings)
        {
            if (profileSO == null || settings == null) return;

            // 1. 레이어 색상 사용 시 사전 동기화
            if (settings.useLayerColor)
            {
                SyncProjectLayers(settings);
            }

            profileSO.Update();

            // 🌟 수동 LabelField를 모두 제거했습니다.
            // SO 필드에 달린 [Header] 속성에 의해 유니티가 헤더를 자동으로 1번만 렌더링합니다.
            EditorGUILayout.PropertyField(profileSO.FindProperty("mapDimension"), new GUIContent("게임 차원 모드"));

            EditorGUILayout.PropertyField(profileSO.FindProperty("projectionPlane"), new GUIContent("투영 평면"));
            EditorGUILayout.PropertyField(profileSO.FindProperty("depthSteps"), new GUIContent("명도 양자화 단계"));

            EditorGUILayout.PropertyField(profileSO.FindProperty("centerPosition"), new GUIContent("월드 중심 좌표"));
            EditorGUILayout.PropertyField(profileSO.FindProperty("totalMapSize"), new GUIContent("전체 맵 크기"));
            EditorGUILayout.PropertyField(profileSO.FindProperty("captureOffset"), new GUIContent("카메라 렌더 깊이 (Offset)"));
            EditorGUILayout.PropertyField(profileSO.FindProperty("maxDepth"), new GUIContent("최대 캡처 깊이"));

            EditorGUILayout.PropertyField(profileSO.FindProperty("tileSize"), new GUIContent("타일 분할 크기"));

            EditorGUILayout.PropertyField(profileSO.FindProperty("resolution"), new GUIContent("타일 해상도"));
            EditorGUILayout.PropertyField(profileSO.FindProperty("renderMask"), new GUIContent("렌더링 마스크"));
            EditorGUILayout.PropertyField(profileSO.FindProperty("backgroundColor"), new GUIContent("배경 색상"));

            SerializedProperty useLayerColorProp = profileSO.FindProperty("useLayerColor");
            EditorGUILayout.PropertyField(useLayerColorProp, new GUIContent("레이어별 색상 사용"));

            if (useLayerColorProp.boolValue)
            {
                EditorGUILayout.HelpBox("프로젝트에 등록된 레이어별로 고유 색상을 지정합니다.", MessageType.Info);

                SerializedProperty layerColorsProp = profileSO.FindProperty("layerColors");
                DrawLayerColorPalette(layerColorsProp);
            }
            else
            {
                if (settings.depthSteps != MapDepthSteps.None)
                {
                    EditorGUILayout.HelpBox("흑백 모드입니다. 지형 깊이(Depth)에 따라 명도가 조절됩니다.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("단색 모드입니다. 캡처된 오브젝트가 깊이와 상관없이 단일 색상으로 렌더링됩니다.", MessageType.Info);
                }
            }

            profileSO.ApplyModifiedProperties();

            GUILayout.Space(15);
            EditorGUILayout.HelpBox($"총 생성될 타일 개수: {settings.Cols} x {settings.Rows} = {settings.Cols * settings.Rows}개", MessageType.Info);

            if (GUILayout.Button("🚀 전체 그리드 맵 굽기", GUILayout.Height(40)))
            {
                BakeGridMap(settings);
            }
        }

        private static void SyncProjectLayers(MapBakeSettingsSO settings)
        {
            string[] projectLayers = InternalEditorUtility.layers;
            if (projectLayers == null || projectLayers.Length == 0) return;

            Dictionary<string, Color> existingColors = new Dictionary<string, Color>();
            if (settings.layerColors != null)
            {
                foreach (var pair in settings.layerColors)
                {
                    if (!string.IsNullOrEmpty(pair.layerName) && !existingColors.ContainsKey(pair.layerName))
                    {
                        existingColors.Add(pair.layerName, pair.color);
                    }
                }
            }
            else
            {
                settings.layerColors = new List<LayerColorPair>();
            }

            bool needSync = settings.layerColors.Count != projectLayers.Length;
            if (!needSync)
            {
                for (int i = 0; i < projectLayers.Length; i++)
                {
                    if (settings.layerColors[i].layerName != projectLayers[i])
                    {
                        needSync = true;
                        break;
                    }
                }
            }

            if (needSync)
            {
                Undo.RecordObject(settings, "Sync Layer Colors");
                settings.layerColors.Clear();
                foreach (string layerName in projectLayers)
                {
                    Color col = existingColors.TryGetValue(layerName, out Color savedCol) ? savedCol : Color.white;
                    settings.layerColors.Add(new LayerColorPair { layerName = layerName, color = col });
                }
                EditorUtility.SetDirty(settings);
            }
        }

        private static void DrawLayerColorPalette(SerializedProperty layerColorsProp)
        {
            EditorGUILayout.LabelField("레이어 색상 팔레트", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            for (int i = 0; i < layerColorsProp.arraySize; i++)
            {
                SerializedProperty pair = layerColorsProp.GetArrayElementAtIndex(i);
                SerializedProperty nameProp = pair.FindPropertyRelative("layerName");
                SerializedProperty colorProp = pair.FindPropertyRelative("color");

                EditorGUILayout.PropertyField(colorProp, new GUIContent(nameProp.stringValue));
            }
            EditorGUI.indentLevel--;
        }

        private static void BakeGridMap(MapBakeSettingsSO settings)
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(sceneName)) sceneName = "UntitledScene";

            string dirPath = $"Assets/GameData/Maps/{sceneName}";
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

            int totalCount = settings.Cols * settings.Rows;

            try
            {
                for (int r = 0; r < settings.Rows; r++)
                {
                    for (int c = 0; c < settings.Cols; c++)
                    {
                        float progress = (float)(r * settings.Cols + c) / totalCount;
                        EditorUtility.DisplayProgressBar("맵 타일 굽는 중...", $"[{c},{r}] 타일 렌더링 중... ({(progress * 100):0.0}%)", progress);

                        Texture2D tileTex = MapBakeProcessor.CaptureTile(settings, c, r);
                        if (tileTex == null) continue;

                        string tilePath = $"{dirPath}/Tile_{c}_{r}.png";
                        File.WriteAllBytes(tilePath, tileTex.EncodeToPNG());
                        UnityEngine.Object.DestroyImmediate(tileTex);
                    }
                }

                EditorUtility.DisplayProgressBar("맵 타일 굽는 중...", "전체 맵 LOD 생성 중...", 0.99f);
                Texture2D lodTex = MapBakeProcessor.CaptureFullMapLOD(settings, 2048);
                if (lodTex != null)
                {
                    string lodPath = $"{dirPath}/{sceneName}_FullLOD.png";
                    File.WriteAllBytes(lodPath, lodTex.EncodeToPNG());
                    UnityEngine.Object.DestroyImmediate(lodTex);
                    AssetDatabase.Refresh();

                    string soPath = $"{dirPath}/{sceneName}_MapGridData.asset";
                    MapGridDataSO gridData = AssetDatabase.LoadAssetAtPath<MapGridDataSO>(soPath);
                    if (gridData == null)
                    {
                        gridData = ScriptableObject.CreateInstance<MapGridDataSO>();
                        AssetDatabase.CreateAsset(gridData, soPath);
                    }

                    gridData.sceneName = sceneName;
                    gridData.totalCols = settings.Cols;
                    gridData.totalRows = settings.Rows;
                    gridData.tileSize = settings.tileSize;
                    gridData.worldMinBounds = new Vector2(settings.centerPosition.x - settings.totalMapSize.x / 2, settings.centerPosition.z - settings.totalMapSize.y / 2);
                    gridData.worldMaxBounds = new Vector2(settings.centerPosition.x + settings.totalMapSize.x / 2, settings.centerPosition.z + settings.totalMapSize.y / 2);

                    TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(lodPath);
                    if (importer != null)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        importer.SaveAndReimport();
                    }

                    gridData.fullMapLOD = AssetDatabase.LoadAssetAtPath<Texture2D>(lodPath);
                    EditorUtility.SetDirty(gridData);
                    AssetDatabase.SaveAssets();
                }

                Debug.Log($"[MapBaker] 맵 베이킹 완료! 총 {totalCount}개 타일 생성 프로세스 종료.");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
#endif