using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour, IBullet
{
    [SerializeField] public float Speed;
    [SerializeField] public float Damage;
    [SerializeField] private Rigidbody2D _rb;

    void Start()
    {
        BulletMove();
    }

    public void BulletMove()
    {
        _rb.AddForce(transform.up * Speed, ForceMode2D.Impulse);
        Destroy(gameObject, 4f);
    }

}

