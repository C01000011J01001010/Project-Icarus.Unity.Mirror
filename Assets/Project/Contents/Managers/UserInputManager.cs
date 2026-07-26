using CoreEngine;
using CoreEngine.EventBus;

using CoreEngine.Interface;
using CoreEngine.Manager;
using System;
using System.Collections;
using Unity.VisualScripting;
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

public struct ToggleMouseLockEvent : IEvent
{
    public bool IsMouseLock { get; }
    public ToggleMouseLockEvent(bool isMouseLock) => this.IsMouseLock = isMouseLock;
}

public class UserInputManager : BaseInputManager<UserInputActions>, IManager, IPlayerInputProvider
{
    UserInputActions.PlayerActions Player;
    UserInputActions.UIActions Ui;
    #region Polling
    public Vector2 Move
    { 
        get
        {
            return inputAction.Player.Move.ReadValue<Vector2>();
        }
    }

    public Vector2 Look
    {
        get
        {
            return inputAction.Player.Look.ReadValue<Vector2>();
        }
    }

    public float ScrollDelta
    {
        get
        {
            return inputAction.Player.Scroll.ReadValue<Vector2>().y;
        }
    }
    #endregion

    public override void Exit()
    {
        base.Exit();
        //EventBus<ControllerSettingEvent>.Unsubscribe(OnControllerCall);
        PlayerInputUnsubscribe();
    }

    public override IEnumerator Initialize()
    {
        yield return base.Initialize();
        Player = inputAction.Player;
        Ui = inputAction.UI;

        //EventBus<ControllerSettingEvent>.Subscribe(OnControllerCall);
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


    private void OnMouseLock(InputAction.CallbackContext context)
    {
        OnMouseLock(!context.ReadValueAsButton());
    }

    private void OnMouseLock(bool isMouseLock)
    {
        SetCursorState(isMouseLock);
        // 📡 단발성 이벤트 발행!
        EventBus<ToggleMouseLockEvent>.Publish(new ToggleMouseLockEvent(isMouseLock));
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
