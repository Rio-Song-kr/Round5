using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasController : MonoBehaviourPun
{
    [SerializeField] private Canvas MasterCanvas;
    [SerializeField] private Canvas ClientCanvas;
    [SerializeField] private CardSelectManager cardSelectManager;

    void Start()
    {
            MasterCanvas.gameObject.SetActive(true);
            ClientCanvas.gameObject.SetActive(false);

        if (PhotonNetwork.IsMasterClient)
        { // 카드 생성
            cardSelectManager.SpawnRandomCards1(true); // 마스터만 상호작용 가능
        }
        
            // RPC로 참가자에게 동기화
            photonView.RPC(nameof(RPC_SyncMasterCanvas), RpcTarget.All);

    }

    [PunRPC]
    void RPC_SyncMasterCanvas()
    {
        MasterCanvas.gameObject.SetActive(true);
        ClientCanvas.gameObject.SetActive(false);

        cardSelectManager.SpawnRandomCards1(false); // 참가자 관전용 (선택 불가)
    }

    [PunRPC]
    public void RPC_SwitchToClientCanvas()
    {
        MasterCanvas.gameObject.SetActive(false);
        ClientCanvas.gameObject.SetActive(true);

        if (PhotonNetwork.IsMasterClient)
        {
            cardSelectManager.SpawnRandomCards1(false); // 마스터는 관전
        }
        else
        {
            cardSelectManager.SpawnRandomCards1(true); // 참가자는 상호작용 가능
        }
    }
}
