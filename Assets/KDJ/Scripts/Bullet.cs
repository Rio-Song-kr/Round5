using System.Collections;
using Photon.Pun;
using UnityEngine;

public class Bullet : MonoBehaviour, IPunObservable
{
   [SerializeField] public float Speed;                     // 총알 속도
    [SerializeField] public float Damage;                    // 데미지 (현재 미사용)
    [SerializeField] private Rigidbody2D _rb;                // 물리 기반 이동
    [SerializeField] private GameObject _bigBullet;          // 큰 총알 이펙트
    [SerializeField] private GameObject _explosiveBullet;    // 폭발 총알 이펙트
    [SerializeField] private GameObject _explosiveBulletEffect; // 폭발 이펙트 프리팹
    [SerializeField] private GameObject _hitEffect;          // 일반 충돌 이펙트
    [SerializeField] private bool _isBigBullet;              // 큰 총알 여부
    [SerializeField] private bool _isExplosiveBullet;        // 폭발 총알 여부
    
    // 250726 추가
    private Vector3 _networkPos;
    
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

    void Start()
    {
        BulletMove(Speed);// 발사 시 바로 힘을 가해 날림
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 내가 소유한 총알이 아닐 경우 충돌처리 하지 않음
        if (!PhotonView.Get(this).IsMine) return; 
        
        // 총알 유형에 따라 처리 
        if (_isBigBullet)
            BigBulletShot();
        if (_isExplosiveBullet)
            ExplosiveBulletShot();
        else
        {
            // 기본 이펙트 생성
            GameObject effect = PhotonNetwork.Instantiate(_hitEffect.name, transform.position, Quaternion.identity);
            // GameObject effect = Instantiate(_hitEffect.name, transform.position, Quaternion.identity);
            effect.transform.LookAt(collision.contacts[0].point + collision.contacts[0].normal);
            CameraShake.Instance.ShakeCaller(0.3f, 0.1f);
        }

        // 약간의 시간 지연 후 안전하게 파괴
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
        // 내 클라이언트일 경우 위치/회전 정보 전송
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
        }
        // 다른 클라이언트일 경우 위치/회전 정보 수신
        else
        {
            _networkPos = (Vector3)stream.ReceiveNext();
        }
    }
}

