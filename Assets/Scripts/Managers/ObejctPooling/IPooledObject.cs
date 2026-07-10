using UnityEngine;
using UnityEngine.Pool;

public interface IPooledObject
{
    // 특정 클래스 타입이 아닌 범용 GameObject 풀을 매개변수로 받습니다.
    void SetPool(IObjectPool<GameObject> pool);
    void ReturnToPool();
}