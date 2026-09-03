using CoreEngine;
using CoreEngine.EventBus;
using CoreEngine.Interface;
using CoreEngine.Manager;
using CoreEngine.Manager.Input;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InputMapType
{
    Ui,
    Player,
}

public struct OnSpaceBarWingFlappedEvent : IEvent
{

}
public struct OnMouseClickWingFlappedEvent : IEvent
{
    public bool isLeft; //false면 right
    public OnMouseClickWingFlappedEvent(bool isLeft)=> this.isLeft = isLeft;
}



public class UserInputManager : BaseInputManager<UserInputActions>, IManager, 
    IMoveInput, ILookInput, IScollDeltaInput
{
    UserInputActions.PlayerActions Player;
    UserInputActions.UIActions Ui;

    InterfaceBinderContainer binder = new();
    InterfacePublisher<IMoveInput> _iMoveInputProvider;
    InterfacePublisher<ILookInput> _iLookInputProvider;
    InterfacePublisher<IScollDeltaInput> _IScollDeltaInputProvider;
    
    Vector2 IMoveInput.value => inputAction.Player.Move.ReadValue<Vector2>();
    Vector2 ILookInput.value => inputAction.Player.Look.ReadValue<Vector2>();
    float IScollDeltaInput.value => inputAction.Player.Scroll.ReadValue<Vector2>().y;
    

    public override void Exit()
    {
        base.Exit();
        binder.UnbindAll();

        PlayerInputUnsubscribe();
    }

    public override IEnumerator Initialize()
    {
        yield return base.Initialize();
        Player = inputAction.Player;
        Ui = inputAction.UI;

        binder.Add(new InterfacePublisher<IMoveInput>(this));
        binder.Add(new InterfacePublisher<ILookInput>(this));
        binder.Add(new InterfacePublisher<IScollDeltaInput>(this));
        binder.BindAll();

        PlayerInputSubScribe();
        SwitchMap(InputMapType.Player);
        yield return null;
    }

    private void PlayerInputSubScribe()
    {
        PlayerInputUnsubscribe();
        Player.Flap.started += OnSpaceBarWingFlapped;
        Player.LeftFlap.started += OnMouseLeftClickWingFlapped;
        Player.RightFlap.started += OnMouseRightClickWingFlapped;
        Player.MouseLockOff.started += OnMouseLock;
        Player.MouseLockOff.canceled += OnMouseLock;
    }

    private void PlayerInputUnsubscribe()
    {
        Player.Flap.started -= OnSpaceBarWingFlapped;
        Player.LeftFlap.started -= OnMouseLeftClickWingFlapped;
        Player.RightFlap.started -= OnMouseRightClickWingFlapped;
        Player.MouseLockOff.started -= OnMouseLock;
        Player.MouseLockOff.canceled -= OnMouseLock;
    }


    

    

    private void OnSpaceBarWingFlapped(InputAction.CallbackContext context)
    {
        EventBus<OnSpaceBarWingFlappedEvent>.Publish(new OnSpaceBarWingFlappedEvent());
    }

    private void OnMouseLeftClickWingFlapped(InputAction.CallbackContext context)
    {
        EventBus<OnMouseClickWingFlappedEvent>.Publish(new OnMouseClickWingFlappedEvent(true));
    }

    private void OnMouseRightClickWingFlapped(InputAction.CallbackContext context)
    {
        EventBus<OnMouseClickWingFlappedEvent>.Publish(new OnMouseClickWingFlappedEvent(false));
    }



    //private void OnUseItemInput(InputAction.CallbackContext context)
    //{

    //}

    public void SwitchMap(InputMapType mapType)
    {
        inputAction.Disable();

        switch (mapType)
        {
            case InputMapType.Player: inputAction.Player.Enable(); break;
            case InputMapType.Ui: inputAction.UI.Enable(); break;
            default: Debug.LogWarning($"{mapType}은 정의되지 않은 Input Action"); break;
        }
    }
    //private void OnControllerCall(ControllerSettingEvent evt)
    //{
    //    evt.controller.SetInputProvider(this);
    //}

    // DOTO: 선입력 시스템(Input Buffering) 넣을지 고려 필요함
}
