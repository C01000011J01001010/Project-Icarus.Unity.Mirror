using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 위치에 대한 메타정보
/// </summary>
public enum SocketType
{
    None,

    #region 캐릭터 소켓
    LeftHand,
    RightHand,
    #endregion

    #region 막대기 or 검
    ContactPoint, // 충돌 이펙트 발생하는 부분
    #endregion

    #region 총기
    Muzzle, // 총구
    Ejection_Port, // 탄피 배출구
    #endregion
}

public abstract class BaseSocket : MonoBehaviour
{
    public abstract SocketType Type { get; }

    // 대상을 소켓에 연결하는 과정
    public void AttachTransform(Transform target)
    {

        // 자식객체의 레이어를 소켓 레이어로 변경
        foreach (Transform targetChild in target.GetComponentsInChildren<Transform>())
        {
            targetChild.gameObject.layer = gameObject.layer;
        }


        target.SetParent(transform);
        target.localPosition = Vector3.zero;
        target.localRotation = Quaternion.identity;
        target.localScale = Vector3.one;
    }

    // 조건없이 바로 리스트 집어넣기
    public void GetSockets(List<BaseSocket> result)
    {
        result.Add(this);
    }

    // 해당 타입의 소켓만 리스트 집어넣기
    public void GetSockets(List<BaseSocket> result, SocketType wantType)
    {
        if(Type == wantType) 
            result.Add(this);
    }

    // 특정 조건을 만족하는 경우 리스트에 집어넣기
    public void GetSockets(List<BaseSocket> result, Func<BaseSocket, bool> predicate)
    {
        if(predicate(this)) 
            result.Add(this);
    }

    // 델리게이트의 반환값이 있을 때 구독된 함수가 여러개 면 어떤것이 반환되는가
    //      => 마지막으로 실행된 함수의 반환값을 받음
    // 만약 조건에 맞는 소켓을 반환하고자 한다면?
    // 가능한 경우 : 찾고자 하는 소켓이 맨 마지막이면 정상작동
    // 불가능한 경우 : 나머지 다
    public BaseSocket GetSocket() => this;
    public BaseSocket GetSocket(SocketType wantType) 
        => (Type == wantType) ? this : null;
    public BaseSocket GetSocket(Func<BaseSocket, bool> predicate) 
        => predicate(this) ? this : null;

    public void GetSocket(ref BaseSocket result) 
        => result ??= GetSocket();
    public void GetSocket(ref BaseSocket result, SocketType wantType)
        => result ??= GetSocket(wantType); // result가 null일때만 값을 할당
    /// <summary>
    /// 대상의 결과값이 이미나왔다면 대입조차 하지 않고, 아직 없다면 Try
    /// </summary>
    public void GetSocket(ref BaseSocket result, Func<BaseSocket, bool> predicate)
        => result ??= GetSocket(predicate); // result가 null일때만 값을 할당

    public void SocketAction(Action<BaseSocket> WantAction)
    {
        if (enabled) WantAction(this);
    }
    public void SocketActionByType(SocketType wantType, Action<BaseSocket> WantAction)
    {
        if (enabled && Type == wantType) WantAction(this);
    }
    public void SocketActionByPredicate(Func<BaseSocket, bool> predicate, Action<BaseSocket> WantAction)
    {
        if (enabled && predicate(this)) WantAction(this);
    }
}
