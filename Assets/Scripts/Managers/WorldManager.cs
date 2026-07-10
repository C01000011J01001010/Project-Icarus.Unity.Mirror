using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//using ScenedHub = CoreManager<WorldManager, IScenedManager, IScenedGameObject>;

//public sealed class WorldManager : ScenedHub
//{
//    //private static WorldManager _instance;
//    //public  static WorldManager Inst => _instance;

//    //private Dictionary<Type, IScenedManager> managerDict = new();
//    //private Dictionary<Type, List<IScenedGameObject> /*중복된 객체 허용*/> objectsDict = new();

//    // Manager에 할당된 초기화 퍼센트는 70%
//    // 개별 WorldManager에서 초기화할 객체의 개수를 설정해줘야함
//    private float _processPercent;
//    private float _currentPercent;

//    public static bool IsInit { get; private set; } = false;

//    public void Exit()
//    {
//        IsInit = false;

//        // 초기화 할 때와 반대 순서로 해제

//        IScenedManager[] targetManagers = managerDict.Values.ToArray();
//        Array.Sort(targetManagers, (x, y) => y.Priority - x.Priority);
//        foreach (var manager in targetManagers)
//        {
//            manager?.Exit();
//        }

//        List<IScenedGameObject> targetObjects = new();
//        foreach (var objList in objectsDict.Values) targetObjects.AddRange(objList);
//        targetObjects.Sort((x, y) => y.Priority - x.Priority);
//        foreach (var obj in targetObjects)
//        {
//            obj?.Exit();
//        }

//        // 싱글톤 반납
//        _instance = null;

//        // 씬 정리하며 사용하지 않는 메모리를 정리
//        GC.Collect();
//    }

//    private void Awake()
//    {
//        if (!this.TryMakeSingleton(ref _instance))
//        {
//            Destroy(gameObject);
//        }
//    }

//    public IEnumerator Initialize()
//    {
//        // 월드매니저 초기화 중 Update와 입력무시
//        IsInit = false;

//        // 씬 전체에서 WorldManger가 직접 초기화 해야하는 대상을 찾기
//        var allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

//        var managerList = AddToDict<IScenedManager>(allMonoBehaviours, AddManager);
//        var ObjectList = AddToDict<IScenedGameObject>(allMonoBehaviours, AddObject);

//        int InitilzieSize = (managerList.Count + ObjectList.Count) * 2; // 2단계 초기화
//        SetInitializeCount(InitilzieSize);
//        yield return Initialize(managerList);
//        yield return Initialize(ObjectList);
//        yield return LateInitialize(managerList);
//        yield return LateInitialize(ObjectList);

//        yield return null;

//        // 월드매니저 초기화 종료 후 CALLBACK_Update 와 입력 활성화
//        IsInit = true;
//    }

//    private List<T> AddToDict<T>(MonoBehaviour[] allMonoBehaviours, Action<T> Add) 
//        where T : IInitialize, ILateInitialize, IPriority
//    {
//        // 같은 오브젝트에 붙은것 체크 하고
//        // 전체 오브젝트
//        List<T> targetList = new();
//        foreach (var mono in allMonoBehaviours)
//        {
//            // 형식 인수 T를 구현한 것만 리스트에 넣기
//            if (mono is T target)
//            {
//                targetList.Add(target);
//                Add.Invoke(target);
//            }
//        }
//        return targetList;
//    }



//    private void AddManager(IScenedManager manager)
//    {
//        Type managerType = manager.GetType();

//        // 중복된 매니저가 부착되는 것 방지
//        if (managerDict.ContainsKey(managerType) && managerDict[managerType] is not null)
//        {
//            // 중복된 컴포넌트 제거
//            // 미리 부착한 컴포넌트중 한개만 남도록 함
//            Debug.LogWarning($"{managerType.Name}은 이미 존재함");
//            Destroy(manager as MonoBehaviour);
//        }
//        else
//        {
//            managerDict.Add(managerType, manager);
//        }
//    }

//    private void AddObject(IScenedGameObject obj)
//    {
//        Type objType = obj.GetType();
//        if (!objectsDict.ContainsKey(objType))
//        {
//            objectsDict.Add(objType, new List<IScenedGameObject>());
//        }
//        objectsDict[objType].Add(obj);
//    }

//    private IEnumerator Initialize<T>(List<T> InitList) where T : IInitialize, ILateInitialize, IPriority
//    {
//        // 우선순위로 정렬
//        InitList.Sort((x, y) => x.Priority - y.Priority);

//        // 초기화 진행
//        foreach (var obj in InitList)
//        {
//            VisualizeNextLoading();
//            yield return obj.Initialize();
//            yield return null;
//        }
//    }

//    private IEnumerator LateInitialize<T>(List<T> InitList) where T : IInitialize, ILateInitialize, IPriority
//    {
//        // 2단계 초기화 진행
//        foreach (var obj in InitList)
//        {
//            VisualizeNextLoading();
//            yield return obj.LateInitialize();
//            yield return null;
//        }
//    }

//    private void SetInitializeCount(int count)
//    {
//        _currentPercent = 0.3f; // 씬로딩에서 30퍼센트 사용
//        _processPercent = (float)70 / count * 0.01f; // 초기화당 진행도를 0~1로 정규화
//    }

//    private void VisualizeNextLoading()
//    {
//        _currentPercent += _processPercent;
//        GlobalUiManager.ClaimLoading_Next("현재 씬 초기화 중...", _currentPercent);
//    }
//}