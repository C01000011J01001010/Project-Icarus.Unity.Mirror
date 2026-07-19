using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class TitleSelection : BaseSelection, IInitialize, IScenedUi
{
    private BaseButton[] _buttonList;
    SceneLoadManager _sceneLoadManager;

    public override IEnumerator Initialize()
    {
        _buttonList = gameObject.GetComponentsInChildren<BaseButton>();
        // GameManager temporarily disabled
        // _sceneLoadManager = GameManager.GetManager<SceneLoadManager>();

        foreach (BaseButton button in _buttonList)
        {
            yield return button.Initialize();
        }

        SetButtonCallback();

        Debug.Log("TitleSelection 초기화 성공");
        yield return null;
    }

    protected override void SetButtonCallback()
    {
        _buttonList[0].SetCallback(CALLBACK_StatNewGame);
        _buttonList[1].SetCallback(CALLBACK_OpenGameSettingsWindow);
        _buttonList[2].SetCallback(CALLBACK_ExitGame);
    }

    protected override void ClearButtonCallback()
    {
        foreach (BaseButton button in _buttonList)
        {
            button.ClearCallback();
        }
    }

    public void CALLBACK_StatNewGame()
    {
#pragma warning disable CS4014 // 이 호출을 대기하지 않으므로 호출이 완료되기 전에 현재 메서드가 계속 실행됩니다.
        // WorldManager/GameManager disabled: scene change temporarily commented out
        // _sceneLoadManager.ChangeScene(Constants.SCENE_NAME_SampleScene);
#pragma warning restore CS4014 // 이 호출을 대기하지 않으므로 호출이 완료되기 전에 현재 메서드가 계속 실행됩니다.
    }

    public void CALLBACK_OpenGameSettingsWindow()
    {

    }

    public void CALLBACK_ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
