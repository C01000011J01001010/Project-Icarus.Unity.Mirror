#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CoreEngine.LevelDesign.Editor
{
    [CustomPropertyDrawer(typeof(LayerOutlineSetting))]
    public class LayerOutlineSettingDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            Rect nameRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            Rect colorRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);
            Rect thickRect = new Rect(position.x, position.y + (EditorGUIUtility.singleLineHeight + 2) * 2, position.width, EditorGUIUtility.singleLineHeight);
            Rect depthRect = new Rect(position.x, position.y + (EditorGUIUtility.singleLineHeight + 2) * 3, position.width, EditorGUIUtility.singleLineHeight);
            Rect overRect = new Rect(position.x, position.y + (EditorGUIUtility.singleLineHeight + 2) * 4, position.width, EditorGUIUtility.singleLineHeight);

            SerializedProperty layerNameProp = property.FindPropertyRelative("layerName");
            int layerIndex = LayerMask.NameToLayer(layerNameProp.stringValue);
            if (layerIndex < 0) layerIndex = 0;

            int newLayerIndex = EditorGUI.LayerField(nameRect, "Target Layer", layerIndex);
            layerNameProp.stringValue = LayerMask.LayerToName(newLayerIndex);

            EditorGUI.PropertyField(colorRect, property.FindPropertyRelative("outlineColor"), new GUIContent("Outline Color"));
            EditorGUI.PropertyField(thickRect, property.FindPropertyRelative("outlineThickness"), new GUIContent("Thickness (Px)"));
            EditorGUI.PropertyField(depthRect, property.FindPropertyRelative("depthThreshold"), new GUIContent("깊이 오차 허용(Threshold)"));

            // 🌟 강제 절단 마스크 UI
            EditorGUI.PropertyField(overRect, property.FindPropertyRelative("forceEdgeMask"), new GUIContent("강제 경계선 생성 (자르기)"));

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (EditorGUIUtility.singleLineHeight + 2) * 5;
        }
    }
}
#endif