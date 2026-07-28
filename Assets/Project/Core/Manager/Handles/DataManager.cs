using System;
using System.IO;
using System.Collections;
using UnityEngine;
using CoreEngine.Data; // PathManager 사용
using CoreEngine.Utility; // BinarySerializer 사용

namespace CoreEngine.Manager
{
    public class DataManager : BaseManager
    {
        public GraphicOptionValues SavedGraphicOption { get; private set; }

        public override IEnumerator Initialize()
        {
            LoadLocalOption();
            yield break;
        }

        // ----------------------------------------------------------------------------------
        // [파일 IO 로직]
        // ----------------------------------------------------------------------------------
        public void SaveFile(string directory, string fileName, params byte[] data)
        {
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            string totalPath = Path.Combine(directory, fileName);

            if (!File.Exists(totalPath)) File.Create(totalPath).Close();
            File.WriteAllBytes(totalPath, data);
        }

        public byte[] LoadFile(string directory, string fileName)
        {
            string totalPath = Path.Combine(directory, fileName);
            return File.Exists(totalPath) ? File.ReadAllBytes(totalPath) : null;
        }

        // ----------------------------------------------------------------------------------
        // [옵션 세이브/로드]
        // ----------------------------------------------------------------------------------
        private void LoadLocalOption()
        {
            try
            {
                byte[] savedData = LoadFile(PathManager.OptionPath, PathManager.GraphicSettingsFileName);
                if (savedData != null)
                {
                    SavedGraphicOption = savedData.ByteArray2Struct<GraphicOptionValues>();
                }
                else
                {
                    SavedGraphicOption = GraphicOptionValues.defaultOption;
                }
            }
            catch (Exception ex)
            {
                SavedGraphicOption = GraphicOptionValues.defaultOption;
                Debug.LogWarning($"[DataManager] 옵션 로드 실패, 기본값 적용: {ex.Message}");
            }
        }

#if UNITY_EDITOR
        public void TestLocalOptionSave()
        {
            GraphicOptionValues options = GraphicOptionValues.testOption;
            SaveFile(PathManager.OptionPath, PathManager.GraphicSettingsFileName, options.Struct2ByteArray());
            Debug.Log("[DataManager] 테스트 옵션 저장 완료");
        }
#endif
    }
}