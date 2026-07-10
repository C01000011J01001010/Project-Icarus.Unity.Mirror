#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// BehaviourController에 추가된 키 상태를 실시간으로 확인하기 위한 창(window) 클래스
public sealed class BehaviourControllerViewerWindow : EditorWindow
{
    // 현재 스크롤링된 위치를 기록하기 위한 변수
    private Vector2 currentScrollPosition;

    // 선택된 BaseBehaviourController 컴포넌트를 기록
    List<BaseBehaviourController> selectedBehaviourControllers = new();

    [MenuItem("Window/Behaviour Key Viewer")] // 메뉴탭에서 window -> Behaviour Key Viewer로 해당 함수 호출 가능
    public static void ShowWindow()
    {
        // 함수호출시 TCharacter 객체를 생성 이 때 T는 EditorWindow를 상속받은 유니티 Window
        GetWindow<BehaviourControllerViewerWindow>().minSize = new Vector2(300.0f, 200.0f);
    }

    /// <summary>
    /// 매 프레임 호출
    /// </summary>
    private void Update()
    {
        selectedBehaviourControllers.Clear();

        // Selection : 선택한 오브젝트의 정보를 가져옴
        foreach (Transform selectedTransform in Selection.GetTransforms // Selection.objects 중에서 Transform이 있는 씬 내 게임 오브젝트들의 Transform 배열을 반환
            (SelectionMode.TopLevel | SelectionMode.ExcludePrefab | SelectionMode.Editable))
        {
            BaseBehaviourController behaviourController = selectedTransform.GetComponent<BaseBehaviourController>();
            if(behaviourController)
            {
                selectedBehaviourControllers.Add(behaviourController);
            }
        }

        Repaint();
    }



    /// <summary>
    /// Repaint 호출시 호출
    /// </summary>
    private void OnGUI()
    {
        try
        {
            currentScrollPosition = GUILayout.BeginScrollView(currentScrollPosition, false, true);
            Draw();
        }
        catch(System.Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
        finally
        {
            GUILayout.EndScrollView();
        }
    }

    private void Draw()
    {
        foreach (BaseBehaviourController behaviourController in selectedBehaviourControllers)
        {
            // EditorGUILayout.BeginVertical();
            using (EditorGUILayout.VerticalScope veticalScope = new()) 
            {
                // 선택된 게임 오브젝트의 이름을 얻음
                string gameObjectName = behaviourController?.gameObject?.name ?? "?";

                if (GUI.Button(veticalScope.rect, GUIContent.none))
                {
                    Debug.Log(gameObjectName);
                    // 클릭한 오브젝트를 선택
                    Selection.objects = new[] { behaviourController.gameObject };
                    return;
                }

                // 키 목록을 얻음
                Dictionary<string, object> dict = behaviourController.PropertyDict;

                GUIStyle nameLabelStyle = new();
                nameLabelStyle.normal.textColor = Color.white;
                nameLabelStyle.fontStyle = FontStyle.Bold;
                nameLabelStyle.alignment = TextAnchor.MiddleLeft;
                nameLabelStyle.contentOffset = Vector2.right * 10.0f;
                GUILayout.Space(5.0f);

                // 선택된 오브젝트 이름 출력
                GUILayout.Label(gameObjectName, nameLabelStyle);

                // 키 목록을 출력
                foreach (KeyValuePair<string, object> pair in dict)
                {
                    // 수평 레이아웃 시작
                    //EditorGUILayout.BeginHorizontal();
                    using (EditorGUILayout.HorizontalScope horizontalScope = new())
                    {
                        // 키 이름 출력
                        GUIStyle keyLabelStyle = new();
                        keyLabelStyle.normal.textColor = Color.white;
                        keyLabelStyle.fontStyle = FontStyle.Bold;
                        keyLabelStyle.alignment = TextAnchor.MiddleLeft;
                        keyLabelStyle.contentOffset = Vector2.right * 20.0f;
                        GUILayout.Label(pair.Key, keyLabelStyle);

                        // 값 출력
                        GUIStyle valueLabelStyle = new();
                        valueLabelStyle.normal.textColor = Color.magenta;
                        valueLabelStyle.fontStyle = FontStyle.Bold;
                        valueLabelStyle.alignment = TextAnchor.MiddleLeft;

                        string valueString = pair.Value is null ? "null" : pair.Value.ToString();
                        GUILayout.Label(pair.Value.ToString(), valueLabelStyle);
                    }
                    //EditorGUILayout.EndHorizontal();
                }
            }
            //EditorGUILayout.EndVertical();
        }
    }


}
#endif
