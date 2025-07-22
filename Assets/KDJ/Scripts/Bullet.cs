using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour, IBullet
{
    [SerializeField] public float Speed;
    [SerializeField] public float Damage;
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private GameObject _bigBullet;
    [SerializeField] private GameObject _hitEffect;
    [SerializeField] private bool _isBigBullet;

    private void Awake()
    {
        if (_isBigBullet)
        {
            _bigBullet.SetActive(true);
        }
        else
        {
            _bigBullet.SetActive(false);
        }
    }

    void Start()
    {
        BulletMove();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject effect = Instantiate(_hitEffect, transform.position, Quaternion.identity);
        effect.transform.LookAt(collision.contacts[0].point + collision.contacts[0].normal);
        _bigBullet.transform.SetParent(null);
        _bigBullet.GetComponent<ParticleSystem>().Stop();
        Destroy(_bigBullet, 1f);
        Destroy(gameObject);
    }

    public void BulletMove()
    {
        _rb.AddForce(transform.up * Speed, ForceMode2D.Impulse);
        Destroy(gameObject, 4f);
    }

    public void Attack()
    {
        // Player 스크립트 생기면 해당 플레이어를 받아와서 TakeDamage 메소드 호출
    }

}

