using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuickSlot : BaseUi, IGlobalUi
{
    private BaseButton[] itemSlots;
    private ItemViewer[] itemViewers;
    private PlayerCotroller controller; // (참고: Controller 오타가 있지만 기존 코드 유지)

    // [추가] UI에서 현재 선택된 슬롯 번호를 기억하기 위한 변수
    private int currentIndex = -1;


    public override void Exit()
    {
        // 이벤트 정리
        for (int i = 0; i < itemSlots.Length; i++)
        {
            itemSlots[i].ClearCallback();
        }

        if (controller != null)
        {
            controller.Event_OnControllTargetSet -= OnControllTargetSet;
            controller.Event_OnControllTargetRemoved -= OnControllTargetRemoved;
        }
    }

    public override IEnumerator Initialize()
    {
        // WorldManager temporarily disabled
        // controller = WorldManager.GetObject<PlayerCotroller>();
        itemSlots = GetComponentsInChildren<BaseButton>();
        itemViewers = new ItemViewer[itemSlots.Length];

        for (int i = 0; i < itemSlots.Length; i++)
        {
            int index = i; // callback 등록을 위해 변수 생성
            
            // 같은 오브젝트에 붙은 뷰어 가져오기
            itemViewers[i] = itemSlots[i].GetComponent<ItemViewer>();

            yield return itemSlots[i].Initialize();

            // 버튼 클릭 시 로컬 함수 호출
            itemSlots[i].AddCallback(() => OnSlotClicked(index));
        }

        // controller may be null while WorldManager is disabled
        // if (controller != null)
        // {
        //     controller.Event_OnControllTargetSet += OnControllTargetSet;
        //     controller.Event_OnControllTargetRemoved += OnControllTargetRemoved;
        // }
        yield return null;
    }

    // Model에서 데이터가 변경되었을 때 화면만 그리는 역할
    public void OnSelectedSlotChanged(int newIndex)
    {
        if (itemSlots == null || itemSlots.Length == 0) return;

        
        if(0 <= currentIndex && currentIndex < itemSlots.Length)
        {
            itemSlots[currentIndex].SetInteractable(true);
        }
        // 새로 변경된 인덱스를 UI 캐시 변수에 저장
        currentIndex = newIndex;

        GameObject targetSlot = itemSlots[currentIndex].gameObject;
        itemSlots[currentIndex].SetInteractable(false);
        EventSystem.current.SetSelectedGameObject(targetSlot);
    }

    private void OnSlotClicked(int index)
    {
        // [추가] 이미 선택된 버튼을 또 눌렀다면 아무 작업도 하지 않고 무시
        if (index == currentIndex)
        {
            return;
        }

        // V -> C [O]
        if (controller != null)
        {
            controller.HandleUI_QuickSlotClicked(index);
        }
    }

    // 컨트롤 타겟이 바뀔때마다 이벤트가 연결되는 대상 교체
    private void OnControllTargetSet(PlayableCharacter newCharacter)
    {
        CharacterInventory inventory = newCharacter.GetModule<CharacterInventory>();
        if (inventory != null)
        {
            inventory.Event_OnSelectedSlotChanged += OnSelectedSlotChanged;
            inventory.Event_OnItemSlotChanged += OnItemSlotChanged;
            
            // view에 model을 연결
            for(int i = 0; i < itemViewers.Length; i++)
            {
                ItemViewer viewer = itemViewers[i];
                viewer.Connect(inventory.Items[i]);
                viewer.UpdateView();
            }

            OnSlotClicked(0); // 기본 선택
        }
    }

    private void OnControllTargetRemoved(PlayableCharacter oldCharacter)
    {
        CharacterInventory inventory = oldCharacter.GetModule<CharacterInventory>();
        if (inventory != null)
        {
            inventory.Event_OnSelectedSlotChanged -= OnSelectedSlotChanged;
            inventory.Event_OnItemSlotChanged -= OnItemSlotChanged;
        }
    }

    private void OnItemSlotChanged(int index)
    {
        itemViewers[index].UpdateView();
    }
}