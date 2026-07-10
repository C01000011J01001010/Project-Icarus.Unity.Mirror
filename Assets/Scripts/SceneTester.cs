
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 이 스크립트를 테스트해볼 씬의 객체에 넣으면 씬 테스트 가능
/// </summary>
public class SceneTester : MonoBehaviour
{

    private void Awake()
    {
#if UNITY_EDITOR
        if (SceneManager.sceneCount == 1)
            SceneManager.sceneLoaded += OnSceneLoaded;
#else
        // 실제 빌드됐다면 이 객체는 필요하지 않음
        Destroy(gameObject);
#endif
    }


#if UNITY_EDITOR
    public static Scene TestScene {  get; private set; }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    

    void Start()
    {
        if(SceneManager.sceneCount == 1)
            SceneManager.LoadSceneAsync(Constants.SCENE_NAME_GlobalScene, LoadSceneMode.Additive);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (SceneManager.sceneCount == 1)
            TestScene = scene;
    }
#endif
}
