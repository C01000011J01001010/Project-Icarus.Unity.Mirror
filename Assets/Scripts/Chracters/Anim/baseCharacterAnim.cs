using UnityEngine;
using UnityEngine.Windows;

public class baseCharacterAnim : BaseAnim
{
    int Hash_InputX;
    int Hash_InputY;
    int Hash_IsMove;
    int Hash_IsSprint;


    protected override void GetAnimPrarmHash()
    {
        Hash_InputX = Animator.StringToHash("InputX");
        Hash_InputY = Animator.StringToHash("InputY");
        Hash_IsMove = Animator.StringToHash("IsMove");
        Hash_IsSprint = Animator.StringToHash("IsSprint");
    }

    private void Awake()
    {
        GetAnimPrarmHash();
    }

    public void SetInputMove(Vector2 inputMove)
    {
        SetParam(Hash_InputX, inputMove.x);
        SetParam(Hash_InputY, inputMove.y);
    }

    public void SetIsMove(bool isMove) => SetParam(Hash_IsMove, isMove);
    public void SetIsSprint(bool isRun) => SetParam(Hash_IsSprint, isRun);
}
