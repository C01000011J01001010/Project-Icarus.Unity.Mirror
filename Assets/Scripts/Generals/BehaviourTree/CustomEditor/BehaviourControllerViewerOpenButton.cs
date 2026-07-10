#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// BaseBehaviourController 에서 사용되는 키 Viewer 창 열기 버튼 생성을 위한 클래스
[CustomEditor(typeof(BaseBehaviourController), true)]
public sealed class BehaviourControllerViewerOpenButton : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Open Key Viewer Window"))
        {
            BehaviourControllerViewerWindow.ShowWindow();
        }
    }

}
#endif