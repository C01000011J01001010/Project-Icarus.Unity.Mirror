using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[Serializable]
public abstract class BaseUiSelection
{
    // (런타임) 안전한 UI 목록 반환 (자식 클래스에서 호출하여 사용)
    protected IEnumerable<AssetReferenceGameObject> GetValidUis(List<AssetReferenceGameObject> list)
    {
        if (list == null) yield break;

        HashSet<string> loadedGuids = new();

        foreach (var assetRef in list)
        {
            if (assetRef == null || !assetRef.RuntimeKeyIsValid()) continue;

            if (!loadedGuids.Add(assetRef.AssetGUID))
            {
                Debug.LogWarning($"[UiSelection] 중복된 UI 감지. 로드를 스킵합니다: {assetRef.RuntimeKey}");
                continue;
            }

            yield return assetRef;
        }
    }

    // (에디터) 인스펙터 중복제거 및 타입 검증 (부모에서 제네릭으로 통합)
#if UNITY_EDITOR
    protected void ValidateList<TInterface>(MonoBehaviour owner, List<AssetReferenceGameObject> list, string propertyPath)
    {
        if (list == null || list.Count == 0) return;

        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (owner == null) return;

            var so = new UnityEditor.SerializedObject(owner);
            so.Update();

            // 자식 클래스의 변수명(propertyPath)을 동적으로 찾아서 접근
            var listProp = so.FindProperty(propertyPath);
            if (listProp == null) return;

            bool isChanged = false;
            var guidSet = new HashSet<string>();

            for (int i = 0; i < list.Count; i++)
            {
                if (i >= listProp.arraySize) break;

                var ui = list[i];
                if (ui == null || string.IsNullOrEmpty(ui.AssetGUID)) continue;

                string currentGuid = ui.AssetGUID;
                bool isDuplicate = !guidSet.Add(currentGuid);
                bool isInvalidType = false;

                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(currentGuid);
                GameObject prefabGo = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

                // 제네릭 TInterface (IScenedUi 또는 IGlobalUi) 부착 여부 검사
                if (prefabGo != null && prefabGo.GetComponent<TInterface>() == null)
                {
                    isInvalidType = true;
                }

                if (isDuplicate || isInvalidType)
                {
                    if (isDuplicate)
                        Debug.LogWarning($"[UiSelection] 중복 차단: '{prefabGo?.name}' 슬롯을 비웁니다.");
                    if (isInvalidType)
                        Debug.LogWarning($"[UiSelection] 타입 오류: '{prefabGo?.name}'은(는) {typeof(TInterface).Name}가 아닙니다!");

                    ui.SetEditorAsset(null);
                    list[i] = new AssetReferenceGameObject("");

                    var elementProp = listProp.GetArrayElementAtIndex(i);
                    var guidProp = elementProp.FindPropertyRelative("m_AssetGUID");
                    if (guidProp != null) guidProp.stringValue = "";

                    isChanged = true;
                }
            }

            if (isChanged)
            {
                so.ApplyModifiedProperties();
                UnityEditor.EditorUtility.SetDirty(owner);
            }
        };
    }
#endif
}

public class UiController : MonoBehaviour, IScenedGameObject
{
    [SerializeField] private int _priority = 100; // 매니저들보다 늦게 초기화되도록 우선순위 설정
    public int Priority => _priority;

    [SerializeField] private ScenedUiSelection _scenedSelection;
    [SerializeField] private GlobalUiSelection _globalSelection;

    // 매니저 참조 캐싱
    private GlobalUiManager _globalUiManager;
    private ScenedUiManager _scenedUiManager;

