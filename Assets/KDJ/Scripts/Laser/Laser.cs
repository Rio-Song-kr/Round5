using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.VFX;

public class Laser : MonoBehaviourPun
{
    [SerializeField] private GameObject _laserSoot; // 레이저 그을림 효과
    [SerializeField] private float _particleDelay;
    [SerializeField] private LayerMask _layerMask;

    private VisualEffect _laserEffect;
    private RaycastHit2D[] _hits = new RaycastHit2D[10];
    private Coroutine _laserCoroutine;
    private bool _isLaserHit;
    private PoolManager _poolManager;
    [Header("레이저 세팅")]
    public float Duration;
    public float LaserScale = 1f;

    private bool _canShoot;
    
    
    // 데미지
    private float _baseDamage = 6f;
    private float _damageMultiplier = 0.3f;

    public bool CanShoot => _laserCoroutine == null; // 레이저가 활성화되어 있지 않으면 true

    private void Awake()
    {
        _laserEffect = GetComponent<VisualEffect>();
        _laserEffect.enabled = false;
        transform.localScale = Vector3.one; // 레이저 오브젝트의 스케일을 초기화
        
        _poolManager = FindFirstObjectByType<PoolManager>();
        _poolManager.InitializePool("LaserSoot", _laserSoot, 100, 200);
    }

    // private void Start()
    // {
    // // _laserSootPool = new LaserSootPool<LaserSoot>();
    // // _laserSootPool.SetPool(_laserSoot, 10, transform); // 레이저 그을림 효과 풀 초기화
    // _poolManager = FindFirstObjectByType<PoolManager>();
    // _poolManager.InitializePool("LaserSoot", _laserSoot, 200, 300);
    // }

    // 레이저 발사시 중복 발사됨
    // private void Update()
    // {
    // if (!photonView.IsMine) return;
    //
    // if (Input.GetMouseButtonDown(0)) // 마우스 왼쪽 버튼 클릭 시 레이저 발사
    // {
    //     photonView.RPC(nameof(Shoot), RpcTarget.All);
    // }
    // }

    void Update()
    {
        Debug.Log("canShoot: " + CanShoot);
    }

    public void ShootLaser()
    {
        Debug.Log("레이저 발사 요청");
        if (!photonView.IsMine) return;
        // photonView.RPC(nameof(Shoot), RpcTarget.All);
        _canShoot = true;
        Debug.Log("레이저 발사 요청 2");
        Debug.Log($"코루틴 상태:{_laserCoroutine}");

        if (CanShoot) // 레이저 코루틴이 실행 중이지 않으면 시작합니다.
        {
            Debug.Log("레이저 코루틴 시작");
            _laserCoroutine = StartCoroutine(LaserCoroutine());
        }
    }

    [PunRPC]
    private void Shoot()
    {
        _canShoot = true;

        if (CanShoot) // 레이저 코루틴이 실행 중이지 않으면 시작합니다.
        {
            
            _laserCoroutine = StartCoroutine(LaserCoroutine());
        }
    }

    /// <summary>
    /// 레이저를 그리는 메서드입니다. 충돌 지점을 _hits 배열에 저장하고 레이저 이펙트를 설정합니다.
    /// </summary>
    private void LaserBeam()
    {
        Debug.Log("레이저 빔 발사");
        CameraShake.Instance.ShakeCaller(0.15f, 0.02f); // 카메라 흔들기 효과
        _laserEffect.SetVector3("StartPos", transform.position); // 레이저 시작 위치 설정
        // if (Physics2D.RaycastNonAlloc(transform.position, transform.up, _hits, 100f, ~_layerMask) > 0)
        if (Physics2D.RaycastNonAlloc(transform.position, transform.up, _hits, 100f) > 0)
        {
            Debug.Log($"레이저가 {_hits[0].collider.name}에 충돌했습니다.");
            _laserEffect.SetVector3("EndPos", _hits[0].point); // 레이저가 충돌한 위치로 끝 위치 설정
            _laserEffect.SetVector3("HitPos", _hits[0].point); // 파편 이펙트용 충돌 위치 설정
            _isLaserHit = true; // 레이저가 충돌했음을 표시
        }
        else
        {
            Debug.Log("레이저가 충돌하지 않았습니다.");
            _laserEffect.SetVector3("EndPos", transform.position + transform.up * 100); // 충돌이 없으면 기본 끝 위치 설정
            _laserEffect.SetVector3("HitPos", transform.position + transform.up * 100); // 파편 이펙트용 기본 위치 설정
            _isLaserHit = false; // 레이저가 충돌하지 않았음을 표시
        }
    }

