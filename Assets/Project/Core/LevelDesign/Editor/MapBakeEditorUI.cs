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
        #region [1] 최상위 UI 진입점 (Main Entry Points)

        public static void DrawDefaultGUI(SerializedObject viewObject, ref SerializedObject profileSO, ref MapBakeSettingsSO settings, Action<MapBakeSettingsSO> onProfileCreated)
        {
            viewObject.Update();
            EditorGUILayout.PropertyField(viewObject.FindProperty("settingsProfile"), new GUIContent("세팅 프로필 (Profile SO)"));
            viewObject.ApplyModifiedProperties();

            // 최신 세팅 값 동기화
            settings = viewObject.FindProperty("settingsProfile").objectReferenceValue as MapBakeSettingsSO;

            if (settings == null)
            {
                DrawProfileCreationPrompt(onProfileCreated);
                return;
            }

            if (profileSO == null || profileSO.targetObject != settings)
            {
                profileSO = new SerializedObject(settings);
            }
        }

        public static void DrawSharedGUI(SerializedObject profileSO, MapBakeSettingsSO settings)
        {
            if (profileSO == null || settings == null) return;

            // 레이어 색상 갱신
            if (settings.useLayerColor) SyncProjectLayers(settings);

            profileSO.Update();

            // 1. 일반 및 맵 기본 설정 UI
            DrawGeneralSettingsGUI(profileSO);
            EditorGUILayout.Space();

            // 2. 투영 및 해상도 렌더 설정 UI
            DrawRenderSettingsGUI(profileSO, settings);
            EditorGUILayout.Space();

            // 3. 외곽선(Outline) 설정 UI
            DrawOutlineSettingsGUI(profileSO);

            profileSO.ApplyModifiedProperties();

            // 4. 베이킹 실행 버튼 UI
            EditorGUILayout.Space(15);
            DrawBakeActionGUI(settings);
        }

        #endregion


        #region [2] 세부 UI 드로잉 영역 (Sub UI Methods)

        private static void DrawProfileCreationPrompt(Action<MapBakeSettingsSO> onProfileCreated)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("할당된 세팅 프로필 에셋이 없습니다. 새로 생성하시겠습니까?", MessageType.Warning);

            GUI.backgroundColor = new Color(0.4f, 0.8f, 1f);
            if (GUILayout.Button("✨ 새로운 세팅 프로필 에셋 생성 (Create Profile)", GUILayout.Height(35)))
            {
                MapBakeSettingsSO newSettings = CreateNewProfileAsset();
                if (newSettings != null)
                {
                    onProfileCreated?.Invoke(newSettings);
                }
            }
            GUI.backgroundColor = Color.white;
        }

        private static void DrawGeneralSettingsGUI(SerializedObject profileSO)
        {
            EditorGUILayout.LabelField("기본 및 맵 크기 설정", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(profileSO.FindProperty("mapDimension"), new GUIContent("게임 차원 모드"));
            EditorGUILayout.PropertyField(profileSO.FindProperty("projectionPlane"), new GUIContent("투영 평면"));
            EditorGUILayout.PropertyField(profileSO.FindProperty("centerPosition"), new GUIContent("월드 중심 좌표"));
            EditorGUILayout.PropertyField(profileSO.FindProperty("totalMapSize"), new GUIContent("전체 맵 크기"));
            EditorGUILayout.PropertyField(profileSO.FindProperty("tileSize"), new GUIContent("타일 분할 크기"));
        }

        private static void DrawRenderSettingsGUI(SerializedObject profileSO, MapBakeSettingsSO settings)
        {
            EditorGUILayout.LabelField("렌더링 및 캡처 설정", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(profileSO.FindProperty("captureOffset"), new GUIContent("카메라 렌더 깊이 (Offset)"));
            EditorGUILayout.PropertyField(profileSO.FindProperty("maxDepth"), new GUIContent("최대 캡처 깊이"));
            EditorGUILayout.PropertyField(profileSO.FindProperty("depthSteps"), new GUIContent("명도 양자화 단계"));
            EditorGUILayout.PropertyField(profileSO.FindProperty("resolution"), new GUIContent("타일 해상도"));
            EditorGUILayout.PropertyField(profileSO.FindProperty("renderMask"), new GUIContent("렌더링 마스크"));
            EditorGUILayout.PropertyField(profileSO.FindProperty("backgroundColor"), new GUIContent("배경 색상"));

            SerializedProperty useLayerColorProp = profileSO.FindProperty("useLayerColor");
            EditorGUILayout.PropertyField(useLayerColorProp, new GUIContent("레이어별 색상 사용"));

            if (useLayerColorProp.boolValue)
            {
                EditorGUILayout.HelpBox("프로젝트에 등록된 레이어별로 고유 색상을 지정합니다.", MessageType.Info);
                DrawLayerColorPalette(profileSO.FindProperty("layerColors"));
            }
            else
            {
                string helpMsg = settings.depthSteps != MapDepthSteps.None
                    ? "흑백 모드입니다. 지형 깊이(Depth)에 따라 명도가 조절됩니다."
                    : "단색 모드입니다. 캡처된 오브젝트가 깊이와 상관없이 단일 색상으로 렌더링됩니다.";
                EditorGUILayout.HelpBox(helpMsg, MessageType.Info);
            }
        }

        private static void DrawOutlineSettingsGUI(SerializedObject profileSO)
        {
            SerializedProperty outlineSettingsProp = profileSO.FindProperty("outlineSettings");
            EditorGUILayout.PropertyField(outlineSettingsProp, new GUIContent("외곽선 대상 레이어 목록"), true);

            if (outlineSettingsProp.arraySize > 0)
            {
                EditorGUILayout.HelpBox("추가된 레이어의 오브젝트 테두리에만 외곽선이 렌더링됩니다. (중복 레이어 자동 취소)", MessageType.None);
            }
        }

        private static void DrawLayerColorPalette(SerializedProperty layerColorsProp)
        {
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

        private static void DrawBakeActionGUI(MapBakeSettingsSO settings)
        {
            EditorGUILayout.HelpBox($"총 생성될 타일 개수: {settings.Cols} x {settings.Rows} = {settings.Cols * settings.Rows}개", MessageType.Info);

            if (GUILayout.Button("🚀 전체 그리드 맵 굽기", GUILayout.Height(40)))
            {
                BakeGridMap(settings);
            }
        }

        #endregion


        #region [3] 베이킹 프로세스 및 파일 입출력 (Bake Execution & IO)

        private static void BakeGridMap(MapBakeSettingsSO settings)
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(sceneName)) sceneName = "UntitledScene";

            string dirPath = $"Assets/GameData/Maps/{sceneName}";
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

            try
            {
                // 1. 개별 타일들 굽기
                BakeAndSaveTiles(settings, dirPath);

                // 2. 전체 맵(LOD) 굽기 및 저장
                string lodPath = BakeAndSaveFullLOD(settings, dirPath, sceneName);

                // 3. GridData SO 업데이트 및 연결
                if (!string.IsNullOrEmpty(lodPath))
                {
                    UpdateMapGridDataSO(settings, dirPath, sceneName, lodPath);
                }

                Debug.Log($"[MapBaker] 맵 베이킹 완료! (타일 {settings.Cols * settings.Rows}개 및 LOD 갱신 완료)");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static void BakeAndSaveTiles(MapBakeSettingsSO settings, string dirPath)
        {
            int totalCount = settings.Cols * settings.Rows;

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
        }

        private static string BakeAndSaveFullLOD(MapBakeSettingsSO settings, string dirPath, string sceneName)
        {
            EditorUtility.DisplayProgressBar("맵 타일 굽는 중...", "전체 맵 LOD 텍스처 생성 중...", 0.99f);

            Texture2D lodTex = MapBakeProcessor.CaptureFullMapLOD(settings, 2048);
            if (lodTex == null) return null;

            string lodPath = $"{dirPath}/{sceneName}_FullLOD.png";
            File.WriteAllBytes(lodPath, lodTex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(lodTex);

            AssetDatabase.Refresh();

            // 텍스처를 Sprite 타입으로 자동 변환
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(lodPath);
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
            }

            return lodPath;
        }

        private static void UpdateMapGridDataSO(MapBakeSettingsSO settings, string dirPath, string sceneName, string lodPath)
        {
            string soPath = $"{dirPath}/{sceneName}_MapGridData.asset";
            MapGridDataSO gridData = AssetDatabase.LoadAssetAtPath<MapGridDataSO>(soPath);

            // 없으면 새로 생성
            if (gridData == null)
            {
                gridData = ScriptableObject.CreateInstance<MapGridDataSO>();
                AssetDatabase.CreateAsset(gridData, soPath);
            }

            // 데이터 갱신
            gridData.sceneName = sceneName;
            gridData.totalCols = settings.Cols;
            gridData.totalRows = settings.Rows;
            gridData.tileSize = settings.tileSize;

            Vector2 extents = new Vector2(settings.totalMapSize.x / 2f, settings.totalMapSize.y / 2f);
            gridData.worldMinBounds = new Vector2(settings.centerPosition.x - extents.x, settings.centerPosition.z - extents.y);
            gridData.worldMaxBounds = new Vector2(settings.centerPosition.x + extents.x, settings.centerPosition.z + extents.y);

            // LOD 이미지 참조 연결
            gridData.fullMapLOD = AssetDatabase.LoadAssetAtPath<Texture2D>(lodPath);

            EditorUtility.SetDirty(gridData);
            AssetDatabase.SaveAssets();
        }

        #endregion


        #region [4] 에셋 유틸리티 및 레이어 동기화 (Utilities)

        private static MapBakeSettingsSO CreateNewProfileAsset()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(sceneName)) sceneName = "UntitledScene";

            string dirPath = $"Assets/GameData/Maps/{sceneName}";

            // 외부 유틸리티 함수(CoreEditor.Utility)를 통해 폴더 및 에셋 자동 생성
            MapBakeSettingsSO newSettings = CoreEditor.Utility.CreateAssetAtFolder<MapBakeSettingsSO>(dirPath, $"{sceneName}_BakeSettings");

            Debug.Log($"[MapBaker] 새 세팅 프로필 에셋이 생성되었습니다: {dirPath}");
            return newSettings;
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

            // 레이어 변경 감지
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

            // 변경사항이 있을 경우에만 초기화 후 갱신
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

        #endregion
    }
}
#endif