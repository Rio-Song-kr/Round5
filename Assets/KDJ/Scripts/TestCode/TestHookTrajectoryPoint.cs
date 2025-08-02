using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.VFX;

public class TestHookTrajectoryPoint : MonoBehaviour
{
    [SerializeField] private GameObject _hookCrosshair;
    [SerializeField] private LineRenderer _hookLineRenderer;
    [SerializeField] private GameObject _hookHitEffect;
    [SerializeField] private GameObject _hookObject;

    private RaycastHit2D[] _hits = new RaycastHit2D[10];
    private LineRenderer _lineRenderer;
    private GameObject _hook;
    private bool _isRayHit;

    private void Awake()
    {
        // _hookEffect.enabled = false; // 초기에는 이펙트를 비활성화
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.SetPosition(0, transform.position); // 위치 초기화
        _lineRenderer.SetPosition(1, transform.position);
        _hook = Instantiate(_hookObject, transform.position, Quaternion.identity); // 훅 오브젝트 생성
        _hook.SetActive(false); // 훅 오브젝트는 초기에는 비활성
    }


    void Update()
    {
        TestLookAtMouse();

        if (Input.GetMouseButton(0)) // 마우스 왼쪽 버튼 클릭 시 레이저 발사
        {
            //HookTrajectory();
            DrawHookTrajectory(); // 궤적 그리기
        }
        else
        {
            //_hookEffect.enabled = false; // 이펙트 비활성화
            _hookCrosshair.SetActive(false); // 크로스헤어 비활성화
            _lineRenderer.SetPosition(0, _hits[0].point); // 끝점을 시작점으로 설정하여 궤적을 그리지 않음
        }

        if (Input.GetMouseButtonUp(0))
        {
            DrawHook();
        }
    }

    /// <summary>
    /// Hook의 예상 궤적을 그리는 메서드 입니다. LineRenderer를 사용하여 궤적을 그립니다.
    /// </summary>
    private void DrawHookTrajectory()
    {
        _lineRenderer.positionCount = 2; // 시작점과 끝점으로 구성된 궤적
        
        if (Physics2D.RaycastNonAlloc(transform.position, transform.up, _hits, 100f) > 0)
        {
            Vector2 pos = new Vector2(transform.position.x, transform.position.y) - _hits[0].point;
            pos.Normalize();
            _lineRenderer.SetPosition(0, _hits[0].point + pos); // 시작점 설정
            _lineRenderer.SetPosition(1, _hits[0].point); // 충돌 지점 설정
            _hookCrosshair.transform.position = _hits[0].point; // 크로스헤어 위치를 충돌 지점으로 설정
            _hookCrosshair.SetActive(true); // 크로스헤어 활성화
        }
        else
        {
            //_lineRenderer.SetPosition(1, transform.position + transform.up * 100); // 충돌이 없으면 기본 끝 위치 설정
            // 안닿았다면 안그리기
            _hookCrosshair.SetActive(false); // 크로스헤어 비활성화
            _lineRenderer.SetPosition(1, transform.position); // 끝점을 시작점으로 설정하여 궤적을 그리지 않음
        }
    }

    private void DrawHook()
    {
        _hookLineRenderer.SetPosition(0, transform.position); // 시작점 설정
        _hookLineRenderer.SetPosition(1, _hits[0].point); // 끝점 설정
        GameObject effect = Instantiate(_hookHitEffect, _hits[0].point, Quaternion.identity); // 충돌 이펙트 생성
        effect.transform.LookAt(_hits[0].point + _hits[0].normal); // 충돌 지점의 법선 방향으로 회전
        _hook.transform.position = _hits[0].point; // 훅 오브젝트 위치 설정
        _hook.SetActive(true); // 훅 오브젝트 활성화
        _hook.transform.up = (_hits[0].point - new Vector2(transform.position.x, transform.position.y)).normalized; // 훅 오브젝트의 방향을 충돌 지점으로 설정
    }

    /// <summary>
    /// 테스트용 코드입니다. 오브젝트가 마우스를 바라보도록 설정합니다.
    /// </summary>
    private void TestLookAtMouse()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0; // 2D 게임이므로 z축은 0으로 설정
        transform.up = (mousePos - transform.position).normalized; // 레이저 방향을 마우스 위치로 설정
    }
}
