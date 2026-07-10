using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class CharacterMoveModule3D : BaseCharacterMoveModule
{
    Rigidbody rigid;

    protected override void ProcessRigidbody(Vector2 deltaMove)
    {
        rigid.AddForce(deltaMove);
    }
}