using UnityEngine;
using System;

public abstract class BaseCondition : ScriptableObject
{
    /// <summary>
    /// 특정 조건을 만족했는가 체크
    /// </summary>
    public abstract bool IsSatisfied(BaseCharacter character);
}