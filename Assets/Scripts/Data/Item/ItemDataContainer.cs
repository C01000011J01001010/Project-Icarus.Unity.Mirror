using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class ItemDataContainer : BaseDataContainer<ItemData>
{
    public int amount;      // 아이템 개수
    public int durability; // 아이템 내구도(장비에 사용 -> 사용 횟수 제한)


    // 새로운 아이템을 얻었을 때
    public event Action Event_OnAmountChanged;
    public event Action Event_OnDurabilityChanged;
    public event Action Event_OnClear; // ui 템칸 비우라고 알림

    // 슬롯이 비어있으면 0 반환
    public int maxStack => connectData?.maxStack ?? 0;

    // 장비가 아니거나 슬롯이 비어있으면 0 반환
    public int maxDurability => (connectData as ItemData_Equipment)?.maxDurability ?? 0;

    public ItemDataContainer() : base()
    {
        //currentObject = null; // class의 default는 null이니 생략
    }

    public ItemDataContainer(ItemData InitialObject) : base(InitialObject)
    {
        Set(InitialObject);
    }

    public virtual bool TryUse(BaseCharacter user,int count)
    {
        // 아이템 자체적으로 사용 가능한지 확인
        bool able = connectData.TryUse(user);
        if (!able) return false;
        
        // 개수나 내구도 같은 간단한 것들 확인
        if (connectData is ItemData_Equipment)
        {
            // 장비라면 count만큼 내구도 감소
            if (durability < count) return false;
        }
        else
        {
            // 그 외 아이템이 사용된 경우
            if (amount < count) return false;
        }
        return true;
    }

    public virtual void Push(int pushAmount, out int remain)
    {
        if (pushAmount < 1)
        {
            Debug.LogError("pushAmount 개수가 음수이거나 0임");
            remain = 0;
            return;
        }
        AddAndGetExcess(ref amount, pushAmount, maxStack, out remain);

        if(connectData is ItemData_Equipment)
        {
            durability = maxDurability;
        }

        // ui에 변화 알림
        Event_OnAmountChanged?.Invoke();
    }

    

    public virtual void Pop(int popAmount, out int remain)
    {
        if (popAmount < 1)
        {
            Debug.LogError("popAmount 개수가 음수이거나 0임");
            remain = 0;
            return;
        }

        amount -= popAmount;
        remain = 0;

        // 아이템 추가 후 최대치보다 많이 갖게 되면 나머지는 뱉어냄
        if(amount < 0)
        {
            remain = amount * -1;
            amount = 0;
        }
        if(amount == 0)
        {
            Clear();
        }

        // ui에 변화 알림
        Event_OnAmountChanged?.Invoke();
    }

    public void ReduceDurability(int count = 1)
    {
        //if(currentObject is not Item_Equipment)
        //{
        //    Debug.LogError("장비가 아닌 객체의 내구도 변화 시도!");
        //    return false;
        //}
        //if (durability - count < 0) return false;
        durability -= count;
        Event_OnDurabilityChanged?.Invoke();
        //return true;
    }

    public void RefillDurability()
    {
        if (connectData is not ItemData_Equipment)
        {
            Debug.LogError("장비가 아닌 객체의 내구도 변화 시도!");
            return;
        }
        durability = maxDurability;
        Event_OnDurabilityChanged?.Invoke();
    }

    public void RefillDurability(int fillAmount, out int remain)
    {
        remain = 0;
        if (connectData is not ItemData_Equipment)
        {
            Debug.LogError("장비가 아닌 객체의 내구도 변화 시도!");
            return;
        }
        if (fillAmount < 1)
        {
            Debug.LogError("fillAmount 개수가 음수이거나 0임");
            return;
        }
        AddAndGetExcess(ref durability, fillAmount, maxDurability, out remain);

        // ui에 변화 알림
        Event_OnDurabilityChanged?.Invoke();
    }

    private void AddAndGetExcess(ref int cur, int plus, int max, out int remain)
    {
        cur += plus;
        remain = 0;

        // 최대치보다 많이 갖게 되면 나머지는 뱉어냄
        if (cur > max)
        {
            remain = cur - max;
            cur = max;
        }
    }

    // 슬롯이 비어있는지 확인하는 헬퍼 함수
    public bool IsEmpty() => connectData == null || amount <= 0;

    // 슬롯 초기화 (아이템을 다 썼을 때)
    public void Clear()
    {
        connectData = null;
        amount = 0;
        Event_OnClear?.Invoke();
    }

    public override ItemData Set(ItemData newObject)
    {
        Clear();
        base.Set(newObject); // 부모 클래스의 기본 Set 기능(currentObject 할당) 실행

        if (newObject == null)
        {
            return null;
        }

        // 아이템 종류에 따라 초기값 셋팅
        if (newObject is ItemData_Equipment eq)
        {
            durability = eq.maxDurability; // 획득 시 내구도 꽉 채우기
        }
        else
        {
            // 소모품/재료는 내구도 0
            // amount는 Push나 Pop에서 알아서 관리할 테니 놔둠
            durability = 0;
        }

        return connectData;
    }
}
