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
    private float _baseDamage = 10f;
    private float _damageMultiplier = 0.25f;

    public bool CanShoot => _laserCoroutine == null; // 레이저가 활성화되어 있지 않으면 true

    private void Awake()
    {
        _laserEffect = GetComponent<VisualEffect>();
        _laserEffect.enabled = false;
        transform.localScale = Vector3.one; // 레이저 오브젝트의 스케일을 초기화

        if (PhotonNetwork.OfflineMode == false)
        {
            _poolManager = FindFirstObjectByType<PoolManager>();
            _poolManager.InitializePool("LaserSoot", _laserSoot, 100, 200);
        }
    }

    private void OnDisable()
    {
        _isLaserHit = false;
        _laserEffect.enabled = false;
        if (_laserCoroutine != null)
            StopCoroutine(_laserCoroutine);
        _laserCoroutine = null;
    }

    public void ShootLaser()
    {
        if (PhotonNetwork.OfflineMode == true)
        {
            Shoot();
            return;
        }

        if (!photonView.IsMine) return;

        if (CanShoot) // 레이저 코루틴이 실행 중이지 않으면 시작합니다.
        {
            _laserCoroutine = StartCoroutine(LaserCoroutine());
        }
    }

    private void Shoot()
    {
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
        CameraShake.Instance.ShakeCaller(0.15f, 0.02f); // 카메라 흔들기 효과
        _laserEffect.SetVector3("StartPos", transform.position); // 레이저 시작 위치 설정

        if (Physics2D.RaycastNonAlloc(transform.position, transform.up, _hits, 100f, ~_layerMask) > 0)
        {
            // Debug.Log($"레이저가 {_hits[0].collider.name}에 충돌했습니다.");
            _laserEffect.SetVector3("EndPos", _hits[0].point); // 레이저가 충돌한 위치로 끝 위치 설정
            _laserEffect.SetVector3("HitPos", _hits[0].point); // 파편 이펙트용 충돌 위치 설정
            _isLaserHit = true; // 레이저가 충돌했음을 표시
        }
        else
        {
            // Debug.Log("레이저가 충돌하지 않았습니다.");
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
        float laserTick = 0f;

        while (Timer <= Duration)
        {
            Timer += Time.deltaTime;
            particleTimer += Time.deltaTime;

            LaserBeam();
            // Debug.Log($"isLaserHit: {_isLaserHit}");
            if (particleTimer >= _particleDelay)
            {
                if (_isLaserHit)
                {
                    LaserSoot soot = null;
                    if (PhotonNetwork.OfflineMode == true)
                    {
                        soot = Instantiate(
                                _laserSoot,
                                _hits[0].point,
                                transform.rotation)
                            .GetComponent<LaserSoot>();
                    }
                    else
                    {
                        soot = PhotonNetwork.Instantiate(
                                "LaserSoot",
                                _hits[0].point,
                                transform.rotation)
                            .GetComponent<LaserSoot>();
                    }

                    if (soot != null)
                    {
                        if (PhotonNetwork.OfflineMode == false)
                        {
                            soot.SetPool();
                        }
                        // soot.transform.position = _hits[0].point;
                        soot.gameObject.transform.SetParent(_hits[0].transform);
                        var rb = _hits[0].transform.GetComponent<Rigidbody2D>();
                        if (rb != null)
                        {
                            rb.AddForce((_hits[0].point - new Vector2(transform.position.x, transform.position.y)).normalized
                                        * 0.1f, ForceMode2D.Impulse); // 충돌한 오브젝트에 넉백 적용
                        }

                        particleTimer = 0f;
                    }
                }
            }

            // 데미지 처리 :: S
            if (_hits[0].collider.gameObject.layer == 8)
            {
                laserTick += Time.deltaTime;
                
                 // 0.5초마다 틱 데미지 적용
                if (laserTick >= 0.495f)
                {
                    var damagable = _hits[0].collider.GetComponent<IDamagable>();
                    if (damagable != null)
                    {
                        float damage = _baseDamage * _damageMultiplier;
                        damagable.TakeDamage(damage, _hits[0].point, _hits[0].normal); // IDamagable 인터페이스를 통해 데미지 적용
                    }
                    laserTick = 0f;
                }
            }
            // 플레이어 레이어가 아닐경우
            else
            {
                laserTick = 0f; 
            }
            // 데미지 처리 :: E


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