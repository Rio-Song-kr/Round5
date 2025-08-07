using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class BulletTest : MonoBehaviourPun, IPunObservable
{
    // 무기 컨트롤러에서 조절
    // [SerializeField] private GameObject _bulletPrefab;
    // [SerializeField] private float _shotInterval = 1f;
    // [SerializeField] private GameObject _body;
    // [SerializeField] private Transform _muzzle;
    // private float _shotTimer = 0f;
    
    private Vector3 _networkPosition;
    private Quaternion _networkRotation;
    
    // 지연보상 :: S
    private Vector3 _networkVelocity;

    private Rigidbody2D _rigid;
    private double _lastSyncTime;
    // 지연보상 :: E

    void Awake()
    {

        // 지연보상 :: S
        _rigid = GetComponent<Rigidbody2D>();
        // 지연보상 :: E
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            LookAtMouse();
            CharMove();
        }
        else
        {
            // 지연보상 :: S
            // 예측 보간 적용
            float lag = (float)(PhotonNetwork.Time - _lastSyncTime);
            Vector3 predictedPos = _networkPosition + _networkVelocity * lag;

            // 예측 위치로 이동
            transform.position = Vector3.Lerp(transform.position, predictedPos, Time.deltaTime * 10f);
            transform.rotation = Quaternion.Slerp(transform.rotation, _networkRotation, Time.deltaTime * 10f);
            // 지연보상 :: E
        }
    }

    // public void NetworkSync()
    // {
    //     if (!photonView.IsMine)
    //     {
    //         // Synchronize position and rotation for non-local players
    //         transform.position = Vector3.Lerp(transform.position, _networkPosition, Time.deltaTime * 5f);
    //         transform.rotation = Quaternion.Slerp(transform.rotation, _networkRotation, Time.deltaTime * 100f);
    //     }
    // }

    // 무기 컨트롤러에서 조절
    // private void ShotCycle()
    // {
    //     _shotTimer += Time.deltaTime;
    //     if (_shotTimer >= _shotInterval && Input.GetMouseButton(0) && photonView.IsMine)
    //     {
    //         photonView.RPC("Shot", RpcTarget.All);
    //         _shotTimer = 0f;
    //     }
    // }
    //
    // [PunRPC]
    // private void Shot()
    // {
    //     // 240706 수정
    //     // GameObject bullet = Instantiate(_bulletPrefab, _muzzle.position, _muzzle.rotation);
    //     if (!photonView.IsMine) return; 
    //     GameObject bullet = PhotonNetwork.Instantiate("Bullets/Bullet", _muzzle.position, _muzzle.rotation);
    //     
    //     CameraShake.Instance.ShakeCaller(0.3f, 0.05f);
    // }

    private void LookAtMouse()
    {
        if (!photonView.IsMine)
        {
            return; // Only allow rotation for the master client
        }

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Ensure the z-coordinate is zero for 2D 
        Vector3 direction = (mousePosition - transform.position).normalized;
        transform.up = direction; // Set the bullet's rotation to face the mouse
    }

    private void CharMove()
    {
        // if (!photonView.IsMine)
        // {
        //     return; // Only allow movement for the master client
        // }
        //
        // float horizontal = Input.GetAxisRaw("Horizontal");
        // float vertical = Input.GetAxisRaw("Vertical");
        //
        // Vector3 moveDirection = new Vector3(horizontal, vertical, 0).normalized;
        // if (moveDirection.magnitude > 0.1f)
        // {
        //     transform.Translate(moveDirection * Time.deltaTime * 5f, Space.World);
        // }
        
        
        
        // 지연보상 :: S
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 moveDir = new Vector3(h, v, 0f).normalized;

        if (moveDir.sqrMagnitude > 0.01f)
        {
            _rigid.velocity = moveDir * 5f;
        }
        else
        {
            _rigid.velocity = Vector2.zero;
        }
        // 지연보상 :: E
    }
    

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            // 지연보상 :: S
            stream.SendNext((Vector3)_rigid.velocity);
            // 지연보상 :: E
        }
        else if (stream.IsReading)
        {
            _networkPosition = (Vector3)stream.ReceiveNext();
            _networkRotation = (Quaternion)stream.ReceiveNext();
            // 지연보상 :: S
            _networkVelocity = (Vector3)stream.ReceiveNext();
            _lastSyncTime = info.SentServerTime;
            // 지연보상 :: E
        }
    }
}
