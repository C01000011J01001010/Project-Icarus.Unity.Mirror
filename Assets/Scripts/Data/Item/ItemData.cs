using System;
using UnityEngine;

//public enum ItemType
//{
//    Equipment,  // 장비
//    Consumable, // 소모품
//    Material    // 재료
//}

public class ItemData : BaseData_ForUi
{
	//public ItemType itemType;
	public int maxStack;
    public int sellPrice;

    /// <summary>
    /// 개별 클래스에서 어떻게 할건지 정의
    /// </summary>
    /// <param name="user">이 아이템의 사용자</param>
    /// <returns></returns>
    public virtual bool TryUse(BaseCharacter user)
    {
        // 사용 가능여부 확인
        // 배열이 비어있으면 true
        bool able = Array.TrueForAll(conditions, condition => condition.IsSatisfied(user));
        if (!able) return false;

        // 사용 불가능한지 확인
        // 배열이 비어있으면 false
        bool disable = Array.Exists(constraints, constraints => constraints.IsSatisfied(user));
        if (disable) return false;

        return true;
    }

    [SerializeField] BaseItemEffect[] effects_OnUse; // 사용즉시 적용할 효과
    [SerializeField] BaseItemEffect[] effects_OnAnimEvent; // 사용 후 애니메이션 이벤트로 적용할 효과


    /// <summary>
    /// 만족해야 하는 조건
    /// </summary>
    [SerializeField] BaseCondition[] conditions;  


    /// <summary>
    /// 만족하면 안되는 조건(제약조건)
    /// </summary>
    [SerializeField] BaseCondition[] constraints;

    public BaseItemEffect[] GetEffect_OnUse() => effects_OnUse;
    public BaseItemEffect[] GetEffect_OnAnimEvent() => effects_OnAnimEvent;

    //public BaseItemCondition[] GetConditions() => conditions;
    //public BaseItemConstraint[] GetConstraints() => constraints;
}


