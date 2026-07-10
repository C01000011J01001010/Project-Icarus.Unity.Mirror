using System;
using System.Collections;
using System.Collections.Generic;

public delegate void DelegateGetSockets(List<BaseSocket> result);
public delegate void DelegateGetSocketsByType(List<BaseSocket> result, SocketType wantType);
public delegate void DelegateGetSocketsByPredicate(List<BaseSocket> result, Func<BaseSocket, bool> predicate);

public delegate void DelegateGetSocket(ref BaseSocket result);
public delegate void DelegateGetSocketByType(ref BaseSocket result, SocketType wantType);
public delegate void DelegateGetSocketByPredicate(ref BaseSocket result, Func<BaseSocket, bool> predicate);

public delegate void DelegateSocketAction(Action<BaseSocket> WantAction);
public delegate void DelegateSocketActionByType(SocketType wantType, Action<BaseSocket> WantAction);
public delegate void DelegateSocketActionByPredicate(Func<BaseSocket, bool> predicate, Action<BaseSocket> WantAction);

public interface ISocketContainer // 소켓x -> 소켓을 가진 애
{
    public bool predicate(BaseSocket socket);

    //--------------------------------------------------------------------------

    // 무기도 컨테이너라고 한다면
    // 캐릭터가 무기을 소유하고 있는 형태는 부모자식 관계가 됨
    // 캐릭터에서 무기을 바꾼다고 했을 때에, 캐릭터는 자식 컨테이너인 무기에 있는 내용만 참조하면 편해짐

    // 확인!
    public void GetSockets(List<BaseSocket> result);
    public void GetSockets(List<BaseSocket> result, SocketType wantType);
    public void GetSockets(List<BaseSocket> result, Func<BaseSocket, bool> predicate); // predicate 조건을 서술하는부분

    public BaseSocket GetSocket();
    public BaseSocket GetSocket(SocketType wantType);
    public BaseSocket GetSocket(Func<BaseSocket, bool> predicate);

    // 추가
    public void AddSocket(BaseSocket target);
    public void AddSocket(params BaseSocket[] target);

    //제거
    public void RemoveSocket(BaseSocket target);
    public void RemoveSocket(Func<BaseSocket, bool> predicate);

    // 콜백
    public void SocketAction(Action<BaseSocket> WantAction);
    public void SocketActionByType(SocketType wantType, Action<BaseSocket> WantAction);
    public void SocketActionByPredicate(Func<BaseSocket, bool> predicate, Action<BaseSocket> WantAction);

}