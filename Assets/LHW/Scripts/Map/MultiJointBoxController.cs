using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 총알에 피격되는 박스 플랫폼의 행동을 제어함. (두 개 이상의 Joint의 경우)
/// </summary>
public class MultiJointBoxController : MonoBehaviourPun, IPunObservable
{
    FixedJoint2D[] fixedJoint;

    private Rigidbody2D rigid;

    // 네트워크 동기화용 변수
    private bool isPhysicsEnabled = false;
    private bool networkPhysicsEnabled = false;
    private Vector3 networkPos;
    private Quaternion networkRot;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        fixedJoint = GetComponents<FixedJoint2D>();
    }

    private void Update()
    {
        if (FixedJointDisabled())
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

    private bool FixedJointDisabled()
    {
        for (int i = 0; i < fixedJoint.Length; i++)
        {
            if (photonView.IsMine && (fixedJoint[i].IsDestroyed()))                
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 물리 활성화
    /// </summary>
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