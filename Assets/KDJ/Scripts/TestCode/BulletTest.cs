using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class BulletTest : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private float _shotInterval;
    [SerializeField] private GameObject _body;
    [SerializeField] private Transform _muzzle;


    private float _shotTimer = 0f;
    private Vector3 _networkPosition;
    private Quaternion _networkRotation;


    void Awake()
    {

    }

    void Update()
    {
        LookAtMouse();
        ShotCycle();
        CharMove();
        NetworkSync();
    }

    private void NetworkSync()
    {
        if (!photonView.IsMine)
        {
            // Synchronize position and rotation for non-local players
            transform.position = Vector3.Lerp(transform.position, _networkPosition, Time.deltaTime * 5f);
            transform.rotation = Quaternion.Slerp(transform.rotation, _networkRotation, Time.deltaTime * 100f);
        }
    }

    private void ShotCycle()
    {
        _shotTimer += Time.deltaTime;
        if (_shotTimer >= _shotInterval && Input.GetMouseButton(0) && photonView.IsMine)
        {
            photonView.RPC("Shot", RpcTarget.All);
            _shotTimer = 0f;
        }
    }

    [PunRPC]
    private void Shot()
    {
        // 240706 수정
        // GameObject bullet = Instantiate(_bulletPrefab, _muzzle.position, _muzzle.rotation);
        if (!photonView.IsMine) return; 
        GameObject bullet = PhotonNetwork.Instantiate(_bulletPrefab.name, _muzzle.position, _muzzle.rotation);
        
        CameraShake.Instance.ShakeCaller(0.3f, 0.05f);
    }

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
        if (!photonView.IsMine)
        {
            return; // Only allow movement for the master client
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, vertical, 0).normalized;
        if (moveDirection.magnitude > 0.1f)
        {
            transform.Translate(moveDirection * Time.deltaTime * 5f, Space.World);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We are the owner of this object, send data
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else if (stream.IsReading)
        {
            // We are receiving data
            _networkPosition = (Vector3)stream.ReceiveNext();
            _networkRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
