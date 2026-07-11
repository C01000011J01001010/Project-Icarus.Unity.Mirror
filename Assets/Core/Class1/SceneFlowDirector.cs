using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Core.EventBus;

// 1. 이벤트 구독 관리를 자동화하기 위해 BaseEventListener_Automatic을 상속받습니다.
public sealed class SceneFlowDirector : BaseEventListener_Automatic
{
    private static Scene currentScene;
    private static AsyncOperation sceneChangeProgress;

    private void OnEnable()
    {
        // ProjectContext나 UI 버튼 등에서 씬 로드를 요청하면 여기서 듣고 실행합니다.
        SubscribeTo<SceneLoadRequestEvent>(OnSceneLoadRequest);

        // 기존 SceneManager 콜백 등록 (필요 시 유지)
        SceneManager.sceneLoaded += CALLBACK_SceneLoaded;
        SceneManager.sceneUnloaded += CALLBACK_SceneUnloaded;
        SceneManager.activeSceneChanged += CALLBACK_ActiveSceneChanged;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        // 부모 클래스에서 EventBus 해제는 알아서 해주므로, SceneManager 이벤트만 수동 해제합니다.
        SceneManager.sceneLoaded -= CALLBACK_SceneLoaded;
        SceneManager.sceneUnloaded -= CALLBACK_SceneUnloaded;
        SceneManager.activeSceneChanged -= CALLBACK_ActiveSceneChanged;
    }

    // EventBus를 통해 씬 전환 요청이 들어왔을 때 실행되는 핸들러
    private void OnSceneLoadRequest(SceneLoadRequestEvent evt)
    {
        _ = ChangeScene(evt.TargetSceneName);
    }

    /// <summary>
    /// 에디터 테스트 및 런타임 초기화 시, SceneContext가 자신의 씬 정보를 다이렉트로 등록하기 위한 함수
    /// </summary>
    public static void RegisterCurrentScene(Scene scene)
    {
        currentScene = scene;
        SceneManager.SetActiveScene(scene);
        Debug.Log($"[SceneFlowDirector] 현재 콘텐츠 씬 등록 완료 (by SceneContext): {currentScene.name}");
    }

    public async Task ChangeScene(string sceneName)
    {
        // 3. 수정된 SystemLoadingEvent 구조체(State, Message, Progress) 규격에 맞게 발송
        EventBus<SystemLoadingEvent>.Publish(new SystemLoadingEvent(SystemLoadingEvent.State.Start, "이전 데이터 정리 중...", 0.0f));

        if (currentScene.IsValid() && currentScene.isLoaded)
        {
            await SceneManager.UnloadSceneAsync(currentScene);
        }

        StartCoroutine(LoadingScene(sceneName));
    }

    private IEnumerator LoadingScene(string sceneName)
    {
        sceneChangeProgress = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        sceneChangeProgress.allowSceneActivation = false;

        while (sceneChangeProgress.progress < 0.9f)
        {
            float loadProgress = Mathf.Clamp01(sceneChangeProgress.progress / 0.9f);

            // Progress 중계
            EventBus<SystemLoadingEvent>.Publish(new SystemLoadingEvent(SystemLoadingEvent.State.Progress, $"{sceneName} 자원 불러오는 중...", loadProgress * 0.7f));
            yield return null;
        }

        sceneChangeProgress.allowSceneActivation = true;

        while (!sceneChangeProgress.isDone) yield return null;
        // 💡 [이 시점] 새 씬이 활성화되면서 유저님이 만든 SceneContext.Awake()가 실행됨!
        // -> SceneContext의 Awake에서 SceneFlowDirector.RegisterCurrentScene()이 호출되어 currentScene 세팅 완료.

        // 3. 새 씬 내부의 시스템 구축 단계 (전체 진행도 70% ~ 95%)
        EventBus<SystemLoadingEvent>.Publish(new SystemLoadingEvent(SystemLoadingEvent.State.Progress, "새로운 시스템 환경 세팅 중...", 0.85f));

        // 💡 디렉터가 씬 컨텍스트를 직접 깨움!
        // (만약 이전에 논의했던 CoreFacade를 사용하셨다면 CoreFacade.InitializeSceneCore() 로 교체하시면 더 완벽합니다)
        if (SceneContext.Inst != null)
        {
            yield return SceneContext.Inst.Initialize();
        }

        // 4. 로딩 완료 명령 (Complete 발송)
        EventBus<SystemLoadingEvent>.Publish(new SystemLoadingEvent(SystemLoadingEvent.State.Complete, "로딩 완료!", 1.0f));
    }

    private void CALLBACK_SceneLoaded(Scene loadedScene, LoadSceneMode loadedMode) { }
    private void CALLBACK_SceneUnloaded(Scene unloadedScene) { }
    private void CALLBACK_ActiveSceneChanged(Scene prevScene, Scene newScene) { }
}