using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class PlayerArmMove : MonoBehaviour
{
    [SerializeField] private SplineContainer _leftArmSpline;
    [SerializeField] private SplineContainer _rightArmSpline;
    [SerializeField] private Transform _gunTransform;
    [SerializeField] private Transform _gunAxis;

    private void Update()
    {
        LookAtMouse();
        //SetLeftArmPosition();
        SetRightArmPosition();
    }

    private void SetRightArmPosition()
    {
        if (_rightArmSpline == null || _gunTransform == null) return;
        Vector3 pos = _gunTransform.position - _rightArmSpline.transform.position;
        _rightArmSpline.Spline.SetKnot(2, new BezierKnot(pos));
    }

    private void SetLeftArmPosition()
    {
        if (_leftArmSpline == null || _gunTransform == null) return;

        _leftArmSpline.Spline.SetKnot(1, new BezierKnot(_gunTransform.position));
    }

    private void LookAtMouse()
    {
        if (_gunTransform == null || _gunAxis == null) return;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // 2D 게임이므로 z축은 0으로 설정

        Vector3 direction = (mousePosition - _gunAxis.transform.position).normalized;
        _gunAxis.transform.up = direction;
    }
}
