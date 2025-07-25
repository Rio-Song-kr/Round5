using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
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

        // TryDestroy();
        // PhotonNetwork.Destroy(gameObject);
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

    /// <summary>
    /// 다른 클라이언트가 만든 오브젝트를 파괴하는 PunRPC 메소드(권한오류 나서 만듬)
    /// </summary>
    // [PunRPC]
    // void TryDestroy()
    // {
    //     PhotonView view = GetComponent<PhotonView>();
    //
    //     if (view.IsMine || PhotonNetwork.IsMasterClient)
    //     {
    //         // 내가 소유자이거나 마스터면 직접 파괴
    //         PhotonNetwork.Destroy(gameObject);
    //     }
    //     else
    //     {
    //         // 마스터에게 Destroy 요청
    //         view.RPC(nameof(ObjectDestroy), RpcTarget.MasterClient);
    //     }
    // }
    //
    // [PunRPC]
    // void ObjectDestroy()
    // {
    //     PhotonNetwork.Destroy(gameObject);
    // }

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
        // TryDestroy();
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
        // TryDestroy();
    }

    [PunRPC]
    public void ExplosiveBulletShot()
    {
        PhotonNetwork.Instantiate(_explosiveBulletEffect.name, transform.position, Quaternion.identity);
    }

}

