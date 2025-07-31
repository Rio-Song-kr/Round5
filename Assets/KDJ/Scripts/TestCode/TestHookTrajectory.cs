using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class TestHookTrajectory : MonoBehaviour
{
    [SerializeField] private VisualEffect _hookEffect;
    [SerializeField] private GameObject _hookCrosshair;

    private RaycastHit2D[] _hits = new RaycastHit2D[10];
    private bool _isRayHit;

    private void Awake()
    {
        _hookEffect.enabled = false; // 초기에는 이펙트를 비활성화
    }


    void Update()
    {
        TestLookAtMouse();

        if (Input.GetMouseButton(0)) // 마우스 왼쪽 버튼 클릭 시 레이저 발사
        {
            HookTrajectory();
        }
        else
        {
            _hookEffect.enabled = false; // 이펙트 비활성화
            _hookCrosshair.SetActive(false); // 크로스헤어 비활성화
        }
    }


    /// <summary>
    /// Hook의 예상 궤적을 그리는 메서드입니다. 충돌 지점을 _hits 배열에 저장하고 길이를 구해 궤적을 그립니다.
    /// </summary>
    private void HookTrajectory()
    {
        _hookEffect.SetVector3("StartPos", transform.position); // 레이저 시작 위치 설정
        if (Physics2D.RaycastNonAlloc(transform.position, transform.up, _hits, 100f) > 0)
        {
            Debug.Log($"레이저가 {_hits[0].collider.name}에 충돌했습니다.");
            _hookEffect.enabled = true; // 이펙트를 활성화
            _hookEffect.SetVector3("EndPos", _hits[0].point); // 레이저가 충돌한 위치로 끝 위치 설정
            _hookCrosshair.transform.position = _hits[0].point; // 크로스헤어 위치를 충돌 지점으로 설정
            _hookCrosshair.SetActive(true); // 크로스헤어 활성화
            _isRayHit = true; // 레이저가 충돌했음을 표시
        }
        else
        {
            // _hookEffect.SetVector3("EndPos", transform.position + transform.up * 100); // 충돌이 없으면 기본 끝 위치 설정
            // 안닿으면 안쏜다
            _hookEffect.enabled = false; // 이펙트 비활성화
            _hookCrosshair.SetActive(false); // 크로스헤어 비활성화
        }
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
