using Unity.VisualScripting;
using UnityEngine;

public class BoxController : MonoBehaviour
{
    FixedJoint fixedJoint;

    private Rigidbody rigid;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        fixedJoint = GetComponent<FixedJoint>();
    }

    private void Update()
    {
        if (fixedJoint.IsDestroyed())
        {
            rigid.isKinematic = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Bullet"))
        {
            rigid.isKinematic = false;
        }
    }
}