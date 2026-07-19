#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// PropertyDrawer -> DecoratorDrawer로 변경합니다.
[CustomPropertyDrawer(typeof(SeparatorAttribute))]
public class SeparatorDrawer : DecoratorDrawer
{
    // DecoratorDrawer는 속성을 직접 그리지 않으므로, OnGUI의 매개변수가 Rect만 남습니다.
    public override void OnGUI(Rect position)
    {
        // 어트리뷰트 정보를 가져옵니다.
        SeparatorAttribute separator = (SeparatorAttribute)attribute;

        // 구분선을 그릴 Rect를 정의합니다.
        // position의 상단에 선을 그립니다.
        Rect lineRect = new Rect(position.x, position.y + separator.padding / 2, position.width, separator.thickness);

        // 선을 그립니다.
        EditorGUI.DrawRect(lineRect, separator.color);
    }

    // 이 데코레이터(구분선)가 차지할 높이를 반환합니다.
    // GetPropertyHeight -> GetHeight로 이름이 바뀝니다.
    public override float GetHeight()
    {
        SeparatorAttribute separator = (SeparatorAttribute)attribute;
        // 선 두께와 위아래 여백(padding)을 더한 값을 높이로 지정합니다.
        return separator.thickness + separator.padding;
    }
}
#endif