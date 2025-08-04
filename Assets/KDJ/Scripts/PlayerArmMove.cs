using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class PlayerArmMove : MonoBehaviour
{
    [SerializeField] private SplineContainer _leftArmSpline;
    [SerializeField] private SplineContainer _rightArmSpline;
    [SerializeField] private Transform _gunPos;
    [SerializeField] private Transform _gunAxis;
    private bool _isMouseInRight => _gunPos.position.x > _gunAxis.position.x;
    private bool _prevMouseInRight;

    private void Update()
    {
        LookAtMouse();
        if (_isMouseInRight) SetRightArmPosition();
        else SetLeftArmPosition();
    }

    void LateUpdate()
    {
        CheckMousePos();
    }
    
    

    /// <summary>
    /// 오른팔의 위치를 설정합니다.
    /// </summary>
    private void SetRightArmPosition()
    {
        if (_rightArmSpline == null || _gunPos == null) return;
        Vector3 pos = _gunPos.position - _rightArmSpline.transform.position;
        Vector3 middlePos = Vector3.Lerp(_rightArmSpline.transform.position, _gunPos.position, 0.5f);
        // 먼저 어깨에서 총으로 향하는 방향 벡터를 계산
        Vector3 dir = (_gunPos.position - _rightArmSpline.transform.position).normalized;
        // 방향 벡터가 얼마나 수직에 가까운지 계산 후 절대값을 사용
        float dot = Mathf.Abs(Vector3.Dot(Vector3.up, dir));
        // 중간 위치를 계산하고, 방향 벡터에 수직인 벡터를 사용하여 중간 위치를 조정
        Vector3 middleFinalPos = Vector3.Lerp(middlePos, middlePos + new Vector3(-dir.y, dir.x, 0) * -0.1f, 1 - dot);
        Vector3 localMiddlePos = _rightArmSpline.transform.InverseTransformPoint(middleFinalPos); // 중간 위치를 Spline 기준으로 변환
        _rightArmSpline.Spline.SetKnot(0, new BezierKnot(Vector3.zero));
        _rightArmSpline.Spline.SetKnot(1, new BezierKnot(localMiddlePos));
        _rightArmSpline.Spline.SetKnot(2, new BezierKnot(pos));
    }

    /// <summary>
    /// 왼팔의 위치를 설정합니다.
    /// </summary>
    private void SetLeftArmPosition()
    {
        if (_leftArmSpline == null || _gunPos == null) return;
        Vector3 pos = _gunPos.position - _leftArmSpline.transform.position;
        Vector3 middlePos = Vector3.Lerp(_leftArmSpline.transform.position, _gunPos.position, 0.5f);
        Vector3 dir = (_gunPos.position - _leftArmSpline.transform.position).normalized;
        float dot = Mathf.Abs(Vector3.Dot(Vector3.up, dir));
        Vector3 middleFinalPos = Vector3.Lerp(middlePos, middlePos + new Vector3(-dir.y, dir.x, 0) * 0.1f, 1 - dot);
        Vector3 localMiddlePos = _leftArmSpline.transform.InverseTransformPoint(middleFinalPos); // 중간 위치를 Spline 기준으로 변환
        _leftArmSpline.Spline.SetKnot(0, new BezierKnot(Vector3.zero)); // 시작점은 (0,0,0)
        _leftArmSpline.Spline.SetKnot(1, new BezierKnot(localMiddlePos));
        _leftArmSpline.Spline.SetKnot(2, new BezierKnot(pos));
    }

    /// <summary>
    /// 마우스 위치에 따라 팔의 회전을 조정합니다.
    /// </summary>
    private void CheckMousePos()
    {
        if (_prevMouseInRight == _isMouseInRight)
        {
            _prevMouseInRight = _isMouseInRight;
            return;
        }

        if (_isMouseInRight)
        {
            _gunPos.localRotation = Quaternion.Euler(0, 0, 0); // 오른팔 회전
            _leftArmSpline.Spline.SetKnot(1, new BezierKnot(new Vector3(-0.12f, -0.1f, 0))); // 왼팔 위치 초기화
            _leftArmSpline.Spline.SetKnot(2, new BezierKnot(new Vector3(-0.3f, 0, 0))); // 왼팔 위치 초기화
        }
        else
        {
            _gunPos.localRotation = Quaternion.Euler(0, 180, 0); // 왼팔 회전
            _rightArmSpline.Spline.SetKnot(1, new BezierKnot(new Vector3(0.12f, -0.1f, 0))); // 오른팔 위치 초기화
            _rightArmSpline.Spline.SetKnot(2, new BezierKnot(new Vector3(0.3f, 0, 0))); // 오른팔 위치 초기화
        }

        _prevMouseInRight = _isMouseInRight;
    }

    /// <summary>
    /// 마우스를 바라보도록 팔을 회전시킵니다.
    /// </summary>
    private void LookAtMouse()
    {
        if (_gunPos == null || _gunAxis == null) return;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // 2D 게임이므로 z축은 0으로 설정

        Vector3 direction = (mousePosition - _gunAxis.transform.position).normalized;
        _gunAxis.transform.up = direction;
    }
}
