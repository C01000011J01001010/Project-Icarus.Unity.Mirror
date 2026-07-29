using UnityEngine;
using UnityEditor;

namespace CoreEngine.Tool
{
    [CreateAssetMenu(fileName = "NewTextureSettings", menuName = "CoreEngine/Tools/Texture Import Settings")]
    public class TextureImportSettingsSO : ScriptableObject
    {
        [Header("Base Settings")]
        [Tooltip("대부분 UI나 2D 맵 타일이므로 Sprite로 설정합니다.")]
        public TextureImporterType textureType = TextureImporterType.Sprite;
        public SpriteImportMode spriteMode = SpriteImportMode.Single;

        [Header("Fix Tearing (갈라짐 방지 설정)")]
        [Tooltip("타일 사이에 실선이 생기는 것을 막기 위해 Clamp를 권장합니다.")]
        public TextureWrapMode wrapMode = TextureWrapMode.Clamp;
        [Tooltip("이미지를 선명하게 유지하고 가장자리 번짐을 막기 위해 Point를 권장합니다.")]
        public FilterMode filterMode = FilterMode.Point;

        [Header("Optimization (최적화)")]
        [Tooltip("모바일이나 저사양이 아니라면 UI 깔끔함을 위해 Uncompressed를 종종 사용합니다.")]
        public TextureImporterCompression textureCompression = TextureImporterCompression.Uncompressed;
        public int maxTextureSize = 2048;
    }
}