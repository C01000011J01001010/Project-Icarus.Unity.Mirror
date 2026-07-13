using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using Core;
using Core.Manager;


[RequireComponent(typeof(baseCharacterAnim))]
[RequireComponent(typeof(CharacterStateController))]

public class BaseCharacter : TickableModuleHub, 
    IScenedGameObject, ITickable, IFixedTickable
{
    [SerializeField] protected int _priority = 11;
    public int Priority => _priority;


    public baseCharacterAnim anim {  get; protected set; }

    public override TickGroup TickGroup => TickGroup.Character;

    public override FixedTickGroup FixedTickGroup => FixedTickGroup.Physics;

    public Vector2 inputMove;
    public bool isMove;
    public bool isSprint;
    public float moveSpeed = 2.5f;
    

    protected bool isReady;

    

    // 입력처리
    public event Action<Vector2> OnMoveInput;
    public event Action<bool> OnSprintInput;

    // 상태값 변경 처리
    public event Action<float> OnMoveSpeedChanged;


    private void OnDisable()
    {
        if (isReady)
        {
            Unsubscribe();
        }
    }

    private void OnEnable()
    {
        if (isReady)
        {
            Subscribe();
        }
    }

    public override void Exit()
    {
        base.Exit();
        if (isReady)
        {
            Unsubscribe();

            // OnDisable에서 다시 Unsubscribe하지 않도록 플래그를 끄기
            isReady = false; 
        }
    }

    protected override void RegisterPreset()
    {
        //TryAddCharacterModule<CharacterStateController>();
        //TryAddCharacterModule<CharacterInventory>();
        //TryAddCharacterModule<CharacterCropDataSheet>();
    }

    protected override void Awake()
    {
        anim = GetComponent<baseCharacterAnim>();
        base.Awake();
    }

    public override IEnumerator LateInitialize()
    {
        yield return base.LateInitialize();
        isReady = true;

        
        Subscribe();
    }

    

    protected void Subscribe()
    {
        //CharacterManager mng = WorldManager.GetManager<CharacterManager>();
        //mng?.AddList(this);
    }

    protected void Unsubscribe()
    {
        //CharacterManager mng = WorldManager.GetManager<CharacterManager>();
        //mng?.RemoveList(this);
    }

    public virtual void Move(Vector2 input)
    {
        inputMove = input;
        isMove = input.sqrMagnitude > 0.01f;
        OnMoveInput?.Invoke(input);
    }

    public void SprintHold(bool value)
    {
        isSprint = value;
        OnSprintInput?.Invoke(value);
    }

    protected override void ValidateModule(IModule module)
    {
        CheckPolicy<ICharacterModule>(module);
    }

    //public void SprintToggle() => isRun = !isRun;
}
