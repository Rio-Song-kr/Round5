using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    private LineRenderer _lineRenderer;

    void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.enabled = false;
    }


    void Update()
    {
        if (Input.GetMouseButton(0)) // 마우스 왼쪽 버튼을 누르고 있는 동안 레이저를 그립니다.
        {
            LaserBeam();
        }
        else
        {
            _lineRenderer.enabled = false; // 마우스 버튼을 떼면 레이저를 숨깁니다.
        }
    }

    private void LaserBeam()
    {
        CameraShake.Instance.ShakeCaller(0.15f, 0.02f); // 카메라 흔들기 효과
        _lineRenderer.enabled = true;
        _lineRenderer.SetPosition(0, transform.position);
        // 끝 지점은 마우스의 위치로 레이캐스트를 쏜 뒤 무언가에 닿을 경우 해당위치로, 닿지 않았다면 최대 사거리인 100으로
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, mousePosition - (Vector2)transform.position, 100f);
        if (hit.collider != null)
        {
            _lineRenderer.SetPosition(1, hit.point);
        }
        else
        {
            _lineRenderer.SetPosition(1, (Vector2)transform.position + (mousePosition - (Vector2)transform.position).normalized * 100f);
        }
    }
}
