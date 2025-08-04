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

    private void Awake()
    {
        _status = GetComponent<PlayerStatus>();
    }

    private void Start()
    {
        _status.OnPlayerSpeedValueChanged += SetMoveSpeed;
        _status.OnPlayerFreezeValueChanged += SetFreeze;
        //# 무적
        _status.OnInvincibilityValueChanged += SetInvincibility;
        _status.OnPlayerCanAttackValueChanged += SetCanAttack;
        _status.OnPlayerHpValueChanged += SetHp;
    }

    private void Update()
    {
        LookAtMouse();
        // ShotCycle();
        CharMove();
        NetworkSync();
    }

    public void NetworkSync()
    {
        if (!photonView.IsMine)
        {
            // Synchronize position and rotation for non-local players
            transform.position = Vector3.Lerp(transform.position, _networkPosition, Time.deltaTime * _moveSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, _networkRotation, Time.deltaTime * 100f);
        }
    }

    private void LookAtMouse()
    {
        if (!photonView.IsMine)
        {
            return; // Only allow rotation for the master client
        }

        var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Ensure the z-coordinate is zero for 2D 
        var direction = (mousePosition - transform.position).normalized;
        transform.up = direction; // Set the bullet's rotation to face the mouse
    }

    private void CharMove()
    {
        if (!photonView.IsMine || _isFreeze)
        {
            return; // Only allow movement for the master client
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        var moveDirection = new Vector3(horizontal, vertical, 0).normalized;
        if (moveDirection.magnitude > 0.1f)
        {
            transform.Translate(moveDirection * Time.deltaTime * _moveSpeed, Space.World);

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

    private void SetHp(float currentHp, float maxHp)
    {
        _currentHp = currentHp;
        _maxHp = maxHp;
    }

    private void SetMoveSpeed(float moveSpeed, float airSpeed)
    {
        if (moveSpeed != 0) _isFreeze = false;
        _moveSpeed = moveSpeed;
        _airSpeed = airSpeed;
    }

    public void SetFreeze(bool value) => _isFreeze = value;

    private void SetInvincibility(bool value) => _isInvincibility = value;

    private void SetCanAttack(bool value) => _canAttack = value;
}