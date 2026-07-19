using System.Collections.Generic;

public abstract class BasePublisher
{
    private List<IObserver> observers = new List<IObserver>();

    // 관찰자 등록
    public void RegisterObserver(IObserver observer)
    {
        if(observer is not null)
        {
            observers.Add(observer);
        }
    }

    // 관찰자 해지
    public void RemoveObserver(IObserver observer)
    {
        if(observer is not null && observers.Contains(observer))
        {
            observers.Remove(observer);
        }
    }
    // 변경사항 알림
    public void NotifyObserver()
    {
        foreach(IObserver observer in observers)
        {
            observer.OnDataChanged();
        }
    }
}

public abstract class BasePublisher<T>
{
    private T data;


    private List<IObserver<T>> observers = new List<IObserver<T>>();

    // 관찰자 등록
    public void RegisterObserver(IObserver<T> observer)
    {
        if (observer is not null)
        {
            observers.Add(observer);
        }
    }

    // 관찰자 해지
    public void RemoveObserver(IObserver<T> observer)
    {
        if (observer is not null && observers.Contains(observer))
        {
            observers.Remove(observer);
        }
    }

    // 변경사항 알림
    public void NotifyObserver()
    {
        foreach (IObserver<T> observer in observers)
        {
            observer.OnDataChanged(data);
        }
    }
}