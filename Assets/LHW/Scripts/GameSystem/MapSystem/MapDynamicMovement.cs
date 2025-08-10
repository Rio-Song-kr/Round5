using DG.Tweening;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// ���� ���� �� ���� �̵���ų ��, �� �������� ǥ���ϴ� ��ũ��Ʈ
/// </summary>
public class MapDynamicMovement : MonoBehaviourPun, IPunObservable
{
    private MapController mapController;
    private RandomMapPresetCreator randomMapPresetCreator;

    [SerializeField] private GameObject[] mapComponents;

    // ù ��° �÷����� �����̱� �����ϴ� ����(������)
    [SerializeField] private float moveDelay = 0.5f;
    // �� �÷����� �̵��ϱ� �����ϴ� ����
    [SerializeField] private float moveDurationOffset = 0.2f;

    private Vector3 networkPos;
    private Quaternion networkRot;
    private bool networkActiveSelf;

    private void Start()
    {
        mapController = GetComponentInParent<MapController>();
        randomMapPresetCreator = GetComponentInParent<RandomMapPresetCreator>();
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            float distance = Vector3.Distance(transform.position, networkPos);

            if (distance > 0.01f)
            {
                transform.position = Vector3.Lerp(transform.position, networkPos, Time.deltaTime * 10f);
            }
            else
            {
                transform.position = networkPos;
            }
        }
    }

    public void DynamicMove()
    {
        photonView.RPC(nameof(RPC_DynamicMove), RpcTarget.All);
    }

    [PunRPC]
    public void RPC_DynamicMove()
    {
        for (int i = 0; i < mapComponents.Length; i++)
        {
            float duration = moveDelay + i * moveDurationOffset;
            mapComponents[i].transform
                .DOMove(mapComponents[i].transform.position + new Vector3(-randomMapPresetCreator.MapTransformOffset, 0, 0),
                    duration)
                .SetDelay(mapController.MapChangeDelay).SetEase(Ease.InOutCirc);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && PhotonNetwork.IsMasterClient)
        {
            stream.SendNext(transform.position);
        }
        else if (stream.IsReading)
        {
            networkPos = (Vector3)stream.ReceiveNext();
        }
    }

    [PunRPC]
    public void SetParentToRound(int round)
    {
        var roundParent = FindObjectOfType<RandomMapPresetCreator>().GetRoundTransform(round);
        transform.SetParent(roundParent);
    }
}