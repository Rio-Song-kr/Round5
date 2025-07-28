using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.VFX;

public class Laser : MonoBehaviour
{
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
    }

    private void LaserBeam()
    {
        CameraShake.Instance.ShakeCaller(0.15f, 0.02f); // 카메라 흔들기 효과
        _laserEffect.SetVector3("StartPos", transform.position); // 레이저 시작 위치 설정
        if (Physics2D.RaycastNonAlloc(transform.position, transform.right, _hits, 100f) > 0)
        {
            _laserEffect.SetVector3("EndPos", _hits[0].point); // 레이저가 충돌한 위치로 끝 위치 설정
        }
        else
        {
            _laserEffect.SetVector3("EndPos", transform.position + transform.right * 100); // 충돌이 없으면 기본 끝 위치 설정
        }
    }

    private IEnumerator LaserCoroutine()
    {
        _laserEffect.enabled = true;
        _laserEffect.SetFloat("Duration", Duration); // 레이저 지속 시간 설정
        float Timer = 0f;

        while (Timer <= Duration)
        {
            Timer += Time.deltaTime;
            LaserBeam();
            yield return null; // 다음 프레임까지 대기
        }

        _laserEffect.enabled = false; // 지속 시간이 끝나면 레이저를 숨깁니다.
        _laserCoroutine = null; // 코루틴 종료
    }
}
