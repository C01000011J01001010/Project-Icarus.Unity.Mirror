using UnityEditor;
using UnityEngine;
using System.IO;

namespace CoreEditor
{
    public static partial class Utility
    {
        /// <summary>
        /// 지정한 경로에 ScriptableObject 에셋을 즉시 생성하고 포커스합니다.
        /// </summary>
        public static T CreateAssetAtFolder<T>(string folderPath, string baseFileName) where T : ScriptableObject
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            T asset = ScriptableObject.CreateInstance<T>();
            string path = $"{folderPath}/{baseFileName}.asset";
            path = AssetDatabase.GenerateUniqueAssetPath(path);

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);

            return asset;
        }
    }
}
