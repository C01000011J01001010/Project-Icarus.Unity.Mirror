using UnityEngine;

public class CharacterWingFlapModule : MonoBehaviour
{
    Rigidbody rigid;
    public float flapForce = 0.01f;
    protected void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // 유저1
        rigid.AddForce(transform.up * flapForce);
        rigid.AddTorque(Vector3.forward);

        // 유저2
        rigid.AddForce(transform.up * flapForce * 0.8f);
        rigid.AddTorque(-Vector3.forward);
    }
}
