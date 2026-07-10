/// <summary>
/// Update를 직접 실행하지 않고 다른 객체에 위임하여 실행하도록 하는 인터페이스
/// </summary>
public interface ITickable
{
    void Tick(float deltaTime);
}