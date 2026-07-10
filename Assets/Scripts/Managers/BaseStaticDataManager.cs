using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static UnityEditor.LightingExplorerTableColumn;

/// <summary>
/// 하나의 정적 데이터(Table)를 관리하는 매니저의 기반구조
/// </summary>
public abstract class BaseStaticDataManager<TDataObject> : BaseGlobalManager, IGlobalManager
    where TDataObject : BaseData
{
    protected const string AssetMenu = "Static Data Manager";
    protected abstract string Label { get; }
    // 아이템 ID를 키(Key)로 사용하여 빠르게 검색하기 위한 딕셔너리
    protected Dictionary<int, TDataObject> database = new Dictionary<int/*id*/, TDataObject>();
    public void Exit()
    {

    }

    public IEnumerator Initialize()
    {
        yield return InitializeDatabase();
        yield return null;
    }

    private IEnumerator InitializeDatabase()
    {
        // Resources/Items 경로 안에 ScriptableObject들을 모아두었다고 가정
        // GameManager가 임시로 비활성화되어 있어 주석 처리
        // FileManager fileManager = GameManager.GetManager<FileManager>();
        // yield return fileManager.LoadAllGameDataAsync<TDataObject>(Label, OnLoadedDataBase);
        yield break;

    }

    private void OnLoadedDataBase(Dictionary<int, TDataObject> LoadedDataBase)
    {
        if (LoadedDataBase.IsNullOrEmpty())
        {
            Debug.LogError($"[{this.GetType().Name}] 데이터 로드 실패");
            return;
        }
        Debug.Log($"[{this.GetType().Name}] 총 {LoadedDataBase.Count}개의 데이터 로드 성공");
        database = LoadedDataBase;
    }

    /// <summary>
    /// ID를 통해 DB의 특정 레코드(데이터)를 가져옴
    /// </summary>
    public TDataObject GetRecord(int id)
    {
        if (database.TryGetValue(id, out TDataObject record))
        {
            return record;
        }

        Debug.LogWarning($"[{this.GetType().Name}] 해당 ID의 레코드(데이터)를 찾을 수 없음: {id}");
        return null;
    }
}