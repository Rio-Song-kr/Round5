using System.Collections;
using UnityEngine;
using UnityEngine.Splines;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic;

public class PlayerArmMove : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private SplineContainer _leftArmSpline;
    [SerializeField] private SplineContainer _rightArmSpline;
    [SerializeField] private Transform _gunPos;
    [SerializeField] private Transform _gunAxis;
    [SerializeField] private Transform _ballPos;

    private Vector3 _prevLArmEndPos;
    private Vector3 _prevRArmEndPos;
    private bool _isGunInRight => _gunPos.position.x > _gunAxis.position.x;
    private bool _isFlipped => transform.right.x < 0;
    private bool _prevFlipped;
    private bool _prevGunInRight;
    private float _lag;

    // 동기화 변수
    private Vector3 _networkGunPos;
    private Vector3 _networkGunAxisUp;
    private Vector3 _networkBallPos;
    // <<<<<<< HEAD

    // =======
    private Vector3 _LArmSpeed;
    private Vector3 _RArmSpeed;

    private Vector3 _networkLArmStartPos;
    private Vector3 _networkRArmStartPos;
    private Vector3 _networkLArmMiddlePos;
    private Vector3 _networkRArmMiddlePos;
    private Vector3 _networkLArmEndPos;
    private Vector3 _networkRArmEndPos;

    private Vector3 _networkPrevLArmEndPos;
    private Vector3 _networkPrevRArmEndPos;
    private Vector3 _networkLArmSpeed;
    private Vector3 _networkRArmSpeed;

    private List<BezierKnot> _leftKnotsCache;
    private List<BezierKnot> _rightKnotsCache;
    private Vector3 _tempVector3 = Vector3.zero;
    private BezierKnot _cachedKnot0 = new BezierKnot(Vector3.zero);
    private BezierKnot _cachedKnot1 = new BezierKnot();
    private BezierKnot _cachedKnot2 = new BezierKnot();

    private BezierKnot _cachedGunRightLeftKnot1 = new BezierKnot(new Vector3(-0.12f, -0.1f, 0));
    private BezierKnot _cachedGunRightLeftKnot2 = new BezierKnot(new Vector3(-0.3f, 0, 0));

    private BezierKnot _cachedGunLeftRightKnot1 = new BezierKnot(new Vector3(0.12f, -0.1f, 0));
    private BezierKnot _cachedGunLeftRightKnot2 = new BezierKnot(new Vector3(0.3f, 0, 0));

    // >>>>>>> Develops
    private void Update()
    {
        // Debug.Log("플립 상태:" + _isFlipped + ", 마우스 위치 오른쪽:" + _isGunInRight);
        CaculateArmSpeed();

        if (PhotonNetwork.OfflineMode)
        {
            LookAtMouse();

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
        else
        {
            if (photonView.IsMine)
            {
                LookAtMouse();

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
            else
            {
                _gunAxis.up = Vector3.Lerp(_gunAxis.up, _networkGunAxisUp, Time.deltaTime * 20f);
                // _gunPos.position = Vector3.Lerp(_gunPos.position, _networkGunPos, Time.deltaTime * 10f);

                if (_leftKnotsCache == null) _leftKnotsCache = _leftArmSpline.Spline.Knots.ToList();
                if (_rightKnotsCache == null) _rightKnotsCache = _rightArmSpline.Spline.Knots.ToList();

                // List<BezierKnot> leftKnots = _leftArmSpline.Spline.Knots.ToList();
                // List<BezierKnot> rightKnots = _rightArmSpline.Spline.Knots.ToList();

                _leftArmSpline.Spline.SetKnot(0,
                    new BezierKnot(Vector3.Lerp(_leftKnotsCache[0].Position, _networkLArmStartPos, Time.deltaTime * 10f)));
                _leftArmSpline.Spline.SetKnot(1,
                    new BezierKnot(Vector3.Lerp(_leftKnotsCache[1].Position, _networkLArmMiddlePos, Time.deltaTime * 10f)));
                _leftArmSpline.Spline.SetKnot(2,
                    new BezierKnot(Vector3.Lerp(_leftKnotsCache[2].Position, _networkLArmEndPos + _networkLArmSpeed * _lag,
                        Time.deltaTime * 10f)));
                _rightArmSpline.Spline.SetKnot(0,
                    new BezierKnot(Vector3.Lerp(_rightKnotsCache[0].Position, _networkRArmStartPos, Time.deltaTime * 10f)));
                _rightArmSpline.Spline.SetKnot(1,
                    new BezierKnot(Vector3.Lerp(_rightKnotsCache[1].Position, _networkRArmMiddlePos, Time.deltaTime * 10f)));
                _rightArmSpline.Spline.SetKnot(2,
                    new BezierKnot(Vector3.Lerp(_rightKnotsCache[2].Position, _networkRArmEndPos + _networkRArmSpeed * _lag,
                        Time.deltaTime * 10f)));
            }
        }

        SetPrevPositions();
    }

    private void LateUpdate()
    {
        CheckMousePos();
        FlipCheck();
        SetBallPos();
    }

    private void SetPrevPositions()
    {
        _prevLArmEndPos = _leftArmSpline.Spline.Knots.ToList()[2].Position;
        _prevRArmEndPos = _rightArmSpline.Spline.Knots.ToList()[2].Position;
    }

    private void CaculateArmSpeed()
    {
        _tempVector3.Set(_leftArmSpline.Spline.Knots.ToList()[2].Position.x,
            _leftArmSpline.Spline.Knots.ToList()[2].Position.y,
            _leftArmSpline.Spline.Knots.ToList()[2].Position.z
        );

        _LArmSpeed = (_tempVector3 - _prevLArmEndPos) / Time.deltaTime;

        _tempVector3.Set(_rightArmSpline.Spline.Knots.ToList()[2].Position.x,
            _rightArmSpline.Spline.Knots.ToList()[2].Position.y,
            _rightArmSpline.Spline.Knots.ToList()[2].Position.z
        );
        _RArmSpeed = (_tempVector3 - _prevRArmEndPos) / Time.deltaTime;

        // _LArmSpeed = (new Vector3(_leftArmSpline.Spline.Knots.ToList()[2].Position.x,
        //     _leftArmSpline.Spline.Knots.ToList()[2].Position.y,
        //     _leftArmSpline.Spline.Knots.ToList()[2].Position.z) - _prevLArmEndPos) / Time.deltaTime;
        // _RArmSpeed = (new Vector3(_rightArmSpline.Spline.Knots.ToList()[2].Position.x,
        //     _rightArmSpline.Spline.Knots.ToList()[2].Position.y,
        //     _rightArmSpline.Spline.Knots.ToList()[2].Position.z) - _prevRArmEndPos) / Time.deltaTime;
    }

    /// <summary>
    /// 오른팔의 위치를 설정합니다.
    /// </summary>
    private void SetRightArmPosition()
    {
        if (_rightArmSpline == null || _gunPos == null) return;
        var pos = _gunPos.position - _rightArmSpline.transform.position;
        var middlePos = Vector3.Lerp(_rightArmSpline.transform.position, _gunPos.position, 0.5f);

        // 먼저 어깨에서 총으로 향하는 방향 벡터를 계산
        var dir = (_gunPos.position - _rightArmSpline.transform.position).normalized;

        // 방향 벡터가 얼마나 수직에 가까운지 계산 후 절대값을 사용
        float dot = Mathf.Abs(Vector3.Dot(Vector3.up, dir));

        // 중간 위치를 계산하고, 방향 벡터에 수직인 벡터를 사용하여 중간 위치를 조정
        float flipMultiplier = _isFlipped ? -1 : 1; // 플립 상태에 따라 방향을 조정
        if (_isFlipped) pos.x *= -1; // 플립 상태일 때 x위치를 반전
        var middleFinalPos = Vector3.Lerp(middlePos, middlePos + new Vector3(-dir.y, dir.x, 0) * -0.1f * flipMultiplier, 1 - dot);
        var localMiddlePos = _rightArmSpline.transform.InverseTransformPoint(middleFinalPos); // 중간 위치를 Spline 기준으로 변환
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
        var pos = _gunPos.position - _leftArmSpline.transform.position;
        var middlePos = Vector3.Lerp(_leftArmSpline.transform.position, _gunPos.position, 0.5f);
        var dir = (_gunPos.position - _leftArmSpline.transform.position).normalized;
        float dot = Mathf.Abs(Vector3.Dot(Vector3.up, dir));
        float flipMultiplier = _isFlipped ? -1 : 1; // 플립 상태에 따라 방향을 조정
        if (_isFlipped) pos.x *= -1; // 플립 상태일 때 x위치를 반전
        var middleFinalPos = Vector3.Lerp(middlePos, middlePos + new Vector3(-dir.y, dir.x, 0) * 0.1f * flipMultiplier, 1 - dot);
        var localMiddlePos = _leftArmSpline.transform.InverseTransformPoint(middleFinalPos); // 중간 위치를 Spline 기준으로 변환

        _cachedKnot1.Position = localMiddlePos;
        _cachedKnot2.Position = pos;
        _leftArmSpline.Spline.SetKnot(0, _cachedKnot0); // 시작점은 (0,0,0)
        _leftArmSpline.Spline.SetKnot(1, _cachedKnot1);
        _leftArmSpline.Spline.SetKnot(2, _cachedKnot2);
        // _leftArmSpline.Spline.SetKnot(0, new BezierKnot(Vector3.zero)); // 시작점은 (0,0,0)
        // _leftArmSpline.Spline.SetKnot(1, new BezierKnot(localMiddlePos));
        // _leftArmSpline.Spline.SetKnot(2, new BezierKnot(pos));
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

        if (photonView.IsMine)
        {
            if (_isGunInRight)
            {
                _gunPos.localRotation = Quaternion.Euler(0, 0, 0); // 오른팔 회전
                _leftArmSpline.Spline.SetKnot(1, _cachedGunRightLeftKnot1); // 왼팔 위치 초기화
                _leftArmSpline.Spline.SetKnot(2, _cachedGunRightLeftKnot2); // 왼팔 위치 초기화
            }
            else
            {
                _gunPos.localRotation = Quaternion.Euler(0, 180, 0); // 왼팔 회전
                _rightArmSpline.Spline.SetKnot(1, _cachedGunLeftRightKnot1); // 오른팔 위치 초기화
                _rightArmSpline.Spline.SetKnot(2, _cachedGunLeftRightKnot2); // 오른팔 위치 초기화
            }
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
                _gunPos.localRotation = Quaternion.Euler(0, 0, 0); // 오른팔 회전
                _leftArmSpline.Spline.SetKnot(1, _cachedGunLeftRightKnot1); // 왼팔 위치 초기화
                _leftArmSpline.Spline.SetKnot(2, _cachedGunLeftRightKnot2); // 왼팔 위치 초기화
            }
            else
            {
                _gunPos.localRotation = Quaternion.Euler(0, 180, 0); // 왼팔 회전
                _rightArmSpline.Spline.SetKnot(1, _cachedGunRightLeftKnot1); // 오른팔 위치 초기화
                _rightArmSpline.Spline.SetKnot(2, _cachedGunRightLeftKnot2); // 오른팔 위치 초기화
            }
        }
        else
        {
            if (_isFlipped)
            {
                _gunAxis.localRotation = Quaternion.Euler(0, 0, 0); // 오른팔 회전
                _leftArmSpline.Spline.SetKnot(1, _cachedGunLeftRightKnot1); // 왼팔 위치 초기화
                _leftArmSpline.Spline.SetKnot(2, _cachedGunLeftRightKnot2); // 왼팔 위치 초기화
            }
            else
            {
                _gunAxis.localRotation = Quaternion.Euler(0, 180, 0); // 왼팔 회전
                _rightArmSpline.Spline.SetKnot(1, _cachedGunRightLeftKnot1); // 오른팔 위치 초기화
                _rightArmSpline.Spline.SetKnot(2, _cachedGunRightLeftKnot2); // 오른팔 위치 초기화
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

        var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // 2D 게임이므로 z축은 0으로 설정

        var direction = (mousePosition - _gunAxis.transform.position).normalized;
        _gunAxis.transform.up = direction;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));

        if (stream.IsWriting)
        {
            // 로컬 플레이어가 데이터를 보냄
            stream.SendNext(_ballPos.localPosition);
            stream.SendNext(_gunPos.position);
            stream.SendNext(_gunAxis.up);
            stream.SendNext(new Vector3(_leftArmSpline.Spline.Knots.ToList()[0].Position.x,
                _leftArmSpline.Spline.Knots.ToList()[0].Position.y, 0));
            stream.SendNext(new Vector3(_rightArmSpline.Spline.Knots.ToList()[0].Position.x,
                _rightArmSpline.Spline.Knots.ToList()[0].Position.y, 0));
            stream.SendNext(new Vector3(_leftArmSpline.Spline.Knots.ToList()[1].Position.x,
                _leftArmSpline.Spline.Knots.ToList()[1].Position.y, 0));
            stream.SendNext(new Vector3(_rightArmSpline.Spline.Knots.ToList()[1].Position.x,
                _rightArmSpline.Spline.Knots.ToList()[1].Position.y, 0));
            stream.SendNext(new Vector3(_leftArmSpline.Spline.Knots.ToList()[2].Position.x,
                _leftArmSpline.Spline.Knots.ToList()[2].Position.y, 0));
            stream.SendNext(new Vector3(_rightArmSpline.Spline.Knots.ToList()[2].Position.x,
                _rightArmSpline.Spline.Knots.ToList()[2].Position.y, 0));
            stream.SendNext(_LArmSpeed);
            stream.SendNext(_RArmSpeed);
            stream.SendNext(lag);
        }
        else
        {
            // 리모트 플레이어가 데이터를 받음
            _networkBallPos = (Vector3)stream.ReceiveNext();
            _networkGunPos = (Vector3)stream.ReceiveNext();
            _networkGunAxisUp = (Vector3)stream.ReceiveNext();

            _networkLArmStartPos = (Vector3)stream.ReceiveNext();
            _networkRArmStartPos = (Vector3)stream.ReceiveNext();
            _networkLArmMiddlePos = (Vector3)stream.ReceiveNext();
            _networkRArmMiddlePos = (Vector3)stream.ReceiveNext();
            _networkLArmEndPos = (Vector3)stream.ReceiveNext();
            _networkRArmEndPos = (Vector3)stream.ReceiveNext();
            _networkLArmSpeed = (Vector3)stream.ReceiveNext();
            _networkRArmSpeed = (Vector3)stream.ReceiveNext();
            _lag = (float)stream.ReceiveNext();
        }
    }

    private void SetBallPos()
    {
        if (_isGunInRight)
        {
            if (_isFlipped)
            {
                _ballPos.localPosition = new Vector3(1.1f, 0.2f, 0);
            }
            else
            {
                _ballPos.localPosition = new Vector3(-1.1f, 0.2f, 0);
            }
        }
        else
        {
            if (_isFlipped)
            {
                _ballPos.localPosition = new Vector3(-1.1f, 0.2f, 0);
            }
            else
            {
                _ballPos.localPosition = new Vector3(1.1f, 0.2f, 0);
            }
        }
    }
}