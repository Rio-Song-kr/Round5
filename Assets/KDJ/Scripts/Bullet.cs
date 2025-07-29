using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;

public class Bullet : MonoBehaviourPun
    // , IPunObservable
{
    // 무기에서 조절
   // [SerializeField] public float Speed;                     // 총알 속도
    [SerializeField] public float Damage;                    // 데미지 (현재 미사용)
    [SerializeField] private Rigidbody2D _rb;                // 물리 기반 이동
    [SerializeField] private GameObject _bigBullet;          // 큰 총알 이펙트
    [SerializeField] private GameObject _explosiveBullet;    // 폭발 총알 이펙트
    [SerializeField] private GameObject _explosiveBulletEffect; // 폭발 이펙트 프리팹
    [SerializeField] private GameObject _hitEffect;          // 일반 충돌 이펙트
    [SerializeField] private bool _isBigBullet;              // 큰 총알 여부
    [SerializeField] private bool _isExplosiveBullet;        // 폭발 총알 여부
    
    // 250726 추가
    private Vector3 _networkPosition;
    private Quaternion _networkRotation;
    // private BaseWeapon _baseWeapon; // BaseWeapon 스크립트 참조
    
    private void Awake()
    {
        //250726 추가
        // 탄환 유형에 따라 오브젝트 켜기/끄기
        _bigBullet.SetActive(_isBigBullet);
        _explosiveBullet.SetActive(_isExplosiveBullet);
        
        // if (_isBigBullet)
        // {
        //     _bigBullet.SetActive(true);
        // }
        // else
        // {
        //     _bigBullet.SetActive(false);
        // }
        //
        // if (_isExplosiveBullet)
        // {
        //     _explosiveBullet.SetActive(true);
        // }
        // else
        // {
        //     _explosiveBullet.SetActive(false);
        // }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!photonView.IsMine) return;

        // 총알 유형에 따라 처리 
        if (_isBigBullet)
            BigBulletShot();
        if (_isExplosiveBullet)
            ExplosiveBulletShot();
        else
        {
            DefaultShot(collision);
        }

        PlayerHitTest playerHitTest = collision.collider.GetComponent<PlayerHitTest>();
        if (playerHitTest != null)
        {
            Attack(playerHitTest); // 플레이어에게 공격
        }

        // 약간의 시간 지연 후 안전하게 파괴
            // Destroy(gameObject);

            StartCoroutine(SafeDestroy());
    }

    // 사용안함 무기에서 바로 호출
    // private void Start()
    // {
    //     BulletMove(_baseWeapon.bulletSpeed);
    // }
    
    
    // 탄알이 2개로 날아가는 문제가 있어 추가
    // private IEnumerator Start()
    // {
    //     if (!photonView.IsMine)
    //     {
    //         // 다른 플레이어가 만든 총알이면, 파괴하지 말고 비활성화만
    //         gameObject.SetActive(false);
    //     }
    //     // 4초 뒤 파괴
    //     yield return new WaitForSeconds(4f);
    // }
    
    private IEnumerator SafeDestroy()
    {
        yield return new WaitForSeconds(0.05f);

        if (PhotonView.Get(this).IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
    
    [PunRPC]
    public void DefaultShot(Collision2D collision)
    {
        // 기본 이펙트 생성
        GameObject effect = PhotonNetwork.Instantiate("Bullets/Fragment", transform.position, Quaternion.identity);
        // GameObject effect = Instantiate(_hitEffect.name, transform.position, Quaternion.identity);
        effect.transform.LookAt(collision.contacts[0].point + collision.contacts[0].normal);
        CameraShake.Instance.ShakeCaller(0.3f, 0.1f);
    }
    
    public void BulletMove(float speed)
    {
        // if (!photonView.IsMine) return;
        _rb.AddForce(transform.up * speed, ForceMode2D.Impulse);
        // _rb.velocity = transform.up * speed;
        //     // Destroy(gameObject, 4f);
        StartCoroutine(DestroyAfterDelay(4f));
    }

    
    public IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (PhotonView.Get(this).IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
    
    [PunRPC]
    public void InitBullet(float speed)
    {
        _rb.AddForce(transform.up * speed, ForceMode2D.Impulse);
        StartCoroutine(DestroyAfterDelay(4f));
    }

    /// <summary>
    /// 테스트용으로 매개변수를 테스트 플레이어로 설정
    /// 나중에 변경해야됨
    /// </summary>
    public void Attack(PlayerHitTest playerHitTest)
    {
        playerHitTest.TakeDamage(1);
    }


    // private void Update()
    // {
    //     if (!photonView.IsMine)
    //     {
    //         transform.position = Vector3.Lerp(transform.position, _networkPosition, Time.deltaTime * 3f);
    //         transform.rotation = Quaternion.Slerp(transform.rotation, _networkRotation, Time.deltaTime * 150f);
    //     }
    // }
    
    

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
        PhotonNetwork.Instantiate("Bullets/Explosive", transform.position, Quaternion.identity);
    }

    // public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    // {
    //     if (stream.IsWriting)
    //     {
    //         stream.SendNext(transform.position);
    //         stream.SendNext(transform.rotation);
    //     }
    //     else
    //     {
    //         _networkPosition = (Vector3)stream.ReceiveNext();
    //         _networkRotation = (Quaternion)stream.ReceiveNext();
    //     }
    // }

}

