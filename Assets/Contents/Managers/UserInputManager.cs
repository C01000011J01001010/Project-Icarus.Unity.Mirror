using Core;
using Core.EventBus;

using Core.Interface;
using Core.Manager;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InputMapType
{
    Ui,
    Player,
}

public struct OnWingFlappedEvent : IEvent
{

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
        Player.Flap.started += OnWingFalpped;
    }

    private void PlayerInputUnsubscribe()
    {
        Player.Flap.started -= OnWingFalpped;
    }

    private void OnWingFalpped(InputAction.CallbackContext context)
    {
        EventBus<OnWingFlappedEvent>.Publish(new OnWingFlappedEvent());
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
