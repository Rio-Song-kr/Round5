using Photon.Pun;
using UnityEngine;

public class BoxController : MonoBehaviourPun, IPunObservable
{
    FixedJoint2D fixedJoint;

    private Rigidbody2D rigid;

    private bool isPhysicsEnabled = false;
    private bool networkPhysicsEnabled = false;

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
            Debug.Log("333");
            EnablePhysics();

            // 이게 없으니까 마스터가 아닌 플레이어는 아예 물건이 안옮겨짐            

            /*if (isPhysicsEnabled)
            {
                transform.position = Vector3.Lerp(transform.position, networkPos, Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, networkRot, Time.deltaTime);
            }
            */
        }
        if (!photonView.IsMine)
        {
            if (isPhysicsEnabled)
            {
                Debug.Log("444");
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

    private Vector3 networkPos;
    private Quaternion networkRot;
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