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

            // 🌟 1. 데이터는 무조건 32개 레이어 분량을 보존합니다 (삭제 안 함)
            SyncAllLayersFixed(settings);

            profileSO.Update();

            // 🌟 1. 경로 설정 UI 호출 추가
            DrawSaveDirectoryGUI(profileSO, settings);
            EditorGUILayout.Space();

            // 1. 일반 및 맵 기본 설정 UI
            DrawGeneralSettingsGUI(profileSO);
            EditorGUILayout.Space();

            // 2. 투영 및 해상도 렌더 설정 UI
            DrawRenderSettingsGUI(profileSO, settings);
            EditorGUILayout.Space();

            // 3. 외곽선(Outline) 설정 UI
            DrawOutlineSettingsGUI(profileSO, settings);

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

        private static void DrawSaveDirectoryGUI(SerializedObject profileSO, MapBakeSettingsSO settings)
        {
            EditorGUILayout.LabelField("저장 경로 설정", EditorStyles.boldLabel);

            // 🌟 [1번째 줄] 버튼 2개 나란히 배치
            EditorGUILayout.BeginHorizontal();

            // 📂 폴더 선택 버튼
            if (GUILayout.Button("📂 저장 폴더 선택", GUILayout.Height(26)))
            {
                string defaultPath = string.IsNullOrEmpty(settings.saveDirectory) ? Application.dataPath : settings.saveDirectory;
                string absPath = EditorUtility.OpenFolderPanel("저장 위치 선택", defaultPath, "");

                if (!string.IsNullOrEmpty(absPath))
                {
                    if (absPath.StartsWith(Application.dataPath))
                    {
                        string relPath = "Assets" + absPath.Substring(Application.dataPath.Length);

                        SerializedProperty saveDirProp = profileSO.FindProperty("saveDirectory");
                        saveDirProp.stringValue = relPath;
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("경로 오류", "프로젝트 내부(Assets 폴더 하위)를 선택해야 합니다.", "확인");
                    }
                }
            }

            // 🔍 위치 확인 (Ping) 버튼
            if (GUILayout.Button("🔍 위치 확인 (Ping)", GUILayout.Height(26)))
            {
                if (!string.IsNullOrEmpty(settings.saveDirectory))
                {
                    UnityEngine.Object folderObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(settings.saveDirectory);
                    if (folderObj != null)
                    {
                        // 🌟 인스펙터 고정을 위해 Selection.activeObject 변경을 제거하고 Ping만 수행!
                        EditorGUIUtility.PingObject(folderObj);
                    }
                    else
                    {
                        Debug.LogWarning($"[MapBaker] '{settings.saveDirectory}' 경로의 폴더 에셋을 찾을 수 없습니다.");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            // 🌟 [2번째 줄] 저장 경로 표시 (DisabledGroup으로 키보드 수정 완전 차단)
            EditorGUI.BeginDisabledGroup(true);
            SerializedProperty prop = profileSO.FindProperty("saveDirectory");
            EditorGUILayout.PropertyField(prop, new GUIContent("현재 저장 경로"));
            EditorGUI.EndDisabledGroup();
        }

        private static void DrawGeneralSettingsGUI(SerializedObject profileSO)
        {
            EditorGUILayout.PropertyField(profileSO.FindProperty("showInteractiveGizmo"), new GUIContent("씬 뷰 화살표 핸들 켜기"));
            EditorGUILayout.PropertyField(profileSO.FindProperty("showCameraGizmo"), new GUIContent("씬 뷰 카메라 방향 켜기"));

            EditorGUILayout.Space(5);
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

            // *요청 반영: Step이 2단계 이상일 때만 하한선 밝기 세팅 노출
            if (settings.depthSteps >= MapDepthSteps.Step_2)
            {
                EditorGUILayout.PropertyField(profileSO.FindProperty("finalDepthBrightness"), new GUIContent("가장 깊은 곳 밝기 제한"));
            }

            EditorGUILayout.PropertyField(profileSO.FindProperty("resolution"), new GUIContent("타일 해상도"));
            EditorGUILayout.PropertyField(profileSO.FindProperty("renderMask"), new GUIContent("렌더링 마스크"));
            EditorGUILayout.PropertyField(profileSO.FindProperty("backgroundColor"), new GUIContent("배경 색상"));

            // *요청 반영: depthSteps가 None이면 layerColors(색상 사용) 옵션 자체를 숨깁니다.
            if (settings.depthSteps != MapDepthSteps.None)
            {
                SerializedProperty useLayerColorProp = profileSO.FindProperty("useLayerColor");
                EditorGUILayout.PropertyField(useLayerColorProp, new GUIContent("레이어별 색상 사용"));

                if (useLayerColorProp.boolValue)
                {
                    EditorGUILayout.HelpBox("RenderMask에 포함된 레이어별로 고유 색상을 지정합니다.", MessageType.Info);
                    DrawLayerColorPalette(profileSO.FindProperty("layerColors"), settings);
                }
                else
                {
                    EditorGUILayout.HelpBox("단색 모드입니다. 캡처된 오브젝트가 단일 색상으로 렌더링됩니다.", MessageType.Info);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("None 상태입니다. 씬의 원본 머티리얼과 텍스처를 그대로 사용하여 캡처합니다.", MessageType.Info);
            }
        }

        // 외곽선 세팅도 렌더마스크에 포함된 것만 추출해서 그립니다.
        private static void DrawOutlineSettingsGUI(SerializedObject profileSO, MapBakeSettingsSO settings)
        {
            EditorGUILayout.LabelField("외곽선 설정", EditorStyles.label);
            EditorGUILayout.HelpBox("체크된 레이어의 테두리에 외곽선을 생성", MessageType.Info);

            SerializedProperty outlineSettingsProp = profileSO.FindProperty("outlineSettings");

            for (int i = 0; i < 32; i++)
            {
                if ((settings.renderMask.value & (1 << i)) != 0 && !string.IsNullOrEmpty(LayerMask.LayerToName(i)))
                {
                    if (i < outlineSettingsProp.arraySize)
                    {
                        SerializedProperty itemProp = outlineSettingsProp.GetArrayElementAtIndex(i);
                        EditorGUILayout.PropertyField(itemProp); // 커스텀 Drawer가 이 부분을 예쁘게 그려줌
                    }
                }
            }
        }

        // 렌더마스크에 포함된 인덱스만 반복문을 돌며 인스펙터에 그림
        private static void DrawLayerColorPalette(SerializedProperty layerColorsProp, MapBakeSettingsSO settings)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < 32; i++)
            {
                // 렌더마스크에 포함되어 있고 이름이 존재하는 레이어만 표시
                if ((settings.renderMask.value & (1 << i)) != 0 && !string.IsNullOrEmpty(LayerMask.LayerToName(i)))
                {
                    if (i < layerColorsProp.arraySize)
                    {
                        SerializedProperty pair = layerColorsProp.GetArrayElementAtIndex(i);
                        SerializedProperty nameProp = pair.FindPropertyRelative("layerName");
                        SerializedProperty colorProp = pair.FindPropertyRelative("color");

                        EditorGUILayout.PropertyField(colorProp, new GUIContent(nameProp.stringValue));
                    }
                }
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
            // 🌟 경로가 비어있으면 경고 띄우고 중단
            if (string.IsNullOrEmpty(settings.saveDirectory))
            {
                EditorUtility.DisplayDialog("저장 실패", "저장 경로가 비어있습니다. '찾기' 버튼을 눌러 경로를 지정해주세요.", "확인");
                return;
            }

            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(sceneName)) sceneName = "UntitledScene";

            // 🌟 하드코딩 제거, 사용자가 지정한 경로 사용
            string dirPath = settings.saveDirectory;
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
            gridData.saveDirectory = settings.saveDirectory;
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

            // 🌟 1. 폴더 선택 창 대신 파일 저장 창(SaveFilePanelInProject)을 띄움
            // 이 함수는 "Assets/..." 형태의 상대 경로를 반환하며, 이름까지 지정할 수 있습니다.
            string savePath = EditorUtility.SaveFilePanelInProject(
                "새 세팅 프로필 저장",
                $"MapBakeSettings_{sceneName}", // 기본 파일명 지정
                "asset",                     // 확장자 고정
                "저장할 위치와 파일명을 지정하세요."
            );

            // 사용자가 취소를 눌렀을 경우
            if (string.IsNullOrEmpty(savePath)) return null;

            // 새로운 SO 인스턴스 메모리에 생성
            MapBakeSettingsSO newSettings = ScriptableObject.CreateInstance<MapBakeSettingsSO>();

            // 🌟 2. 파일 경로(savePath)에서 폴더 경로(Directory)만 추출하여 세팅에 저장
            // 윈도우 환경에서 백슬래시(\)가 나올 수 있으므로 슬래시(/)로 치환하여 유니티 표준에 맞춥니다.
            string dirPath = System.IO.Path.GetDirectoryName(savePath).Replace("\\", "/");
            newSettings.saveDirectory = dirPath;

            // 🌟 3. 에셋 생성 및 디스크에 저장
            AssetDatabase.CreateAsset(newSettings, savePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 🌟 4. 인스펙터 고정: Selection을 건드리지 않고 해당 에셋을 반짝(Ping)거리게만 만듦
            EditorGUIUtility.PingObject(newSettings);

            Debug.Log($"[MapBaker] 새 세팅 프로필 에셋이 생성되었습니다: {savePath}");
            return newSettings;
        }

        // 🌟 4. 데이터 보존의 핵심! 무조건 32개의 배열 공간을 유지시킵니다.
        private static void SyncAllLayersFixed(MapBakeSettingsSO settings)
        {
            bool isChanged = false;

            // 크기가 32개가 아니면 강제로 맞춤
            while (settings.layerColors.Count < 32) { settings.layerColors.Add(new LayerColorPair()); isChanged = true; }
            while (settings.outlineSettings.Count < 32) { settings.outlineSettings.Add(new LayerOutlineSetting()); isChanged = true; }

            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);

                // =================================================================
                // 🚨 [수정된 부분] 구조체(struct)는 꺼내서 수정한 뒤 다시 덮어씌워야 합니다!
                // =================================================================
                LayerColorPair tempColorPair = settings.layerColors[i];
                bool colorPairChanged = false;

                if (tempColorPair.layerName != layerName)
                {
                    tempColorPair.layerName = layerName;
                    colorPairChanged = true;
                }

                if (tempColorPair.color == default(Color))
                {
                    tempColorPair.color = Color.white;
                    colorPairChanged = true;
                }

                // 변경사항이 있다면 수정한 임시 구조체를 리스트에 통째로 다시 할당
                if (colorPairChanged)
                {
                    settings.layerColors[i] = tempColorPair;
                    isChanged = true;
                }

                // =================================================================
                // 클래스(class)인 OutlineSetting은 원본이 바로 참조되므로 직접 수정 가능
                // =================================================================
                if (settings.outlineSettings[i].layerName != layerName)
                {
                    settings.outlineSettings[i].layerName = layerName;
                    if (settings.outlineSettings[i].outlineColor == default(Color))
                    {
                        settings.outlineSettings[i].outlineColor = Color.black;
                        settings.outlineSettings[i].outlineColor.a = 1f;
                        settings.outlineSettings[i].outlineThickness = 2;
                        settings.outlineSettings[i].depthThreshold = 0.001f;
                    }
                    isChanged = true;
                }
            }

            if (isChanged)
            {
                EditorUtility.SetDirty(settings);
            }
        }

        #endregion
    }
}
#endif