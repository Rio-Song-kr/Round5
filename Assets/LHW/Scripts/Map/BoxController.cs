using Unity.VisualScripting;
using UnityEngine;

public class BoxController : MonoBehaviour
{
    FixedJoint2D fixedJoint;

    private Rigidbody2D rigid;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        fixedJoint = GetComponent<FixedJoint2D>();
    }

    private void Update()
    {
        if (fixedJoint.IsDestroyed())
        {
            rigid.bodyType = RigidbodyType2D.Dynamic;
            rigid.mass = 0.2f;
            rigid.gravityScale = 0.1f;
        }        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            rigid.bodyType = RigidbodyType2D.Dynamic;
            rigid.mass = 0.2f;
            rigid.gravityScale = 0.1f;
        }
    }
}