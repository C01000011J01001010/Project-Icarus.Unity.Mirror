using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using CoreEngine;



public class BasePoolManager<PoolType> : MonoBehaviour, IManager
    where PoolType : Enum
{
    #region PoolSetup


    [Serializable]
    public class PoolSetup
    {
        public PoolType poolType;
        public GameObject prefab;

        private const int maxCount = 256;
        [Range(1, maxCount)] public int defaultAmount;
        [Range(2, maxCount)] public int defaultCapacity;
        [Range(2, maxCount)] public int maxSize;

        public PoolSetup() { SetDefaultValues(); }
        public void SetDefaultValues()
        {
            defaultAmount = 8;
            defaultCapacity = 16;
            maxSize = 128;
        }


#if UNITY_EDITOR
        public void ValidateValues()
        {
            if (defaultAmount < 1 || defaultCapacity < 2 || maxSize < 2)
            {
                SetDefaultValues();
                return;
            }
            if (defaultAmount > defaultCapacity) defaultCapacity = defaultAmount;
            if (defaultCapacity > maxSize) maxSize = defaultCapacity;
        }
#endif
    }

    #endregion

    public int _priority = 0;
    public int Priority => _priority;

    public bool IsActive => throw new NotImplementedException();

    public List<PoolSetup> poolSetups = new();

    private Dictionary<PoolType, IObjectPool<GameObject>> _pools;
    private Dictionary<PoolType, GameObject> _prefabs;
    private Dictionary<PoolType, Transform> _poolParents;

    // 씬 종료 처리가 진행 중인지 체크하는 플래그
    private bool _isShuttingDown = false;

    public IEnumerator Initialize()
    {
        _isShuttingDown = false;
        InitializePools();
        yield return null;
    }

    public IEnumerator LateInitialize()
    {
        yield return PreWarming();
    }

    public void Exit()
    {
        // 씬 전환 시 플래그를 켜서 더 이상 풀 반환 로직이 실행되지 않도록 막음
        _isShuttingDown = true;

        if (_pools != null)
        {
            _pools.Clear();
            _prefabs.Clear();
            _poolParents.Clear();
        }
    }

    // (MonoBehaviour 기본 콜백) 매니저 자체가 파괴될 때도 플래그 작동
    private void OnDestroy()
    {
        _isShuttingDown = true;
    }

    private void InitializePools()
    {
        _pools = new Dictionary<PoolType, IObjectPool<GameObject>>();
        _prefabs = new Dictionary<PoolType, GameObject>();
        _poolParents = new Dictionary<PoolType, Transform>();

        foreach (var setup in poolSetups)
        {
            if (_pools.ContainsKey(setup.poolType)) continue;
            if (setup.prefab == null) continue;

            _prefabs.Add(setup.poolType, setup.prefab);

            GameObject parentObj = new GameObject($"[{setup.poolType}_Pool]");
            parentObj.transform.SetParent(this.transform);
            _poolParents.Add(setup.poolType, parentObj.transform);

            PoolType currentType = setup.poolType;

            IObjectPool<GameObject> pool = new ObjectPool<GameObject>(
                createFunc: () => CreateItem(currentType),
                actionOnGet: OnTakeFromPool,
                actionOnRelease: (obj) => OnReturnedToPool(obj, currentType),
                actionOnDestroy: OnDestroyPoolObject,
#if UNITY_EDITOR
                collectionCheck: true,
#else
                collectionCheck: false,
#endif
                defaultCapacity: setup.defaultCapacity,
                maxSize: setup.maxSize
            );

            _pools.Add(setup.poolType, pool);
        }
    }

    private IEnumerator PreWarming()
    {
        int lastTime = Environment.TickCount;

        foreach (var setup in poolSetups)
        {
            if (!_pools.TryGetValue(setup.poolType, out IObjectPool<GameObject> pool)) continue;

            List<GameObject> prewarmList = new List<GameObject>(setup.defaultAmount);

            for (int i = 0; i < setup.defaultAmount; i++)
            {
                prewarmList.Add(pool.Get());

                if (Environment.TickCount - lastTime > 100)
                {
                    yield return null;
                    lastTime = Environment.TickCount; // 갱신 위치 수정 (프레임 단위 대기 후 시간 리셋)
                }
            }

            foreach (var obj in prewarmList)
            {
                pool.Release(obj);
            }
            yield return null;
        }
    }

    private GameObject CreateItem(PoolType type)
    {
        GameObject obj = Instantiate(_prefabs[type], _poolParents[type]);

        if (obj.TryGetComponent(out IPooledObject pooledItem))
        {
            pooledItem.SetPool(_pools[type]);
            return obj;
        }

        Destroy(obj);
        return null;
    }

    private void OnTakeFromPool(GameObject obj) => obj.SetActive(true);

    private void OnReturnedToPool(GameObject obj, PoolType type)
    {
        // 씬이 종료 중이거나 객체가 파괴 중일 때는 아무 처리도 하지 않음 (유니티가 알아서 날림)
        if (_isShuttingDown || obj == null) return;

        obj.SetActive(false);
        obj.transform.SetParent(_poolParents[type]);
    }

    private void OnDestroyPoolObject(GameObject obj) => Destroy(obj);

    public GameObject Spawn2D(PoolType type, Vector2 position2D)
    {
        return Spawn(type, new Vector3(position2D.x, position2D.y, 0));
    }

    public GameObject Spawn(PoolType type)
    {
        if (!_pools.ContainsKey(type)) return null;
        return _pools[type].Get();
    }

    public GameObject Spawn(PoolType type, Vector3 position)
    {
        GameObject obj = Spawn(type);
        if (obj != null) obj.transform.position = position;
        return obj;
    }

#if UNITY_EDITOR
    HashSet<PoolType> ___typeCheckSet = new HashSet<PoolType>();

    private void OnValidate()
    {
        foreach (PoolSetup poolSetup in poolSetups)
        {
            poolSetup.ValidateValues();

            if (poolSetup.prefab == null)
            {
                Debug.LogWarning($"[MultiObjectPoolManager] {poolSetup.poolType}의 프리팹이 비어있음");
            }

            if (!___typeCheckSet.Add(poolSetup.poolType))
            {
                Debug.LogError($"[MultiObjectPoolManager] 인스펙터에 {poolSetup.poolType} 풀이 중복해서 등록되어 있음");
            }
        }
        ___typeCheckSet.Clear();
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