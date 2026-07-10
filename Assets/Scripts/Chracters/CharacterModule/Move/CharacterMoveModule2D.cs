using UnityEngine;
using static UnityEngine.EventSystems.StandaloneInputModule;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterMoveModule2D : BaseCharacterMoveModule
{
    Rigidbody2D rigid;

    public override void OnMoveInput(Vector2 input)
    {
        base.OnMoveInput(input);

        // 3. Scale -1을 이용한 좌우 반전 로직
        // x값이 0일 때는 마지막 방향을 유지하기 위해 '0이 아닐 때만' 업데이트
        if (input.x != 0)
        {
            float direction = input.x > 0 ? 1f : -1f;

            // 부모의 Scale을 뒤집어 하위 무기, 이펙트 위치까지 한꺼번에 반전
            transform.localScale = new Vector3(direction, 1f, 1f);
        }
        // 위 아래 일 시 정상 scale로 변경
        else if (input.y != 0)
        {
            transform.localScale = Vector3.one;
        }
    }


    protected override void ProcessRigidbody(Vector2 deltaMove)
    {
        rigid.MovePosition(rigid.position + deltaMove);
    }
}