using Photon.Pun;
using UnityEngine;

public class CanvasController : MonoBehaviourPun
{
    [SerializeField] private Canvas MasterCanvas;
    [SerializeField] private Canvas ClientCanvas;
    [SerializeField] private CardSelectManager cardSelectManager;

    void Start()
    {
        MasterCanvas.gameObject.SetActive(true);
        ClientCanvas.gameObject.SetActive(false);

        //  if (PhotonNetwork.IsMasterClient)
        //  { // 카드 생성
        //      cardSelectManager.SpawnRandomCards1(photonView.IsMine); // 마스터만 상호작용 가능
        //  }

        // RPC로 참가자에게 동기화
        if (PhotonNetwork.IsMasterClient)
        {
            
            Debug.Log("마스터 카드 선택지 생성");
            int[] selectedIndexes = cardSelectManager.GetRandomCardIndexes().ToArray();
            photonView.RPC(nameof(RPC_SyncMasterCanvas), RpcTarget.All);

        }
    }

    [PunRPC]
    void RPC_SyncMasterCanvas(int[] indexes)
    {
        MasterCanvas.gameObject.SetActive(true);
        ClientCanvas.gameObject.SetActive(false);
        Debug.Log("마스터 카드 선택지 생성");
        bool canInteract = PhotonNetwork.IsMasterClient;
        cardSelectManager.SpawnCardsFromIndexes(indexes, canInteract); // 참가자 관전용 (선택 불가)
    }

    [PunRPC]
    public void RPC_SwitchToClientCanvas()
    {
        Debug.Log("캔버스 스위칭 시작");
        MasterCanvas.gameObject.SetActive(false);
        ClientCanvas.gameObject.SetActive(true);

        // if (PhotonNetwork.IsMasterClient)
        // {
        //     cardSelectManager.SpawnRandomCards1(false); // 마스터는 관전
        // }
        // else
        // {
        //     cardSelectManager.SpawnRandomCards1(true); // 참가자는 상호작용 가능
        // }

        if (PhotonNetwork.IsMasterClient == false) // 참가자만 인덱스 생성자
        {
            Debug.Log("참가자 카드 선택지 생성");
            int[] selectedIndexes = cardSelectManager.GetRandomCardIndexes().ToArray();
            photonView.RPC(nameof(RPC_SyncClientCanvas), RpcTarget.All, selectedIndexes);
        }
    }

    [PunRPC]
    public void RPC_SyncClientCanvas(int[] indexes)
    {
        MasterCanvas.gameObject.SetActive(false);
        ClientCanvas.gameObject.SetActive(true);
        Debug.Log("참가자 카드 선택지 생성");
        bool canInteract = !PhotonNetwork.IsMasterClient; // 참가자만 상호작용 가능
        cardSelectManager.SpawnClientCardsFromIndexes(indexes, canInteract);
    }
}
