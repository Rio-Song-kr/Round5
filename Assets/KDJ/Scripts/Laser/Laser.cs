using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.VFX;

public class Laser : MonoBehaviour
{
    [SerializeField] private GameObject _laserSoot; // 레이저 그을림 효과
    [SerializeField] private float _particleDelay;
    private VisualEffect _laserEffect;
    private RaycastHit2D[] _hits = new RaycastHit2D[10];
    private Coroutine _laserCoroutine;
    private bool _isLaserHit;
    private LaserSootPool<LaserSoot> _laserSootPool;
    public LaserSootPool<LaserSoot> LaserSootPool => _laserSootPool;
    public float Duration;
    public bool CanShoot => _laserCoroutine == null; // 레이저가 활성화되어 있지 않으면 true

    void Awake()
    {
        _laserSootPool = new LaserSootPool<LaserSoot>();
        _laserSootPool.SetPool(_laserSoot, 10, this.transform); // 레이저 그을림 효과 풀 초기화
        _laserEffect = GetComponent<VisualEffect>();
        _laserEffect.enabled = false;
    }

    void Update()
    {
        if (Input.GetMouseButton(0)) // 마우스 왼쪽 버튼을 누르고 있는 동안 레이저를 그립니다.
        {
            if (CanShoot) // 레이저 코루틴이 실행 중이지 않으면 시작합니다.
            {
                _laserCoroutine = StartCoroutine(LaserCoroutine());
            }
        }

        // TestLookAtMouse();
    }

    /// <summary>
    /// 레이저를 그리는 메서드입니다. 충돌 지점을 _hits 배열에 저장하고 레이저 이펙트를 설정합니다.
    /// </summary>
    private void LaserBeam()
    {
        CameraShake.Instance.ShakeCaller(0.15f, 0.02f); // 카메라 흔들기 효과
        _laserEffect.SetVector3("StartPos", transform.position); // 레이저 시작 위치 설정
        if (Physics2D.RaycastNonAlloc(transform.position, transform.right, _hits, 100f) > 0)
        {
            Debug.Log($"레이저가 {_hits[0].collider.name}에 충돌했습니다.");
            _laserEffect.SetVector3("EndPos", _hits[0].point); // 레이저가 충돌한 위치로 끝 위치 설정
            _laserEffect.SetVector3("HitPos", _hits[0].point); // 파편 이펙트용 충돌 위치 설정
            _isLaserHit = true; // 레이저가 충돌했음을 표시
        }
        else
        {
            _laserEffect.SetVector3("EndPos", transform.position + transform.right * 100); // 충돌이 없으면 기본 끝 위치 설정
            _laserEffect.SetVector3("HitPos", transform.position + transform.right * 100); // 파편 이펙트용 기본 위치 설정
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
        _laserEffect.SetFloat("Duration", Duration); // 레이저 지속 시간 설정
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
                    LaserSoot soot = _laserSootPool.Pool.Get();
                    if (soot != null)
                    {
                        soot.SetPool(_laserSootPool, transform);
                        soot.transform.position = _hits[0].point; 
                        soot.gameObject.transform.SetParent(_hits[0].transform);
                        Rigidbody2D rb = _hits[0].transform.GetComponent<Rigidbody2D>();
                        if (rb != null)
                        {
                            rb.AddForce(-_hits[0].normal * 0.1f, ForceMode2D.Impulse); // 충돌한 오브젝트에 반발력 적용
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
    // private void TestLookAtMouse()
    // {
    //     Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //     mousePos.z = 0; // 2D 게임이므로 z축은 0으로 설정
    //     transform.right = (mousePos - transform.position).normalized; // 레이저 방향을 마우스 위치로 설정
    // }
}
