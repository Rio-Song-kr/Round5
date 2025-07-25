using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour, IPunObservable
{
    [SerializeField] public float Speed;
    [SerializeField] public float Damage;
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private GameObject _bigBullet;
    [SerializeField] private GameObject _explosiveBullet;
    [SerializeField] private GameObject _explosiveBulletEffect;
    [SerializeField] private GameObject _hitEffect;
    [SerializeField] private bool _isBigBullet;
    [SerializeField] private bool _isExplosiveBullet;
    
    // 250726 추가
    private Vector3 _networkPos;
    
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

        if (_isExplosiveBullet)
        {
            _explosiveBullet.SetActive(true);
        }
        else
        {
            _explosiveBullet.SetActive(false);
        }
    }

    void Start()
    {
        BulletMove(Speed);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!PhotonView.Get(this).IsMine) return; // 내 총알만 파괴 가능
        
        if (_isBigBullet)
            BigBulletShot();

        if (_isExplosiveBullet)
            ExplosiveBulletShot();

        else
        {
            GameObject effect = PhotonNetwork.Instantiate(_hitEffect.name, transform.position, Quaternion.identity);
            // GameObject effect = Instantiate(_hitEffect.name, transform.position, Quaternion.identity);
            effect.transform.LookAt(collision.contacts[0].point + collision.contacts[0].normal);
            CameraShake.Instance.ShakeCaller(0.3f, 0.1f);
        }

        // Destroy(gameObject);
        StartCoroutine(SafeDestroy());
    }
    private IEnumerator SafeDestroy()
    {
        yield return new WaitForSeconds(0.05f);

        if (PhotonView.Get(this).IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    public void BulletMove(float speed)
    {
        _rb.AddForce(transform.up * speed, ForceMode2D.Impulse);
        // Destroy(gameObject, 4f);
        StartCoroutine(DestroyAfterDelay(4f));
        
    }
    
    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (PhotonView.Get(this).IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    public void Attack()
    {
        // Player 스크립트 생기면 해당 플레이어를 받아와서 TakeDamage 메소드 호출
    }

    [PunRPC]
    public void BigBulletShot()
    {
        _bigBullet.transform.SetParent(null);
        _bigBullet.GetComponent<ParticleSystem>().Stop();
        StartCoroutine(DestroyBigBulletAfterDelay(_bigBullet, 1f));
    }
    
    /// <summary>
    /// 큰 탄알을 일정 시간 후에 파괴하는 코루틴
    /// </summary>
    /// <param name="bullet"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    private IEnumerator DestroyBigBulletAfterDelay(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        PhotonNetwork.Destroy(bullet);
    }

    [PunRPC]
    public void ExplosiveBulletShot()
    {
        PhotonNetwork.Instantiate(_explosiveBulletEffect.name, transform.position, Quaternion.identity);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
        }
        else
        {
            _networkPos = (Vector3)stream.ReceiveNext();
        }
    }
}

