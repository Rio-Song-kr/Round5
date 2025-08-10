using System.Collections;
using Photon.Pun;
using UnityEngine;

public class BigBullet : MonoBehaviourPun
{
    [SerializeField] private GameObject _bullet;
    [SerializeField] private GameObject _hitEffect;
    [SerializeField] private GameObject _explosionEffect;
    private ParticleSystem _particleSystem;
    private Coroutine _coroutine;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        _particleSystem.Clear();
        _particleSystem.Play();

        _coroutine = StartCoroutine(ReturnToPool());
    }

    private void OnDisable()
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
            // PhotonNetwork.Destroy(gameObject);
        }
    }

    private IEnumerator ReturnToPool()
    {
        yield return new WaitForSeconds(_particleSystem.main.duration);
        _particleSystem.Clear();
        _particleSystem.Stop();
        // PhotonNetwork.Destroy(gameObject);
        _coroutine = null;
        gameObject.SetActive(false);
    }

    // [PunRPC]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 8)
        {
            if (!photonView.IsMine) return;
            var hitPoint = collision.ClosestPoint(transform.position);
            GameObject effect = null;
            if (PhotonNetwork.OfflineMode == false)
                effect = PhotonNetwork.Instantiate("Fragment", hitPoint, Quaternion.identity);
            else
                effect = Instantiate(_hitEffect, hitPoint, Quaternion.identity);
            effect.transform.LookAt(hitPoint +
                                    (hitPoint - new Vector2(collision.transform.position.x, collision.transform.position.y))
                                    .normalized);
            var vfxEffect = effect.GetComponent<VfxFragmentEffect>();
            vfxEffect.Play();

            _particleSystem.Stop();
            var damagable = collision.GetComponent<IDamagable>();
            if (damagable != null)
            {
                float damage = CardManager.Instance.GetCaculateCardStats().DefaultDamage;
                Debug.Log($"Damage : {damage}");
                damagable.TakeDamage(damage, hitPoint,
                    (collision.transform.position - transform.position).normalized); // IDamagable 인터페이스를 통해 데미지 적용
            }

            // Instantiate(_explosionEffect, hitPoint, Quaternion.identity);
            // PhotonNetwork.Instantiate("Explosive", hitPoint, Quaternion.identity);
            // gameObject.transform.SetParent(null);
            // PhotonNetwork.Destroy(gameObject, 1f);
            // PhotonNetwork.Destroy(_bullet);

            if (_bullet.activeSelf)
                PhotonNetwork.Destroy(_bullet);
        }
    }
}