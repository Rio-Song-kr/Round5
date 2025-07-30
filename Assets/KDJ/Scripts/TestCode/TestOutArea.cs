using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestOutArea : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 8)
        {
            Rigidbody2D rb2d = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rb2d != null)
            {
                Vector2 originalPosition = collision.transform.position;
                Vector2 forceDirection = (originalPosition - collision.contacts[0].point).normalized;
                rb2d.AddForce(forceDirection * 17f, ForceMode2D.Impulse);
            }
        }
    }
}
