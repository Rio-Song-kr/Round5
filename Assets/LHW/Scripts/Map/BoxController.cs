using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

public class BoxController : MonoBehaviourPun, IPunObservable
{
    FixedJoint2D fixedJoint;

    private Rigidbody2D rigid;

    // 네트워크 동기화용 변수
    private bool isPhysicsEnabled = false;
    private bool networkPhysicsEnabled = false;
    private Vector3 networkPos;
    private Quaternion networkRot;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        fixedJoint = GetComponent<FixedJoint2D>();
    }

    private void Update()
    {
        if (photonView.IsMine &&
            (fixedJoint != null && fixedJoint.connectedBody != null && fixedJoint.connectedBody.bodyType == RigidbodyType2D.Dynamic))
        {
            EnablePhysics();
        }
        if (!photonView.IsMine)
        {
            if (isPhysicsEnabled)
            {
                transform.position = Vector3.Lerp(transform.position, networkPos, Time.deltaTime * 10f);
                transform.rotation = Quaternion.Lerp(transform.rotation, networkRot, Time.deltaTime * 10f);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (photonView.IsMine && (collision.gameObject.CompareTag("Bullet") || collision.gameObject.CompareTag("Hammer")))
        {
            EnablePhysics();
        }
    }

    private void EnablePhysics()
    {
        networkPhysicsEnabled = true;

        if (PhotonNetwork.IsMasterClient)
        {
            rigid.bodyType = RigidbodyType2D.Dynamic;
            rigid.mass = 0.1f;
            rigid.gravityScale = 0.3f;
            isPhysicsEnabled = true;
        }
    }

    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(isPhysicsEnabled);
            if (isPhysicsEnabled)
            {
                stream.SendNext(rigid.velocity);
                stream.SendNext(rigid.angularVelocity);
            }
        }
        else if (stream.IsReading)
        {
            networkPos = (Vector3)stream.ReceiveNext();
            networkRot = (Quaternion)stream.ReceiveNext();
            networkPhysicsEnabled = (bool)stream.ReceiveNext();
            if (networkPhysicsEnabled != isPhysicsEnabled)
            {
                isPhysicsEnabled = networkPhysicsEnabled;
            }

            if (isPhysicsEnabled)
            {
                Vector2 networkVelocity = (Vector2)stream.ReceiveNext();
                float networkAngularVelocity = (float)stream.ReceiveNext();

                if (!photonView.IsMine)
                {
                    rigid.velocity = networkVelocity;
                    rigid.angularVelocity = networkAngularVelocity;
                }
            }
        }
    }
}