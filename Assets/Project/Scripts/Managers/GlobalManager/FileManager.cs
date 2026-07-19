using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class FileManager : BaseGlobalManager, IGlobalManager
{
    public static GraphicOptionValues savedGraphicOption { get; protected set; }

    // 1. 공용 UI 저장소 (게임 종료 시까지 유지 - UIManager용)
    private Dictionary<string, GameObject> globalUIPrefabs = new();
    private List<AsyncOperationHandle<GameObject>> globalUIHandles = new();

    // 2. 씬 전용 UI 저장소 (씬 전환 시 비워짐 - UIController용)
    private Dictionary<string, GameObject> sceneUIPrefabs = new();
    private List<AsyncOperationHandle<GameObject>> sceneUIHandles = new();

    private PathManager pathManager;

    public void Exit()
    {
        // 모든 핸들 해제 (공용 + 씬 전용)
        ReleaseAssets(globalUIHandles, globalUIPrefabs);
        ReleaseAssets(sceneUIHandles, sceneUIPrefabs);

        if (gameObject != null) Destroy(gameObject);
    }

    public IEnumerator Initialize()
    {
        // GameManager temporarily disabled
        // pathManager = GameManager.GetManager<PathManager>();

#if UNITY_EDITOR
        TestLocalOption();
#endif
        LoadLocalOption();
        CheckStruct<GraphicOptionValues>(savedGraphicOption);

        yield break;
    }

    // ----------------------------------------------------------------------------------
    // [UI 로드 로직] 공용 및 씬 전용 구분 로드
    // ----------------------------------------------------------------------------------

    // UIManager가 초기화 시 호출: 공용 UI들을 미리 로드
    public IEnumerator LoadGlobalUIs(List<string> addresses)
    {
        yield return LoadAndCacheAssets(addresses, globalUIPrefabs, globalUIHandles);
    }

    // UIController가 씬 시작 시 호출: 씬 종속 UI들을 로드
    public IEnumerator LoadSceneUIs(List<string> addresses)
    {
        yield return LoadAndCacheAssets(addresses, sceneUIPrefabs, sceneUIHandles);
    }

    // 실제 비동기 로드 처리 핵심 로직
    private IEnumerator LoadAndCacheAssets(List<string> addresses,
        Dictionary<string, GameObject> targetDict, List<AsyncOperationHandle<GameObject>> targetHandles)
    {
        foreach (string addr in addresses)
        {
            if (targetDict.ContainsKey(addr)) continue;

            var handle = Addressables.LoadAssetAsync<GameObject>(addr);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                targetDict.TryAdd(addr, handle.Result);
                targetHandles.Add(handle);
            }
            else
            {
                Debug.LogError($"[FileManager] 에셋 로드 실패: {addr}");
            }
        }
    }

    // ----------------------------------------------------------------------------------
    // [에셋 사용] 캐싱된 프리팹 즉시 반환
    // ----------------------------------------------------------------------------------

    public GameObject GetGlobalUiPrefab(string address) => GetCachedPrefab(address, globalUIPrefabs);
    public GameObject GetSceneUI(string address) => GetCachedPrefab(address, sceneUIPrefabs);

    private GameObject GetCachedPrefab(string address, Dictionary<string, GameObject> dict)
    {
        if (dict.TryGetValue(address, out GameObject prefab)) return prefab;
        Debug.LogWarning($"[FileManager] 캐싱되지 않은 에셋 요청: {address}");
        return null;
    }

    // ----------------------------------------------------------------------------------
    // [메모리 해제] 씬 전환 시 UIController가 호출
    // ----------------------------------------------------------------------------------

    public void ReleaseSceneAssets()
    {
        ReleaseAssets(sceneUIHandles, sceneUIPrefabs);
        Debug.Log("[FileManager] 씬 전용 에셋 메모리 해제 완료");
    }

    private void ReleaseAssets(List<AsyncOperationHandle<GameObject>> handles, Dictionary<string, GameObject> dict)
    {
        foreach (var handle in handles)
        {
            if (handle.IsValid()) Addressables.Release(handle);
        }
        handles.Clear();
        dict.Clear();
    }

    // ----------------------------------------------------------------------------------
    // [데이터 로드] ScriptableObject 등 정적 데이터 로드
    // ----------------------------------------------------------------------------------

    public IEnumerator LoadAllGameDataAsync<DataType>(string label, Action<Dictionary<int, DataType>> onComplete)
        where DataType : BaseData
    {
        var dataHandle = Addressables.LoadAssetsAsync<DataType>(label, null);
        yield return dataHandle;

        if (dataHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Dictionary<int, DataType> dataDict = new Dictionary<int, DataType>();
            foreach (var data in dataHandle.Result)
            {
                if (!dataDict.TryAdd(data.Index, data))
                {
                    Debug.LogError($"유일하지 않은 Index 발견: {data.name}");
                }
            }
            onComplete?.Invoke(dataDict);
        }
        // 데이터는 보통 참조만 하므로 핸들을 바로 놔줘도 되지만, 
        // 확실한 메모리 관리를 위해선 사용하는 곳에서 Release 시점을 정해야 함
    }

    // ----------------------------------------------------------------------------------
    // 일반 IO 저장 시스템 (유지)
    // ----------------------------------------------------------------------------------
    public static void SaveFile(string directory, string fileName, params byte[] data)
    {
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        string totalDirectory = Path.Combine(directory, fileName);
        if (!File.Exists(totalDirectory)) File.Create(totalDirectory).Close();
        File.WriteAllBytes(totalDirectory, data);
    }

    public static byte[] LoadFile_FromSaveFolder(string directory, string fileName)
    {
        string totalDirectory = Path.Combine(directory, fileName);
        return File.Exists(totalDirectory) ? File.ReadAllBytes(totalDirectory) : null;
    }

#if UNITY_EDITOR
    private void TestLocalOption()
    {
        GraphicOptionValues options = GraphicOptionValues.testOption;

        string directory = pathManager.directory.Option;
        string fileName = pathManager.fileName.GraphicSettings;
        SaveFile(directory, fileName, options.Struct2ByteArray());
    }
#endif

    private void LoadLocalOption()
    {
        try
        {
            string directory = pathManager.directory.Option;
            string fileName = pathManager.fileName.GraphicSettings;
            byte[] savedData = LoadFile_FromSaveFolder(directory, fileName);
            if (savedData != null) savedGraphicOption = savedData.ByteArray2Struct<GraphicOptionValues>();
        }
        catch (Exception ex) { savedGraphicOption = GraphicOptionValues.defaultOption; Debug.LogWarning(ex); }
    }

    private void CheckStruct<T_Struct>(object obj)
    {
        if (typeof(T_Struct).IsValueType)
        {
            foreach (FieldInfo field in typeof(T_Struct).GetFields())
                Debug.Log($"{field.Name} : {field.GetValue(obj)}");
        }
    }
}