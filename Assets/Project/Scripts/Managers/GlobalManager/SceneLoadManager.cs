using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 전환 담당
/// </summary>
public sealed class SceneLoadManager : BaseGlobalManager, IGlobalManager
{
    // 현재 씬 -> UnloadScene에서 사용
    private static Scene currentScene;

    // 씬로딩 진행상황을 모니터링
    private static AsyncOperation sceneChangeProgress;

    // 씬 로드 후 해당 씬의 오브젝트를 관리하는 매니저
    // 씬 넘어갈때마다 파괴될거고 여기서는 한번만 접근할거니 굳이 캐싱하지 않음
    // WorldManager is temporarily disabled; references commented out
    // private WorldManager worldManager => WorldManager.Inst; 

    public void Exit()
    {
        SceneManager.sceneLoaded -= CALLBACK_SceneLoaded;
        SceneManager.sceneUnloaded -= CALLBACK_SceneUnloaded;
        SceneManager.activeSceneChanged -= CALLBACL_ActiveSceneChanged;
    }

    public IEnumerator Initialize()
    {
        // GamaManager의 Initialize가 호출되어 구독되므로 첫씬에서 실행되지 않음
        SceneManager.sceneLoaded -= CALLBACK_SceneLoaded;
        SceneManager.sceneLoaded += CALLBACK_SceneLoaded;
        SceneManager.sceneUnloaded -= CALLBACK_SceneUnloaded;
        SceneManager.sceneUnloaded += CALLBACK_SceneUnloaded;
        SceneManager.activeSceneChanged -= CALLBACL_ActiveSceneChanged;
        SceneManager.activeSceneChanged += CALLBACL_ActiveSceneChanged;

#if UNITY_EDITOR
        // SceneTester에서 적용시 사용
        Scene globalScene = SceneManager.GetSceneByName(Constants.SCENE_GlobalScene);
        // GlobalScene제외
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene cur = SceneManager.GetSceneAt(i);
            if (globalScene == cur) continue;
            if (cur.IsValid() && cur.isLoaded)
            {
                currentScene = cur;
                Debug.Log($"currentScene name is {currentScene.name}");
                break;
            }
        }
#endif
        yield return null;
    }


    // async를 반환한다고 해서 비동기인 것이 아니라 await 키워드 시작 시점부터 해당 라인의 코드가 완료될 때까지 비동기
    public async Task ChangeScene(string sceneName)
    {
        // currentScene.IsValid()는 초기화되지 않은 Scene객체에 대해서 false를 반환함
        // currentScene은 Start에 의해 실행된 Initialize에서 구독된 핸들러 SceneLoaded에서 초기화되기 때문에
        // GameManager를 포함하는 씬은 currentScene.IsValid()가 false를 반환함
        if (currentScene.IsValid() && currentScene.isLoaded)
        {
            // 메모리 정리 (WorldManager disabled)
            // worldManager.Exit();
            await SceneManager.UnloadSceneAsync(currentScene);
        }

        // 비동기로 씬로드해야 안끊김
        StartCoroutine(LoadingScene(sceneName));
    }


    public IEnumerator LoadingScene(string sceneName)
    {
        // 씬에서는 퍼센트로 초기화 상태를 보여주며, 정수 개수를 전달하지 않음
        GlobalUiManager.ClaimLoading_Start(-1);

        sceneChangeProgress = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        while (!sceneChangeProgress.isDone)
        {
            GlobalUiManager.ClaimLoading_Next($"SceneLoad {sceneName}...", sceneChangeProgress.progress * 0.3f);
            yield return null;
        }
        // 이 시점에서 씬의 모든 객체는 Awake와 OnEnable이 호출되어 처리됨

        // 새로운 씬을 로드 후 현재씬의 객체들 초기화 (WorldManager disabled)
        // yield return worldManager.Initialize();

        GlobalUiManager.ClaimLoading_End();
    }


    private void CALLBACK_SceneLoaded(Scene loadedScene, LoadSceneMode loadedMode)
    {
        // 최초 게임매니저를 가진 씬은 무조건 0번
        // currentScene에 들어가는 씬은 함수 ChangeScene에서 제거 대상
        if (loadedScene.buildIndex != 0)
        {
            SceneManager.SetActiveScene(loadedScene);
            currentScene = loadedScene;
        }
    }

    private void CALLBACK_SceneUnloaded(Scene unloadedScene)
    {

    }

    private void CALLBACL_ActiveSceneChanged(Scene prevScene, Scene newScene)
    { 

    }
}