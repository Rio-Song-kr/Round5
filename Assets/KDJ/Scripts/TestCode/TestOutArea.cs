using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestOutArea : MonoBehaviour
{
    [SerializeField] private GameObject _borderEffect;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 8)
        {
            Rigidbody2D rb2d = collision.gameObject.GetComponent<Rigidbody2D>();
            IDamagable player = collision.gameObject.GetComponent<IDamagable>();

            if (rb2d != null && player != null)
            {
                rb2d.velocity = Vector2.zero; // 속도 초기화
                switch (gameObject.name)
                {
                    case "Left":
                        GameObject effectL = Instantiate(_borderEffect, collision.transform.position, Quaternion.identity);
                        effectL.transform.LookAt(collision.transform.position + Vector3.right);
                        rb2d.AddForce(Vector2.right * 17f, ForceMode2D.Impulse);
                        player.TakeDamage(6, collision.transform.position, Vector2.right);
                        break;
                    case "Right":
                        GameObject effectR = Instantiate(_borderEffect, collision.transform.position, Quaternion.identity);
                        effectR.transform.LookAt(collision.transform.position + Vector3.left);
                        rb2d.AddForce(Vector2.left * 17f, ForceMode2D.Impulse);
                        player.TakeDamage(6, collision.transform.position, Vector2.left);
                        break;
                    case "Up":
                        //GameObject effectU = Instantiate(_borderEffect, collision.transform.position, Quaternion.identity);
                        //effectU.transform.LookAt(collision.transform.position + Vector3.up);
                        //rb2d.AddForce(Vector2.down * 17f, ForceMode2D.Impulse);
                        break;
                    case "Down":
                        GameObject effectD = Instantiate(_borderEffect, collision.transform.position, Quaternion.identity);
                        effectD.transform.LookAt(collision.transform.position + Vector3.up);
                        rb2d.AddForce(Vector2.up * 17f, ForceMode2D.Impulse);
                        player.TakeDamage(6, collision.transform.position, Vector2.up);
                        break;
                }
            }
        }
    }
}
