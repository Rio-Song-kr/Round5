using Photon.Pun;
using UnityEngine;

public class BigBullet : MonoBehaviour
{
    [SerializeField] private GameObject _bullet;
    [SerializeField] private GameObject _hitEffect;
    [SerializeField] private GameObject _explosionEffect;
    private ParticleSystem _particleSystem;

    void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    // [PunRPC]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 8)
        {
            Vector2 hitPoint = collision.ClosestPoint(transform.position);
            GameObject effect = PhotonNetwork.Instantiate("Fragment", hitPoint, Quaternion.identity);
            effect.transform.LookAt(hitPoint + (hitPoint - new Vector2(collision.transform.position.x, collision.transform.position.y)).normalized);
            _particleSystem.Stop();
            // transform.SetParent(null);
            // Instantiate(_explosionEffect, hitPoint, Quaternion.identity);
            // PhotonNetwork.Instantiate("Explosive", hitPoint, Quaternion.identity);
            Destroy(gameObject, 1f);
            Destroy(_bullet);
        }
    }
}
