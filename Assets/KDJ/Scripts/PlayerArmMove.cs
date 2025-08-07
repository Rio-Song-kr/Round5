using System.Collections;
using UnityEngine;
using UnityEngine.Splines;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class PlayerArmMove : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private SplineContainer _leftArmSpline;
    [SerializeField] private SplineContainer _rightArmSpline;
    [SerializeField] private Transform _gunPos;
    [SerializeField] private Transform _gunAxis;
    private bool _isGunInRight => _gunPos.position.x > _gunAxis.position.x;
    private bool _isFlipped => transform.right.x < 0;
    private bool _prevFlipped;
    private bool _prevGunInRight;

    // 동기화 변수
    private Vector3 _networkGunPos;
    private Vector3 _networkGunAxisUp;
    
    private void Update()
    {

        // Debug.Log("플립 상태:" + _isFlipped + ", 마우스 위치 오른쪽:" + _isGunInRight);

        if (PhotonNetwork.OfflineMode){
            LookAtMouse();
            _gunAxis.up = Vector3.Lerp(_gunAxis.up, _networkGunAxisUp, Time.deltaTime * 10f);
            _gunPos.position = Vector3.Lerp(_gunPos.position, _networkGunPos, Time.deltaTime * 10f);
        }
        else
        {
            if (photonView.IsMine)
            {
                LookAtMouse();
            }
            else
            {
                _gunAxis.up = Vector3.Lerp(_gunAxis.up, _networkGunAxisUp, Time.deltaTime * 10f);
                _gunPos.position = Vector3.Lerp(_gunPos.position, _networkGunPos, Time.deltaTime * 10f);
            }
        }

        if (_isGunInRight)
        {
            if (_isFlipped)
                SetLeftArmPosition();
            else
                SetRightArmPosition();

        }
        else
        {
            if (_isFlipped)
                SetRightArmPosition();
            else
                SetLeftArmPosition();
        }
    }

    void LateUpdate()
    {
        CheckMousePos();
        FlipCheck();
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
        float flipMultiplier = _isFlipped ? -1 : 1; // 플립 상태에 따라 방향을 조정
        if (_isFlipped) pos.x *= -1; // 플립 상태일 때 x위치를 반전
        Vector3 middleFinalPos = Vector3.Lerp(middlePos, middlePos + new Vector3(-dir.y, dir.x, 0) * -0.1f * flipMultiplier, 1 - dot);
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
        float flipMultiplier = _isFlipped ? -1 : 1; // 플립 상태에 따라 방향을 조정
        if (_isFlipped) pos.x *= -1; // 플립 상태일 때 x위치를 반전
        Vector3 middleFinalPos = Vector3.Lerp(middlePos, middlePos + new Vector3(-dir.y, dir.x, 0) * 0.1f * flipMultiplier, 1 - dot);
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
        if (_isFlipped)
        {
            // 반전 되었을때는 서로 달라야 정상
            if (_prevGunInRight != _isGunInRight)
            {
                _prevGunInRight = _isGunInRight;
                return;
            }
        }
        else
        {
            if (_prevGunInRight == _isGunInRight)
            {
                _prevGunInRight = _isGunInRight;
                return;
            }
        }

        if (_isGunInRight)
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

        _prevGunInRight = _isGunInRight;
    }

    /// <summary>
    /// 플립 상태를 확인합니다.
    /// </summary>
    private void FlipCheck()
    {
        if (_prevFlipped == _isFlipped)
        {
            _prevFlipped = _isFlipped;
            return;
        }

        if (_isGunInRight)
        {
            if (_isFlipped)
            {
                _gunAxis.localRotation = Quaternion.Euler(0, 180, 0); // 왼팔 회전
                _rightArmSpline.Spline.SetKnot(1, new BezierKnot(new Vector3(0.12f, -0.1f, 0))); // 오른팔 위치 초기화
                _rightArmSpline.Spline.SetKnot(2, new BezierKnot(new Vector3(0.3f, 0, 0))); // 오른팔 위치 초기화
            }
            else
            {
                _gunAxis.localRotation = Quaternion.Euler(0, 0, 0); // 오른팔 회전
                _leftArmSpline.Spline.SetKnot(1, new BezierKnot(new Vector3(-0.12f, -0.1f, 0))); // 왼팔 위치 초기화
                _leftArmSpline.Spline.SetKnot(2, new BezierKnot(new Vector3(-0.3f, 0, 0))); // 왼팔 위치 초기화
            }
        }
        else
        {
            if (_isFlipped)
            {
                _gunAxis.localRotation = Quaternion.Euler(0, 0, 0); // 오른팔 회전
                _leftArmSpline.Spline.SetKnot(1, new BezierKnot(new Vector3(-0.12f, -0.1f, 0))); // 왼팔 위치 초기화
                _leftArmSpline.Spline.SetKnot(2, new BezierKnot(new Vector3(-0.3f, 0, 0))); // 왼팔 위치 초기화
            }
            else
            {
                _gunAxis.localRotation = Quaternion.Euler(0, 180, 0); // 왼팔 회전
                _rightArmSpline.Spline.SetKnot(1, new BezierKnot(new Vector3(0.12f, -0.1f, 0))); // 오른팔 위치 초기화
                _rightArmSpline.Spline.SetKnot(2, new BezierKnot(new Vector3(0.3f, 0, 0))); // 오른팔 위치 초기화
            }
        }

        _prevFlipped = _isFlipped;
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 로컬 플레이어가 데이터를 보냄
            stream.SendNext(_gunPos.position);
            stream.SendNext(_gunAxis.up);
        }
        else
        {
            // 리모트 플레이어가 데이터를 받음
            _networkGunPos = (Vector3)stream.ReceiveNext();
            _networkGunAxisUp = (Vector3)stream.ReceiveNext();
        }
    }
}
