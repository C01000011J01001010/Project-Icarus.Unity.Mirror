
public interface IObserver
{
    public void OnDataChanged();

}

public interface IObserver<T>
{
    public void OnDataChanged(T data);
}



