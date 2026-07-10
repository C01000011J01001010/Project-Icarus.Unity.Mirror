using UnityEngine;

// ChractorController를 사용하면 RigidBody와의 충돌 처리가 안되니 사용
public class BaseRigidBodyPush : MonoBehaviour
{
	public LayerMask pushLayers;
	public bool canPush;
	[Range(0.5f, 5f)] public float strength = 1.1f;

    /* OnControllerColliderHit가 호출되는 경우
     *		CharacterController.Move()
     *		CharacterController.SimpleMove()
     * 를 실행할 때 충돌시
	 */
    private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (canPush) PushRigidBodies(hit);
	}

    /* hit 프로퍼티
     *		hit.gameObject	→ 부딪힌 오브젝트
     *		hit.collider		→ 부딪힌 콜라이더
     *		hit.moveDirection → 플레이어가 이동하던 방향
     *		hit.normal		→ 부딪힌 표면의 법선 벡터
     *		hit.point		→ 충돌 지점 좌표
	 */

    private void PushRigidBodies(ControllerColliderHit hit)
	{
		Rigidbody hitRigidBody = hit.collider.attachedRigidbody;
		if (hitRigidBody is null || hitRigidBody.isKinematic) return;

		var bodyLayerMask = 1 << hitRigidBody.gameObject.layer;
		if ((bodyLayerMask & pushLayers.value) == 0) return;

        Debug.Log($"hit == {hit.gameObject.name}");

        // 플레이어가 위에서 아래로 물체를 누르고 있을 때
        if (hit.moveDirection.y < -0.3f) return;

        // xz 평면에서 밀고있는 방향
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0.0f, hit.moveDirection.z);

		// 충돌체의 rigidBody를 써서 충돌한 물체가 이동하도록함
		hitRigidBody.AddForce(pushDir * strength, ForceMode.Impulse);
	}
}