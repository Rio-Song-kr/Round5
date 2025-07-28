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
    public float Duration;


    void Awake()
    {
        _laserEffect = GetComponent<VisualEffect>();
        _laserEffect.enabled = false;
    }


    void Update()
    {
        if (Input.GetMouseButton(0)) // 마우스 왼쪽 버튼을 누르고 있는 동안 레이저를 그립니다.
        {
            if (_laserCoroutine == null) // 레이저 코루틴이 실행 중이지 않으면 시작합니다.
            {
                _laserCoroutine = StartCoroutine(LaserCoroutine());
            }
        }

        TestLookAtMouse();
    }

    private void LaserBeam()
    {
        CameraShake.Instance.ShakeCaller(0.15f, 0.02f); // 카메라 흔들기 효과
        _laserEffect.SetVector3("StartPos", transform.position); // 레이저 시작 위치 설정
        if (Physics2D.RaycastNonAlloc(transform.position, transform.right, _hits, 100f) > 0)
        {
            _laserEffect.SetVector3("EndPos", _hits[0].point); // 레이저가 충돌한 위치로 끝 위치 설정
            _laserEffect.SetVector3("HitPos", _hits[0].point); // 파편 이펙트용 충돌 위치 설정
        }
        else
        {
            _laserEffect.SetVector3("EndPos", transform.position + transform.right * 100); // 충돌이 없으면 기본 끝 위치 설정
            _laserEffect.SetVector3("HitPos", transform.position + transform.right * 100); // 파편 이펙트용 기본 위치 설정
        }
    }

    private IEnumerator LaserCoroutine()
    {
        _laserEffect.enabled = true;
        _laserEffect.SetFloat("Duration", Duration); // 레이저 지속 시간 설정
        float Timer = 0f;
        float particleTimer = 0f;


        while (Timer <= Duration)
        {
            Timer += Time.deltaTime;
            LaserBeam();
            if (particleTimer >= _particleDelay)
            {
                GameObject soot = Instantiate(_laserSoot, transform.position, Quaternion.identity); // 레이저 그을림 효과 생성
                soot.transform.position = _hits[0].point; // 그을림 효과 위치 업데이트
                soot.transform.SetParent(_hits[0].collider.transform);
            }
            yield return null;
        }

        _laserEffect.enabled = false; // 지속 시간이 끝나면 레이저를 숨깁니다.
        _laserCoroutine = null; // 코루틴 종료
    }

    /// <summary>
    /// 테스트 코드입니다.
    /// </summary>
    private void TestLookAtMouse()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0; // 2D 게임이므로 z축은 0으로 설정
        transform.right = (mousePos - transform.position).normalized; // 레이저 방향을 마우스 위치로 설정
    }
}