    /// <summary>
    /// 레이저 코루틴입니다. 레이저가 활성화되고 지속 시간 동안 레이저를 그립니다.
    /// 레이저가 충돌한 경우 그을림 효과를 생성합니다.
    /// </summary>
    private IEnumerator LaserCoroutine()
    {
        _isLaserHit = false;
        _laserEffect.enabled = true;
        _laserEffect.SetFloat("ScaleMultiply", LaserScale); // 레이저 스케일 설정
        _laserEffect.SetFloat("Duration", Duration); // 레이저 지속 시간 설정
        var scale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
        _laserEffect.SetVector3("ParentScale", scale); // 부모 오브젝트의 스케일 설정
        float Timer = 0f;
        float particleTimer = 0f;


        while (Timer <= Duration)
        {
            Timer += Time.deltaTime;
            particleTimer += Time.deltaTime;
            LaserBeam();
            Debug.Log($"isLaserHit: {_isLaserHit}");
            if (particleTimer >= _particleDelay)
            {
                if (_isLaserHit)
                {
                    // LaserSoot soot = _laserSootPool.Pool.Get();
                    // var soot = _laserSootPool.Instantiate(
                    //         _laserSoot.name,
                    //         transform.position,
                    //         transform.rotation)
                    //     .GetComponent<LaserSoot>();
                    // var soot = PhotonNetwork.Instantiate(
                            // "LaserSoot",
                            // transform.position,
                            // transform.rotation)
                        // .GetComponent<LaserSoot>();
                    var soot = PhotonNetwork.Instantiate(
                            "LaserSoot",
                            _hits[0].point,
                            transform.rotation)
                        .GetComponent<LaserSoot>();
                    if (soot != null)
                    {
                        soot.SetPool();
                        // soot.transform.position = _hits[0].point;
                        soot.gameObject.transform.SetParent(_hits[0].transform);
                        var rb = _hits[0].transform.GetComponent<Rigidbody2D>();
                        if (rb != null)
                        {
                            rb.AddForce((_hits[0].point - new Vector2(transform.position.x, transform.position.y)).normalized
                                        * 0.1f, ForceMode2D.Impulse); // 충돌한 오브젝트에 넉백 적용
                        }
                        
                        // 틱 데미지 적용 (이펙트와 동시에)
                        if (photonView.IsMine)
                        {
                            var targetView = _hits[0].collider.GetComponent<PhotonView>();
                            if (targetView != null)
                            {
                                float damage = _baseDamage * _damageMultiplier; // 6 * (1 - 0.7) 데미지 6에서 -70% = 6 * 0.3 = 1.8
                                // TODO: 적한테 데미지 적용
                                // targetView.RPC("TakeDamage", RpcTarget.All, damage);
                            }
                        }
    
                        particleTimer = 0f;
                    }
                }
            }
            yield return null;
        }

        _isLaserHit = false;
        _laserEffect.enabled = false;
        _laserCoroutine = null;
    }

    /// <summary>
    /// 테스트 코드입니다.
    /// </summary>
    private void TestLookAtMouse()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0; // 2D 게임이므로 z축은 0으로 설정
        transform.up = (mousePos - transform.position).normalized; // 레이저 방향을 마우스 위치로 설정
    }
}