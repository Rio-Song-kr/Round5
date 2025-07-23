using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigBullet : MonoBehaviour
{
    [SerializeField] private GameObject _bullet;
    [SerializeField] private GameObject _hitEffect;
    private ParticleSystem _particleSystem;

    void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 8)
        {
            Vector2 hitPoint = collision.ClosestPoint(transform.position);
            GameObject effect = Instantiate(_hitEffect, hitPoint, Quaternion.identity);
            effect.transform.LookAt(hitPoint + (hitPoint - new Vector2(collision.transform.position.x, collision.transform.position.y)).normalized);
            _particleSystem.Stop();
            transform.SetParent(null);
            Destroy(gameObject, 1f);
            Destroy(_bullet);
        }
    }
}
