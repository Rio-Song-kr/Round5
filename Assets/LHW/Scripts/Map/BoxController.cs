using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 총알에 피격되는 박스 플랫폼의 행동을 제어함. (단일 Joint의 경우)
/// </summary>
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
        // 자신과 연결된 Joint (자신보다 아래에 있는 오브젝트) 가 파괴되었을 경우 물리 활성화
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
        // 총알 혹은 해머에 피격되었을 때 물리 활성화
        if (photonView.IsMine && (collision.gameObject.CompareTag("Bullet") || collision.gameObject.CompareTag("Hammer")))
        {
            EnablePhysics();
        }
    }

    /// <summary>
    /// 물리 활성화
    /// </summary>
    private void EnablePhysics()
    {
        networkPhysicsEnabled = true;

        // 물리 판정은 마스터 클라이언트만 하며,
        // 상대 플레이어는 물리 상태를 Kinematic으로 유지한 채 물리 변화를 읽어오는 방식으로 동기화
        // 플레이어가 각자 물리 판정을 하는 것을 방지하기 위함
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
            // 만약 네트워크상 물리 활성화가 누락되었을 경우 한 번 더 체크
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