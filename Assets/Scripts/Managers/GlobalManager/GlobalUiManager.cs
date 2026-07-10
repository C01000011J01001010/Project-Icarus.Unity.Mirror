using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

// 로딩 이벤트 델리게이트
public delegate void DelegateLoading_Call(int processAmount);
public delegate void DelegateLoading_Next(string loadingContext, int skipAmount);
public delegate void DelegateLoading_NextPercent(string loadingContext, float percent);
public delegate void DelegateLoading_End();

[Serializable]
public class GlobalUiSelection : BaseUiSelection
{
    [Header("전역 UI 목록")]
    [AssetReferenceUILabelRestriction("GlobalUi")] // GlobalUi 라벨 전용 필터링
    [SerializeField] private List<AssetReferenceGameObject> _initialUis;

    public IEnumerable<AssetReferenceGameObject> GetValidUis() => base.GetValidUis(_initialUis);

#if UNITY_EDITOR
    public void Validate(MonoBehaviour owner, string fieldName)
    {
        ValidateList<IGlobalUi>(owner, _initialUis, $"{fieldName}._initialUis");
    }
#endif
}

public class GlobalUiManager : BaseGlobalManager, IGlobalManager
{
    // --- 로딩 이벤트 ---
    public static event DelegateLoading_Call OnLoadingCall;
    public static event DelegateLoading_Next OnLoadingNext;
    public static event DelegateLoading_NextPercent OnLoadingNextPercent;
    public static event DelegateLoading_End OnLoadingEnd;

    private Transform _uiRoot;
    private Dictionary<string, Canvas> _canvases = new();

    // IGlobalUi 태그를 가진 BaseUi만 저장하는 딕셔너리
    private Dictionary<Type, BaseUi> _uiDict = new();

    // 캔버스 상수
    private const string HUD_CANVAS = "HUD Canvas";
    private const string POPUP_CANVAS = "PopUp Canvas";
    private const string LOADING_CANVAS = "Loading Canvas";

    [Tooltip("화면에 띄우지 않고 로드만 해둘 UI들")]
    [SerializeField] private GlobalUiSelection _preloadSelection;

    public IEnumerator Initialize()
    {
        GameObject root = new GameObject("Global_UI_Root");
        DontDestroyOnLoad(root);
        _uiRoot = root.transform;

        // 캔버스 프리셋(Addressable)으로 기본 캔버스들 생성
        yield return CreateCanvas(HUD_CANVAS, 0);
        yield return CreateCanvas(POPUP_CANVAS, 10);
        yield return CreateCanvas(LOADING_CANVAS, 100);
    }

    private IEnumerator CreateCanvas(string canvasName, int order)
    {
        var handle = Addressables.InstantiateAsync("CanvasPreset", _uiRoot);
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject go = handle.Result;
            go.name = canvasName;
            Canvas canvas = go.GetComponent<Canvas>();
            canvas.sortingOrder = order;
            _canvases.Add(canvasName, canvas);
        }
    }

    // ----------------------------------------------------
    // UI 관리 로직
    // ----------------------------------------------------

    /// <summary>
    /// 전역 UI 가져오기 (컴파일 단계에서 IGlobalUi인지 검사)
    /// </summary>
    public T GetUi<T>() where T : BaseUi, IGlobalUi
    {
        Type type = typeof(T);
        if (_uiDict.TryGetValue(type, out BaseUi ui)) return ui as T;

        Debug.LogWarning($"[GlobalUiManager] {type.Name} is not loaded. Use LoadUi first.");
        return null;
    }

    /// <summary>
    /// 전역 UI 비동기 로드 (클래스 이름을 Addressable Key로 사용)
    /// </summary>
    public IEnumerator LoadUi<T>(string targetCanvas = POPUP_CANVAS) where T : BaseUi, IGlobalUi
    {
        Type type = typeof(T);
        if (_uiDict.ContainsKey(type)) yield break;

        Transform parent = _canvases.ContainsKey(targetCanvas) ? _canvases[targetCanvas].transform : _uiRoot;

        var handle = Addressables.InstantiateAsync(type.Name, parent);
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            T ui = handle.Result.GetComponent<T>();
            yield return ui.Initialize();
            ui.ClaimClose();
            _uiDict.Add(type, ui);
        }
        else
        {
            Debug.LogError($"[GlobalUiManager] Failed to load Addressable Key: {type.Name}");
        }
    }

    public void RegisterPreplacedUi<T>(T preplacedUi) where T : BaseUi, IGlobalUi
    {
        if (preplacedUi == null) return;

        Type type = preplacedUi.GetType();

        // 1. 이미 등록되어 있는지 확인
        if (_uiDict.ContainsKey(type))
        {
            Debug.LogWarning($"[GlobalUiManager] {type.Name}는 이미 등록되어 있습니다.");
            return;
        }

        // 2. 딕셔너리에 추가
        _uiDict.Add(type, preplacedUi);

        // 3. 초기화 (이미 초기화가 되어있을 수도 있으므로 상황에 맞춰 조절)
        // 수동 등록 시에는 보통 게임 시작 시 혹은 Awake에서 직접 호출하게 됩니다.
        Debug.Log($"[GlobalUiManager] 외부 UI 사전 등록 완료: {type.Name}");
    }

    public void Exit()
    {
        foreach (var ui in _uiDict.Values) ui.Exit();
        _uiDict.Clear();
    }

    // ----------------------------------------------------
    // 로딩 제어 로직 (Static)
    // ----------------------------------------------------

    public static void ClaimLoading_Start(int processAmount)
    {
        OnLoadingCall?.Invoke(processAmount);
    }

    public static void ClaimLoading_Next(string loadingContext, int skipAmount = 1)
    {
        OnLoadingNext?.Invoke(loadingContext, skipAmount);
    }

    public static void ClaimLoading_Next(string loadingContext, float percent)
    {
        OnLoadingNextPercent?.Invoke(loadingContext, percent);
    }

    public static void ClaimLoading_End()
    {
        OnLoadingEnd?.Invoke();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _preloadSelection?.Validate(this, nameof(_preloadSelection));
    }
#endif
}