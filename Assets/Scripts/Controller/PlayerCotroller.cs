using Core;
using Core.EventBus;
using Core.EventBus.Event;
using Core.Interface;
using System.Collections;
using UnityEngine;



public class PlayerCotroller : BaseActor<ControllerType>, IControllerSetter
{
    IPlayerInputProvider _inputProvider;

    private UserInputManager inputManager;
    private UiController uiController;

    private PlayableCharacter character;
    private CharacterInventory inventory;

    public PlayableCharacter curTargetCharacter => character;

    // 각 객체는(특히 Ui) 어떤 캐릭터가 대상이 될지 모르니 캐릭터의 이벤트에 연결할 수 없음
    // 그러므로 컨트롤러에서 해결
    public event System.Action<PlayableCharacter>  Event_OnControllTargetSet;
    public event System.Action<PlayableCharacter>  Event_OnControllTargetRemoved;

    public override ControllerType GroupType => ControllerType.PlayerController;

    //public static event System.Action<float/*마우스 휠 스크롤*/> OnQuickSlotScrollInput;

    protected override void OnEnable()
    {
        base.OnEnable();
        UpdateManager.UPDATE_OnController += Tick;
        
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        UpdateManager.UPDATE_OnController -= Tick;
    }

    public void Exit()
    {
        if (inputManager != null)
        {
            //inputManager.Event_OnUseItemInput -= UseItem;
        }
    }

    public IEnumerator Initialize()
    {
        // GameManager/WorldManager are temporarily disabled; 주석 처리함
        // inputManager = GameManager.GetManager<UserInputManager>();
        // uiController = WorldManager.GetObject<UiController>();

        //inputManager.Event_OnUseItemInput += UseItem;
        EventBus<ControllerSettingEvent>.Publish(new ControllerSettingEvent(this));
        yield return null;
    }
    public IEnumerator LateInitialize() 
    { 
        yield break; 
    }

    public void SetInputProvider(IPlayerInputProvider inputProvider)
    {
        _inputProvider = inputProvider;
    }

    public void SetControllTarget(PlayableCharacter newCharacter)
    {
        if (newCharacter != null)
        {
            // 기존 캐릭터가 있으면 메모리 정리
            if(character)
            {
                character.OnControllTargetRemoved();
                Event_OnControllTargetRemoved?.Invoke(character);
            }
            character = newCharacter;
            inventory = newCharacter.GetModule<CharacterInventory>();

            // 새캐릭터에 이벤트 연결됐음을 알림
            character.OnControllTargetSet();
            Event_OnControllTargetSet?.Invoke(newCharacter);
        }
        else
        {
            Debug.LogWarning("컨트롤 타겟이 존재하지 않음");
        }
    }

    private void Tick(float deltaTime)
    {
        InputMove();
        //InputSprint();
        InputScroll();
    }
    #region Tick
    private void InputMove()
    {
        if (inputManager == null) return;
        character?.Move(inputManager.Move);
    }

    //private void InputSprint()
    //{
    //    if (inputManager == null) return;
    //    character?.SprintHold(inputManager.Sprint);
    //}

    private void InputScroll()
    {
        if (inputManager == null) return;
        float scrollDelta = inputManager.ScrollY;

        if (scrollDelta != 0)
        {
            inventory?.ScrollSlot(scrollDelta);
        }
    }
    #endregion


    #region Handle Ui
    public void HandleUI_QuickSlotClicked(int index)
    {
        // Controller가 UI 이벤트를 받아 Model에게 데이터 변경을 지시
        inventory?.SetSelectedSlot(index);
    }
    #endregion

    //public void UseItem()
    //{
    //    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
    //    {
    //        Debug.Log("UI 클릭 중이므로 아이템 사용 액션을 무시");
    //        return;
    //    }
    //    inventory?.UseItem();
    //}

    public override void OnDespawn()
    {
        throw new System.NotImplementedException();
    }

    public override void OnSpawn()
    {
        throw new System.NotImplementedException();
    }

    
}
