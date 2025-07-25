using Unity.VisualScripting;
using UnityEngine;

public class MultiJointBoxController : MonoBehaviour
{
    FixedJoint2D[] fixedJoint;

    private Rigidbody2D rigid;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        fixedJoint = GetComponents<FixedJoint2D>();
    }

    private void Update()
    {
        if (FixedJointDisabled())
        {
            rigid.bodyType = RigidbodyType2D.Dynamic;
            rigid.mass = 0.1f;
            rigid.gravityScale = 0.3f;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet") || collision.gameObject.CompareTag("Hammer"))
        {
            rigid.bodyType = RigidbodyType2D.Dynamic;
            rigid.mass = 0.1f;
            rigid.gravityScale = 0.3f;
        }
    }

    private bool FixedJointDisabled()
    {
        for (int i = 0; i < fixedJoint.Length; i++)
        {
            if (fixedJoint != null && fixedJoint[i].IsDestroyed() || (fixedJoint != null && fixedJoint[i].connectedBody != null && fixedJoint[i].connectedBody.bodyType == RigidbodyType2D.Dynamic))
            {                
                return true;
            }
        }
        return false;
    }
}
