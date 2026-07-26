
using System;
using System.Collections;
using UnityEngine;
using CoreEngine;

public abstract class BaseCharacterModule : BaseMonoModule<BaseCharacter>//, IModule
{
    //public BaseCharacter Owner { get; protected set; }


    public event Action<bool> Evnet_OnSetActive;

    public override void SetActive(bool active)
    {
        base.SetActive(active);
        Evnet_OnSetActive?.Invoke(active);
    }

    private void OnDestroy()
    {
        Evnet_OnSetActive = null;
    }
}