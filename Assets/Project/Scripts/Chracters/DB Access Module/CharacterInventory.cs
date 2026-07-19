using System;
using System.Collections;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;

public class CharacterInventory : BaseDatabaseAccessModule<ItemStaticManager, ItemData>
{
    protected int curItemIndex;
    //protected InfoItemContainer curItemSlot;
    protected ItemDataContainer[] items = new ItemDataContainer[9];
    public ItemDataContainer[] Items => items;

    public event Action<int/*slotIndex*/> Event_OnSelectedSlotChanged;
    public event Action<int/*slotIndex*/> Event_OnItemSlotChanged;
    public event Action Event_OnItemUseInput;

    public override IEnumerator Initialize(IModuleHub hub)
    {
        yield return base.Initialize(hub);

        // 게임 시작 시 빈 슬롯으로 깔끔하게 초기화
        for (int i = 0; i < items.Length; i++)
        {
            items[i] = new ItemDataContainer();
        }
        // 초기 슬롯 인덱스 설정
        curItemIndex = 0;
    }

    public ItemDataContainer GetItem(int index)
    {
        if(items.TryGet(index, out var slot))
        {
            return slot;
        }
        return null;
    }

    // 컨트롤러(마우스 휠)에서 호출할 스크롤 처리 로직
    public void ScrollSlot(float scrollDelta)
    {
        if (scrollDelta == 0) return;

        // UI에 있던 인덱스 순환 로직을 Model(인벤토리)로 가져옴
        if (scrollDelta > 0) curItemIndex--;
        else curItemIndex++;

        if (curItemIndex < 0) curItemIndex = items.Length - 1;
        else if (curItemIndex >= items.Length) curItemIndex = 0;

        // 상태가 변했으므로 이벤트 발생
        // -> 구독 중인 UI가 화면을 갱신함
        Event_OnSelectedSlotChanged?.Invoke(curItemIndex);
    }

    // 마우스 클릭이나 숫자키 등으로 특정 슬롯을 직접 지정할 때 사용
    public void SetSelectedSlot(int index)
    {
        if(0 <= index && index < items.Length)
        {
            curItemIndex = index;
            Event_OnSelectedSlotChanged?.Invoke(curItemIndex);
        }
        else
        {
            Debug.LogWarning($"Selected slot {index}");
        }
    }

    /// <summary>
    /// 필드에서 아이템을 획득했을 때 호출 (itemID만 넘겨주면 됨!)
    /// </summary>
    public void AcquireItem(int itemID, int count = 1)
    {
        int curCount = count;
        if (curCount < 0) return;

        // ItemManager를 통해 ID에 해당하는 원본 데이터를 가져옴
        ItemData newItem = GetData(itemID);

        // 이미 같은 아이템이 있는 공간 찾기
        for(int i = 0; i < items.Length; i++)
        {
            ItemDataContainer itemslot = items[i];
            if (!itemslot.IsEmpty() && itemslot.Get() == newItem)
            {
                ChangeInPossessionAmount(itemslot, newItem, curCount, out curCount);

                Event_OnItemSlotChanged?.Invoke(i);
                break;
            }
        }

        if (curCount < 0) return;

        // 같은 아이템이 없다면, 비어있는 슬롯 찾아서 새로 등록
        for (int i = 0; i < items.Length; i++)
        {
            ItemDataContainer itemslot = items[i];
            // 비었다면 설정후 개수 변화
            if (itemslot.IsEmpty() && itemslot.Set(newItem))
            {
                ChangeInPossessionAmount(itemslot, newItem, curCount, out curCount);
                Event_OnItemSlotChanged?.Invoke(i);
                return;
            }
        }

        if(curCount > 0)
        {
            // 슬롯이 꽉 찼을 때의 처리 (아이템을 땅에 다시 떨구거나, 무시하거나)
            Debug.LogWarning("인벤토리(퀵슬롯)가 꽉 찼습니다!");
        }
    }

    /// <summary>
    /// 아이템 개수 변경
    /// </summary>
    private void ChangeInPossessionAmount(ItemDataContainer container, ItemData newItem, int count, out int remain)
    {
        container.Push(count, out remain);
        Debug.Log($"[획득] {newItem.NameTag} {count - remain}개 획득! (현재 슬롯에 총 {container.amount}개)");
        if (remain > 0)
        {
            Debug.Log($"아이템({newItem.NameTag}) 상한 도달 -> {remain}개 획득 불가");
            // TODO 아이템을 상한 이상으로 획득시 처리할 이벤트 넣을 곳
        }
    }


    /// <summary>
    /// 특정 슬롯의 아이템을 사용 (0번, 1번, 2번 슬롯)
    /// </summary>
    public void UseItem(int count = 1)
    {
#if UNITY_EDITOR
        if (curItemIndex < 0 || curItemIndex >= items.Length)
        {
            Debug.LogError($"슬롯번호 {curItemIndex}는 슬롯 범위를 벗어남");
            return;
        }
#endif

        ItemDataContainer item = items[curItemIndex];
        if (item.IsEmpty())
        {
            Debug.Log("빈 슬롯입니다.");
            return;
        }

        // 아이템 타입에 따른 사용
        ItemData staticData = item.Get();
        bool isSuccess = item.TryUse(Owner, count);
        // 사용 결과에 따른 처리
        if (isSuccess)
        {
            Debug.Log($"{staticData.NameTag} 아이템 사용 성공");
            // 성공했으니 종류에 따라 내구도를 깎거나 개수를 줄임!
            if (staticData is ItemData_Equipment)
            {
                item.ReduceDurability(count);
            }
            else if (staticData is ItemData_Consumable)
            {
                item.Pop(count, out _);
            }
            Event_OnItemUseInput?.Invoke();
        }
        else
        {
            Debug.LogWarning("사용 실패시 효과음 출력 바람");
        }
    }



    
}