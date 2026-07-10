using System.Collections;
using UnityEngine;
using Core;
public abstract class BaseModule<TOnwer> : IModule
    where TOnwer : MonoBehaviour, IModuleHub
{
    public TOnwer Onwer { get; private set; }
    public bool IsActive { get; protected set; }

    public void Exit()
    {
        throw new System.NotImplementedException();
    }

    public IEnumerator Initialize(IModuleHub owner)
    {
        Onwer = owner.AsOrThrow<TOnwer>();
        yield break;
    }

    public void SetActive(bool active)
    {
        IsActive = active;
    }
}