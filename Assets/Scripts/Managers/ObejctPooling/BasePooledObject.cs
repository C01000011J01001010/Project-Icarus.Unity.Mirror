using UnityEngine;
using UnityEngine.Pool;

public class BasePooledObject : MonoBehaviour, IPooledObject
{
    private IObjectPool<GameObject> _managedPool;

    // 풀 매니저가 이 오브젝트를 생성할 때 풀의 참조를 주입합니다.
    public void SetPool(IObjectPool<GameObject> pool)
    {
        _managedPool = pool;
    }

    public virtual void OnSetPool() { }

    // 오브젝트 사용이 끝났을 때 호출합니다. (예: 파티클 종료, 일정 시간 경과 후)
    public void ReturnToPool()
    {
        _managedPool?.Release(gameObject);
    }

    public virtual void OnReturnToPool() { }
}