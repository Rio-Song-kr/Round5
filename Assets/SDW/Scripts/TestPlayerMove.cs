using System;
using Photon.Pun;
using UnityEngine;

public class TestPlayerMove : MonoBehaviourPun, IPunObservable
{
    private Vector3 _networkPosition;
    private Quaternion _networkRotation;
    private PlayerStatus _status;

    private float _moveSpeed = 5f;
    public float MoveSpeed => _moveSpeed;

    private float _airSpeed = 0.2f;

    private bool _isFreeze = false;

    public Action<bool> OnPlayerMoveStateChanged;

    private float _currentHp;
    private float _maxHp;

    private bool _isPlayerMoved;
    private bool _prevPlayerMoveState;
    private bool _canAttack;
    private bool _isInvincibility;

    /// <summary>
    /// 컴포넌트 연결
    /// </summary>
    private void Awake() => _status = GetComponent<PlayerStatus>();

    /// <summary>
    /// 초기화 및 이벤트 구독
    /// </summary>
    private void Start()
    {
        _status.OnPlayerSpeedValueChanged += SetMoveSpeed;
        _status.OnPlayerFreezeValueChanged += SetFreeze;
        //# 무적
        _status.OnInvincibilityValueChanged += SetInvincibility;
        _status.OnPlayerCanAttackValueChanged += SetCanAttack;
        _status.OnPlayerHpValueChanged += SetHp;
    }

    /// <summary>
    /// 주기적으로 게임 내 상태 업데이트 및 동기화 처리를 수행
    /// </summary>
    private void Update()
    {
        LookAtMouse();
        // ShotCycle();
        CharMove();
        NetworkSync();
    }

    /// <summary>
    /// 동기화 관련 네트워크 데이터 업데이트를 수행
    /// 로컬 플레이어가 아닌 경우 네트워크로부터 수신된 위치와 회전 정보를 기반으로 오브젝트의 위치와 회전을 점진적으로 동기화
    /// </summary>
    public void NetworkSync()
    {
        if (!photonView.IsMine)
        {
            transform.position = Vector3.Lerp(transform.position, _networkPosition, Time.deltaTime * _moveSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, _networkRotation, Time.deltaTime * 100f);
        }
    }

    /// <summary>
    /// 마우스 포인터를 향하도록 오브젝트 방향 조정
    /// </summary>
    private void LookAtMouse()
    {
        if (!photonView.IsMine) return;

        var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        var direction = (mousePosition - transform.position).normalized;
        transform.up = direction;
    }

    /// <summary>
    /// 플레이어 캐릭터의 이동을 처리하고 동기화 상태를 업데이트
    /// 로컬 플레이어가 아닌 경우 이동하지 않으며, 동결 상태일 때도 이동이 제한됨
    /// 키보드 입력을 통해 방향과 속도를 결정하고, 이동 중인 상태 변화를 옵저버에게 알림
    /// </summary>
    private void CharMove()
    {
        if (!photonView.IsMine || _isFreeze) return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        var moveDirection = new Vector3(horizontal, vertical, 0).normalized;
        if (moveDirection.magnitude > 0.1f)
        {
            transform.Translate( Time.deltaTime * _moveSpeed * moveDirection, Space.World);

            //# 이전에 움직이지 않았을 때만 Invoke
            if (!_isPlayerMoved)
            {
                _isPlayerMoved = true;
                OnPlayerMoveStateChanged?.Invoke(_isPlayerMoved);
            }
        }
        else
        {
            //# 이전에 움직였을 때만 Invoke
            if (_isPlayerMoved)
            {
                _isPlayerMoved = false;
                OnPlayerMoveStateChanged?.Invoke(_isPlayerMoved);
            }
        }
    }

    /// <summary>
    /// Photon 네트워크 뷰 데이터를 직렬화 및 역직렬화
    /// </summary>
    /// <param name="stream">데이터를 직렬화하거나 역직렬화할 PhotonStream 객체</param>
    /// <param name="info">데이터 전송에 대한 정보를 담고 있는 PhotonMessageInfo 객체</param>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else if (stream.IsReading)
        {
            _networkPosition = (Vector3)stream.ReceiveNext();
            _networkRotation = (Quaternion)stream.ReceiveNext();
        }
    }

    /// <summary>
    /// 플레이어의 현재 체력과 최대 체력을 설정
    /// </summary>
    /// <param name="currentHp">현재 체력 값</param>
    /// <param name="maxHp">최대 체력 값</param>
    private void SetHp(float currentHp, float maxHp)
    {
        _currentHp = currentHp;
        _maxHp = maxHp;
    }

    /// <summary>
    /// 플레이어의 이동 속도와 공기 저항을 설정
    /// </summary>
    /// <param name="moveSpeed">플레이어의 지상 이동 속도</param>
    /// <param name="airSpeed">플레이어의 공중 이동 속도</param>
    private void SetMoveSpeed(float moveSpeed, float airSpeed)
    {
        if (moveSpeed != 0) _isFreeze = false;
        _moveSpeed = moveSpeed;
        _airSpeed = airSpeed;
    }

    /// <summary>
    /// 플레이어의 동결 상태를 설정
    /// </summary>
    /// <param name="value">동결 상태를 설정ㅇ하며 true이면 동결 활성화, false면 동결 비활성화</param>
    public void SetFreeze(bool value) => _isFreeze = value;

    /// <summary>
    /// 플레이어의 무적 상태를 업데이트
    /// </summary>
    /// <param name="value">무적 상태를 설정하며 true면 무적 활성화, false면 무적 비활성화)</param>
    private void SetInvincibility(bool value) => _isInvincibility = value;

    /// <summary>
    /// 플레이어의 공격 가능 여부를 설정합니다.
    /// </summary>
    /// <param name="value">플레이어의 공격 가능 여부를 설명하며 true이면 공격 가능, false이면 공격 불가능</param>
    private void SetCanAttack(bool value) => _canAttack = value;
}