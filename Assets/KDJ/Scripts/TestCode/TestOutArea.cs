using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestOutArea : MonoBehaviour
{
    [SerializeField] private GameObject _borderEffect;

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

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 8)
        {
            Rigidbody2D rb2d = collision.gameObject.GetComponent<Rigidbody2D>();
            // 플레이어쪽 IDamagable 인터페이스 구현 되면 주석 제거
            // IDamagable player = collision.gameObject.GetComponent<IDamagable>();
            // if (player != null)
            // player.TakeDamage(6);
            if (rb2d != null)
            {
                rb2d.velocity = Vector2.zero; // 속도 초기화
                switch (gameObject.name)
                {
                    case "Left":
                        GameObject effectL = Instantiate(_borderEffect, collision.transform.position, Quaternion.identity);
                        effectL.transform.LookAt(collision.transform.position + Vector3.right);
                        rb2d.AddForce(Vector2.right * 17f, ForceMode2D.Impulse);
                        break;
                    case "Right":
                        GameObject effectR = Instantiate(_borderEffect, collision.transform.position, Quaternion.identity);
                        effectR.transform.LookAt(collision.transform.position + Vector3.left);
                        rb2d.AddForce(Vector2.left * 17f, ForceMode2D.Impulse);
                        break;
                    case "Up":
                        GameObject effectU = Instantiate(_borderEffect, collision.transform.position, Quaternion.identity);
                        effectU.transform.LookAt(collision.transform.position + Vector3.down);
                        rb2d.AddForce(Vector2.down * 17f, ForceMode2D.Impulse);
                        break;
                    case "Down":
                        GameObject effectD = Instantiate(_borderEffect, collision.transform.position, Quaternion.identity);
                        effectD.transform.LookAt(collision.transform.position + Vector3.up);
                        rb2d.AddForce(Vector2.up * 17f, ForceMode2D.Impulse);
                        break;
                }
            }
        }
    }
}
