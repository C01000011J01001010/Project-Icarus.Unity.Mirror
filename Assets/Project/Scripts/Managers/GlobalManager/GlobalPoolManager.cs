public class GlobalPoolManager : BasePoolManager<GlobalPoolType>, IGlobalManager
{
    private bool _isInit = false;
    public bool IsInit => _isInit;

    public bool EndInit() => _isInit = true;
}