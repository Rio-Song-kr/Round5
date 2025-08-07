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
            GameObject effect = null;
            if (PhotonNetwork.OfflineMode == false)
                effect = PhotonNetwork.Instantiate("Fragment", hitPoint, Quaternion.identity);
            else
                effect = Instantiate(_hitEffect, hitPoint, Quaternion.identity);
            effect.transform.LookAt(hitPoint + (hitPoint - new Vector2(collision.transform.position.x, collision.transform.position.y)).normalized);
            _particleSystem.Stop();
            IDamagable damagable = collision.GetComponent<IDamagable>();
            if (damagable != null)
            {
                float damage = CardManager.Instance.GetCaculateCardStats().DefaultDamage;
                damagable.TakeDamage(damage, hitPoint, (collision.transform.position - transform.position).normalized); // IDamagable 인터페이스를 통해 데미지 적용
            }

            // Instantiate(_explosionEffect, hitPoint, Quaternion.identity);
            // PhotonNetwork.Instantiate("Explosive", hitPoint, Quaternion.identity);
            Destroy(gameObject, 1f);
            Destroy(_bullet);
        }
    }
}