    public IEnumerator Initialize()
    {
        // 1. 매니저 참조 가져오기
        // GameManager/WorldManager 임시 비활성화로 인해 주석 처리
        // _globalUiManager = GameManager.GetManager<GlobalUiManager>();
        // _scenedUiManager = WorldManager.GetManager<ScenedUiManager>();

        // 씬 UI 로드 및 열기
        foreach (var assetRef in _scenedSelection.GetValidUis())
        {
            if (assetRef.RuntimeKeyIsValid())
            {
                yield return LoadAndOpenInitialUi(assetRef);
            }
        }

        // 전역 UI 로드 및 열기
        foreach (var assetRef in _globalSelection.GetValidUis())
        {
            if (assetRef.RuntimeKeyIsValid())
            {
                yield return LoadAndOpenInitialUi(assetRef);
            }
        }

        Debug.Log("[UiController] 초기화 완료");
    }

    public IEnumerator LateInitialize()
    {
        yield break;
    }

    /// <summary>
    /// Addressables를 이용해 초기 UI를 인스턴스화하고 엽니다.
    /// </summary>
    private IEnumerator LoadAndOpenInitialUi(AssetReferenceGameObject assetRef)
    {
        // UiController의 자식으로 UI를 생성 (원한다면 다른 Canvas Transform을 넘겨도 됨)
        var handle = assetRef.InstantiateAsync(transform);
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            if (handle.Result.TryGetComponent(out BaseUi ui))
            {
                // 생성된 UI 초기화 및 열기
                yield return ui.Initialize();
                ui.ClaimOpen();

                // (선택) 만약 생성한 UI를 ScenedUiManager에서도 검색(GetUi)되게 하고 싶다면 등록
                // _scenedUiManager.RegisterDynamicUi(ui); 
            }
        }
        else
        {
            Debug.LogError($"[UiController] 초기 UI 로드 실패: {assetRef.RuntimeKey}");
        }
    }

    // ====================================================================
    // 비즈니스 로직에서 UI를 조작할 때 사용하는 단일 창구 (Facade) 메서드들
    // ====================================================================

    /// <summary>
    /// 전역(Global) UI를 화면에 엽니다.
    /// 사용 예: OpenGlobalUi<InventoryUi>();
    /// </summary>
    public T OpenGlobalUi<T>() where T : BaseUi, IGlobalUi
    {
        if (_globalUiManager == null)
        {
            Debug.LogWarning($"[UiController] GlobalUiManager is not available. (_globalUiManager == null)");
            return null;
        }
        T ui = _globalUiManager.GetUi<T>();
        if (ui != null)
        {
            ui.ClaimOpen();
        }
        else
        {
            Debug.LogWarning($"[UiController] 전역 UI ({typeof(T).Name}) 가 매니저에 없습니다.");
        }
        return ui;
    }

    /// <summary>
    /// 씬(Scened) 전용 UI를 화면에 엽니다.
    /// 사용 예: OpenScenedUi<SmithyUi>();
    /// </summary>
    public T OpenScenedUi<T>() where T : BaseUi, IScenedUi
    {
        if (_scenedUiManager == null)
        {
            Debug.LogWarning($"[UiController] ScenedUiManager is not available. (_scenedUiManager == null)");
            return null;
        }
        T ui = _scenedUiManager.GetUi<T>();
        if (ui != null)
        {
            ui.ClaimOpen();
        }
        else
        {
            Debug.LogWarning($"[UiController] 씬 UI ({typeof(T).Name}) 가 매니저에 없습니다.");
        }
        return ui;
    }

    /// <summary>
    /// 열려있는 UI를 닫습니다.
    /// </summary>
    public void CloseUi(BaseUi ui)
    {
        if (ui != null)
        {
            ui.ClaimClose();
        }
    }

    public void Exit()
    {
        // 실제 UI 파괴와 메모리 해제는 ScenedUiManager의 Exit()에서 일괄 처리되므로
        // Controller에서는 별도의 파괴 로직을 신경 쓰지 않아도 됩니다. (관심사 분리)
        Debug.Log("[UiController] Exit 처리됨");
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 본인 클래스에 선언된 변수 이름을 동적으로 넘겨줍니다.
        _scenedSelection?.Validate(this, nameof(_scenedSelection));
        _globalSelection?.Validate(this, nameof(_globalSelection));
    }
#endif
}