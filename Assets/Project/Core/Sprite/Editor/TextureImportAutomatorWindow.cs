using CoreEngine.Tool;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CoreEngine.EditorTools
{
    public class TextureImportAutomatorWindow : EditorWindow
    {
        private DefaultAsset _targetFolder;         // 드래그 앤 드롭으로 넣을 대상 폴더
        private TextureImportSettingsSO _settings;  // 적용할 설정 SO 파일

        [MenuItem("Tools/Core System/Texture Import Automator")]
        public static void ShowWindow()
        {
            var window = GetWindow<TextureImportAutomatorWindow>("Texture Automator");
            window.minSize = new Vector2(350, 200);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("텍스처 세팅 일괄 자동화 툴", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 1. 타겟 폴더 선택 (DefaultAsset을 이용해 폴더만 드래그 앤 드롭 가능하게 처리)
            _targetFolder = (DefaultAsset)EditorGUILayout.ObjectField(
                "Target Folder (폴더)",
                _targetFolder,
                typeof(DefaultAsset),
                false);

            // 2. 적용할 SO 세팅 파일 선택
            _settings = (TextureImportSettingsSO)EditorGUILayout.ObjectField(
                "Settings SO (설정 파일)",
                _settings,
                typeof(TextureImportSettingsSO),
                false);

            EditorGUILayout.Space(20);

            // 3. 실행 버튼
            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f); // 버튼 색상 강조 (초록색)
            if (GUILayout.Button("폴더 내 모든 텍스처에 설정 적용하기", GUILayout.Height(40)))
            {
                ApplySettingsToFolder();
            }
            GUI.backgroundColor = Color.white; // 색상 초기화
        }

        private void ApplySettingsToFolder()
        {
            // 예외 처리 (방어적 프로그래밍)
            if (_targetFolder == null || _settings == null)
            {
                EditorUtility.DisplayDialog("경고", "폴더와 설정(SO) 파일을 모두 지정해주세요.", "확인");
                return;
            }

            // 폴더 경로 추출
            string folderPath = AssetDatabase.GetAssetPath(_targetFolder);
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                EditorUtility.DisplayDialog("경고", "유효한 폴더가 아닙니다.", "확인");
                return;
            }

            // 폴더 내의 모든 텍스처(t:Texture2D) GUID 검색
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });

            if (guids.Length == 0)
            {
                EditorUtility.DisplayDialog("알림", "해당 폴더에 텍스처(이미지)가 없습니다.", "확인");
                return;
            }

            // 🌟 핵심 최적화: 대량의 에셋을 수정할 때 유니티가 매번 멈추는 것을 방지
            AssetDatabase.StartAssetEditing();

            try
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                    TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

                    if (importer != null)
                    {
                        // 프로그레스 바 표시 (에디터 멈춤 현상 방지용 시각적 피드백)
                        EditorUtility.DisplayProgressBar("텍스처 설정 적용 중...", $"Processing: {Path.GetFileName(assetPath)}", (float)i / guids.Length);

                        // SO의 값들을 Importer에 주입
                        importer.textureType = _settings.textureType;
                        if (importer.textureType == TextureImporterType.Sprite)
                        {
                            importer.spriteImportMode = _settings.spriteMode;
                        }

                        importer.wrapMode = _settings.wrapMode;
                        importer.filterMode = _settings.filterMode;
                        importer.maxTextureSize = _settings.maxTextureSize;
                        importer.textureCompression = _settings.textureCompression;

                        // 수정 사항 저장 및 재구축
                        importer.SaveAndReimport();
                    }
                }
            }
            finally
            {
                // 에러가 나더라도 무조건 실행되어야 하는 안전 장치
                EditorUtility.ClearProgressBar();
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }

            EditorUtility.DisplayDialog("완료", $"총 {guids.Length}개의 텍스처에 세팅을 완료했습니다!", "확인");
        }
    }
}