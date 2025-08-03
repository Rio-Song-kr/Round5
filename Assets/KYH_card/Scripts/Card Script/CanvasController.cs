using DG.Tweening;
using Photon.Pun;
using UnityEngine;

public class CanvasController : MonoBehaviourPun
{
    [SerializeField] private Canvas MasterCanvas;
    [SerializeField] private Canvas ClientCanvas;
    [SerializeField] private CardSelectManager cardSelectManager;

    void OnEnable()
    {
        MasterCanvas.gameObject.SetActive(true);
        ClientCanvas.gameObject.SetActive(false);

        Debug.Log("온 인에이블 실행됨");
        //  if (PhotonNetwork.IsMasterClient)
        //  { // 카드 생성
        //      cardSelectManager.SpawnRandomCards1(photonView.IsMine); // 마스터만 상호작용 가능
        //  }

        // RPC로 참가자에게 동기화
        // if (PhotonNetwork.IsMasterClient)
        // {
        //     
        //     Debug.Log("마스터 카드 선택지 생성");
        //     int[] selectedIndexes = cardSelectManager.GetRandomCardIndexes().ToArray();
        //     photonView.RPC(nameof(RPC_SyncMasterCanvas), RpcTarget.All, selectedIndexes);
        //
        // }

        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("isWinner", out object isWinnerObj))
            {
                bool isWinner = (bool)isWinnerObj;

                if (isWinner)
                {
                    Debug.Log("마스터가 승자 → 카드 선택 건너뜀");

                    ExitGames.Client.Photon.Hashtable props = new();
                    props["Select"] = true;
                    PhotonNetwork.LocalPlayer.SetCustomProperties(props);

                    DOVirtual.DelayedCall(2f, () =>
                    {
                        // 여기서 RPC_SwitchToClientCanvas 실행 → 클라이언트에게만 선택 기회 줘야 함
                        photonView.RPC(nameof(RPC_SwitchToClientCanvas), RpcTarget.All);
                    });

                    return;
                }

                // 마스터가 패자 → 카드 선택 진행
                Debug.Log("마스터 카드 선택지 생성 (패자)");
            }
            else
            {
                // 첫 진입 → 선택 가능
                Debug.Log("첫 카드 선택 → 카드 선택 진행");
            }

            int[] selectedIndexes = cardSelectManager.GetRandomCardIndexes().ToArray();
            photonView.RPC(nameof(RPC_SyncMasterCanvas), RpcTarget.All, selectedIndexes);
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

        if (!PhotonNetwork.IsMasterClient)
        {
            // 클라이언트가 승자인 경우 → 선택 금지
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("isWinner", out object isWinnerObj)
                && (bool)isWinnerObj == true)
            {
                Debug.Log("클라이언트가 승자 → 카드 선택 생략");

                ExitGames.Client.Photon.Hashtable props = new();
                props["Select"] = true;
                PhotonNetwork.LocalPlayer.SetCustomProperties(props);
                return; // 선택 패널 생성하지 않음
            }

            // 클라이언트가 패자 → 선택 가능
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
