using System.Collections;
using UnityEngine;

public abstract class BaseGlobalManager : MonoBehaviour, IGlobalManager
{
    public bool IsInit { get; private set; }

    public bool IsActive => throw new System.NotImplementedException();

    public bool EndInit() => IsInit = true;

    public virtual void Exit() {  }

    public virtual IEnumerator Initialize() { yield break; }

    public IEnumerator Initialize(IModuleHub hub)
    {
        throw new System.NotImplementedException();
    }

    public virtual IEnumerator LateInitialize() { yield break; }

    public void SetActive(bool active)
    {
        throw new System.NotImplementedException();
    }
}