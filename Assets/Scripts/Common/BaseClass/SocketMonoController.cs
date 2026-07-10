using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 소켓을 갖는 모든 객체가 상속받아야함
public class SocketMonoController : MonoBehaviour, ISocketContainer
{
    // 실제 소켓들을 추가할 때

    // 2개 이상의 소켓을 등록할 때
    public event DelegateGetSockets OnGetSockets;
    public event DelegateGetSocketsByType OnGetSocketsByType;
    public event DelegateGetSocketsByPredicate OnGetSocketsByPredicate;

    // 1개의 소켓을 등록할 때
    public event DelegateGetSocket OnGetSocket;
    public event DelegateGetSocketByType OnGetSocketByType;
    public event DelegateGetSocketByPredicate OnGetSocketByPredicate;

    // 소켓에서 실행할 콜백을 등록할 때
    public event DelegateSocketAction OnSocketAction;
    public event DelegateSocketActionByType OnSocketActionByType;
    public event DelegateSocketActionByPredicate OnSocketActionByPredicate;

    public virtual void Exit()
    {
        RemoveSocket(x=>true);
    }

    public virtual void Initialize()
    {
        AddSocket(GetComponentsInChildren<BaseSocket>());
    }

    

    public virtual bool predicate(BaseSocket socket)
    {
        return true;
    }

    //--------------------------------------------------------------------------

    public void GetSockets(List<BaseSocket> result)
        => OnGetSockets?.Invoke(result);
    public void GetSockets(List<BaseSocket> result, SocketType wantType)
        => OnGetSocketsByType?.Invoke(result, wantType);
    public void GetSockets(List<BaseSocket> result, Func<BaseSocket, bool> predicate)
        => OnGetSocketsByPredicate?.Invoke(result, predicate);

    public BaseSocket[] GetSockets()
    {
        if (OnGetSockets is null) return null;

        List<BaseSocket> result = new();
        OnGetSockets?.Invoke(result);
        return result.ToArray();
    }
    public BaseSocket[] GetSockets(SocketType wantType)
    {
        if (OnGetSocketsByType is null) return null;

        List<BaseSocket> result = new();
        OnGetSocketsByType?.Invoke(result, wantType);
        return result.ToArray();
    }
    public BaseSocket[] GetSockets(Func<BaseSocket, bool> predicate)
    {
        if (OnGetSocketsByPredicate is null) return null;

        List<BaseSocket> result = new();
        OnGetSocketsByPredicate?.Invoke(result, predicate);
        return result.ToArray();
    }

    public BaseSocket GetSocket()
    {
        BaseSocket result = null;
        OnGetSocket?.Invoke(ref result);
        return result;
    }
    public BaseSocket GetSocket(SocketType wantType)
    {
        BaseSocket result = null;
        OnGetSocketByType?.Invoke(ref result, wantType);
        return result;
    }
    public BaseSocket GetSocket(Func<BaseSocket, bool> predicate)
    {
        BaseSocket result = null;
        OnGetSocketByPredicate?.Invoke(ref result, predicate);
        return result;
    }

    public void AddSocket(params BaseSocket[] targets)
    {
        foreach (var current in targets)
        {
            AddSocket(current);
        }
    }
    public void AddSocket(BaseSocket target)
    {
        if (target is null) return;
        OnGetSockets -= target.GetSockets;
        OnGetSockets += target.GetSockets;
        OnGetSocketsByType -= target.GetSockets;
        OnGetSocketsByType += target.GetSockets;
        OnGetSocketsByPredicate -= target.GetSockets;
        OnGetSocketsByPredicate += target.GetSockets;

        OnGetSocket -= target.GetSocket;
        OnGetSocket += target.GetSocket;
        OnGetSocketByType -= target.GetSocket;
        OnGetSocketByType += target.GetSocket;
        OnGetSocketByPredicate -= target.GetSocket;
        OnGetSocketByPredicate += target.GetSocket;

        OnSocketAction -= target.SocketAction;
        OnSocketAction += target.SocketAction;
        OnSocketActionByType -= target.SocketActionByType;
        OnSocketActionByType += target.SocketActionByType;
        OnSocketActionByPredicate -= target.SocketActionByPredicate;
        OnSocketActionByPredicate += target.SocketActionByPredicate;
    }

    

    public void RemoveSocket(BaseSocket target)
    {
        if (target is null) return;

        OnGetSockets -= target.GetSockets;
        OnGetSocketsByType -= target.GetSockets;
        OnGetSocketsByPredicate -= target.GetSockets;

        OnGetSocket -= target.GetSocket;
        OnGetSocketByType -= target.GetSocket;
        OnGetSocketByPredicate -= target.GetSocket;

        OnSocketAction -= target.SocketAction;
        OnSocketActionByType -= target.SocketActionByType;
        OnSocketActionByPredicate -= target.SocketActionByPredicate;

    }

    public void RemoveSocket(Func<BaseSocket, bool> predicate)
    {
        foreach (var current in GetSockets(predicate))
            RemoveSocket(current);
    }

    public void SocketAction(Action<BaseSocket> WantAction)
        => OnSocketAction?.Invoke(WantAction);

    public void SocketActionByType(SocketType wantType, Action<BaseSocket> WantAction)
        => OnSocketActionByType?.Invoke(wantType, WantAction);

    public void SocketActionByPredicate(Func<BaseSocket, bool> predicate, Action<BaseSocket> WantAction)
        => OnSocketActionByPredicate?.Invoke(predicate, WantAction);
}
