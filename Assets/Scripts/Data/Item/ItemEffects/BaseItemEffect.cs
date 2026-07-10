using UnityEngine;
using System;

public abstract class BaseItemEffect : ScriptableObject
{
    public abstract void ApplyEffect(BaseCharacter character, ItemDataContainer item);
}