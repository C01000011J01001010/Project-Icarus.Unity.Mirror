using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public class ScenedUiSelection : BaseUiSelection
{
    [Header("씬 UI 목록")]
    [AssetReferenceUILabelRestriction("ScenedUi")] // ScenedUi 라벨 전용 필터링
    [SerializeField] private List<AssetReferenceGameObject> _initialUis;

    public IEnumerable<AssetReferenceGameObject> GetValidUis() => base.GetValidUis(_initialUis);

#if UNITY_EDITOR
    // fieldName: 객체를 선언한 변수의 이름 (예: "_scenedSelection")
    public void Validate(MonoBehaviour owner, string fieldName)
    {
        ValidateList<IScenedUi>(owner, _initialUis, $"{fieldName}._initialUis");
    }
#endif
}

public class ScenedUiManager : MonoBehaviour, IScenedManager
{
    public int Priority => 10;

    public bool IsActive => throw new NotImplementedException();

    [Tooltip("화면에 띄우지 않고 로드만 해둘 UI들")]
    [SerializeField] private ScenedUiSelection _preloadSelection;

    private Dictionary<Type, BaseUi> _scenedUiDict = new();

    public IEnumerator Initialize()
    {
        // 씬 하이라키에 미리 배치된 UI들 자동 등록 (DFS)
        yield return RegisterPreplacedUis(transform);
    }

    public IEnumerator LateInitialize() => null;

    private IEnumerator RegisterPreplacedUis(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.TryGetComponent(out BaseUi ui) && ui is IScenedUi)
            {
                yield return ui.Initialize();
                ui.ClaimClose();
                _scenedUiDict.TryAdd(ui.GetType(), ui);
            }
            yield return RegisterPreplacedUis(child);
        }
    }

    /// <summary>
    /// 씬 전용 UI 가져오기
    /// </summary>
    public T GetUi<T>() where T : BaseUi, IScenedUi
    {
        Type type = typeof(T);
        if (_scenedUiDict.TryGetValue(type, out BaseUi ui)) return ui as T;

        Debug.LogError($"[ScenedUiManager] {type.Name} not found in this scene!");
        return null;
    }

    public void Exit()
    {
        foreach (var ui in _scenedUiDict.Values) ui.Exit();
        _scenedUiDict.Clear();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _preloadSelection?.Validate(this, nameof(_preloadSelection));
    }

    public void SetActive(bool active)
    {
        throw new NotImplementedException();
    }

    public IEnumerator Initialize(IModuleHub hub)
    {
        throw new NotImplementedException();
    }
#endif
}