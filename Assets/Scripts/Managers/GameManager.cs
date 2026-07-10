using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


//using GlobalHub = CoreManager<GameManager, IGlobalManager, IGlobalGameObject>;

//public sealed class GameManager : GlobalHub
//{
//    //private List<IGlobalManager> managerList = new(); // 초기화 순서에 사용
//    public bool IsInit { get; private set; }
//    //private Dictionary<Type, IGlobalManager> managerDict = new(); // 접근에 사용

//    // Update 이벤트는 UpdateManager로 이동함

//    private bool pauseUpdate;

//    IEnumerator Initializer;

//    //private static GameManager _instance;
//    //public static GameManager Inst => _instance;

//    private void OnDisable()
//    {
//        Exit();
//    }

//    public void Exit()
//    {
//        // 강제종료시 초기화 코루틴이 있는데 아직 초기화가 완료되지 않은 경우
//        if (Initializer is not null && IsInit is false)
//        {
//            // 중간에 정지를 시킬 수 있도록 변수로 빼둔 것!
//            StopCoroutine(Initializer);
//        }

//        for (int i = PreSetManagerList.Count - 1; i >= 0; i--)
//        {
//            IGlobalManager manager = PreSetManagerList[i];
//            if(manager != null && manager.IsInit)
//            {
//                PreSetManagerList[i].Exit();
//            }
//        }
//        _instance = null;
//    }

//    private void Awake()
//    {
//        if (!this.TryMakeSingleton(ref _instance))
//        {
//            Destroy(this);
//        }
//        RegisterPreset();
//        // UpdateManager 인스턴스가 씬에 존재하도록 보장
//        if (GetComponent<UpdateManager>() == null)
//        {
//            gameObject.AddComponent<UpdateManager>();
//        }
//    }

//    private IEnumerator Start()
//    {
//        yield return null; // 로딩 ui에 초기화 우선권 부여
//        yield return Initializer = Initialize();
//    }

//    public IEnumerator Initialize()
//    {
//        // UiManager를 먼저 추가하여 로딩 화면을 보여줌
//        //TryGetOrAddManager<UIManager>();
//        //IGlobalManager uiManager = GetManager<UIManager>();
//        //yield return uiManager?.Initialize();
//        //uiManager.EndInit();

//        // LoadingScreen의 Start에서 초기화 끝냈으니 바로 사용
//        GlobalUiManager.ClaimLoading_Start(PreSetManagerList.Count);
//        foreach (var manager in PreSetManagerList)
//        {
//            string loadingMessage = GetManagerLoadingMessage(manager);
//            GlobalUiManager.ClaimLoading_Next(loadingMessage);
//            yield return manager.Initialize();
//            manager.EndInit();
//            yield return null;

//            // TODO: WorldManager처럼 Global 객체의 초기화도 추가하자
//            // TODO: 
//        }
//        GlobalUiManager.ClaimLoading_End();

//        //yield return ProcessManagerLoading();
        

//        SceneLoadManager loadManager = GetManager<SceneLoadManager>();

//#if UNITY_EDITOR
//        // 테스트 씬인 경우에 사용
//        if (SceneManager.sceneCount == 1)
//#endif
//#pragma warning disable CS4014 // 이 호출을 대기하지 않으므로 호출이 완료되기 전에 현재 메서드가 계속 실행됩니다.
//            loadManager.ChangeScene(Constants.SCENE_NAME_TitleScene);
//#pragma warning restore CS4014 // 이 호출을 대기하지 않으므로 호출이 완료되기 전에 현재 메서드가 계속 실행됩니다.

//#if UNITY_EDITOR
//        else
//        {
//            GlobalUiManager.ClaimLoading_Start(-1);
//            GlobalUiManager.ClaimLoading_Next("", 30.0f);
//            SceneManager.SetActiveScene(SceneTester.TestScene);
//            yield return WorldManager.Inst.Initialize();
//            GlobalUiManager.ClaimLoading_End();
//        }
//#endif

//        IsInit = true;
//    }


//    public void ClearEventUpdate()
//    {
//        UpdateManager.ClearEventUpdate();
//    }


//    // Pause 플래그 조회용 (UpdateManager에서 사용)
//    public static bool IsUpdatePaused() => Inst != null && Inst.pauseUpdate;

//    protected void RegisterPreset()// protected override void RegisterPreset()
//    {
//        TryGetOrAddManager<PathManager>();
//        TryGetOrAddManager<FileManager>();
//        TryGetOrAddManager<GlobalUiManager>();
//        TryGetOrAddManager<OptionManager>();
//        TryGetOrAddManager<AudioManager>();
//        TryGetOrAddManager<UserInputManager>();
//        TryGetOrAddManager<DragManager>();
//        TryGetOrAddManager<ItemStaticManager>();
//        //TryGetOrAddManager<QuestStaticManager>();
//        //TryGetOrAddManager<CropStaticManager>();
//        TryGetOrAddManager<SceneLoadManager>();
//        TryGetOrAddManager<TimeManager>();
//    }

//    // 타입패턴
//    public string GetManagerLoadingMessage(IGlobalManager manager) => manager switch
//    {
//        PathManager     => "파일 경로를 초기화 중...",
//        FileManager        => "파일을 불러오는 중...",
//        OptionManager      => "옵션 초기화 중...",
//        AudioManager       => "오디오 초기화 중...",
//        UserInputManager   => "유저 입력디바이스를 조정중...",
//        SceneLoadManager   => "씬을 불러오는 중...",
//        _=> "기타 로딩중..."
//    };

//    // ReSharper disable Unity.PerformanceAnalysis
//    public static T GetManager<T>() where T : class, IGlobalManager
//    {
//        Type managerType = typeof(T);
//        if(Inst.managerDict.TryGetValue(managerType, out IGlobalManager manager))
//        {
//            return (T)manager;
//        }

//        Debug.LogError($"Object({managerType.Name}) is not in managerDict");
//        return null;
//    }

//    public static bool SetPauseUpdate(bool paused) => Inst.pauseUpdate = paused;

    

//}
