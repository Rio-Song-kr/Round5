using DG.Tweening;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
public class CanvasController : MonoBehaviourPunCallbacks
{
    [SerializeField] private Canvas MasterCanvas;
    [SerializeField] private Canvas ClientCanvas;
    [SerializeField] private CardSelectManager cardSelectManager;
    FlipCard flipCard;
    private bool isMyTurn = false;
    private bool alreadyStarted = false;
    void Start()
    {
        MasterCanvas.gameObject.SetActive(false);
        ClientCanvas.gameObject.SetActive(false);

        Debug.Log("실행됨");

        if (PhotonNetwork.IsMasterClient)
        {
            // 랜덤 선택자 결정
            int firstSelectorActorNum = Random.Range(0, 2) == 0
                ? PhotonNetwork.PlayerList[0].ActorNumber
                : PhotonNetwork.PlayerList[1].ActorNumber;

            ExitGames.Client.Photon.Hashtable props = new();
            props["IsFirstSelector"] = firstSelectorActorNum;
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }

        // 마스터/클라이언트 구분 없이 초기화 시도
       // TryStartCardSelection();
    }



    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("IsFirstSelector"))
        {
            Debug.Log("온룸프로퍼티업데이트 실행");
            TryStartCardSelection();
        }
    }

    public void TryStartCardSelection()
    {
        Debug.Log($"[TryStartCardSelection] 호출됨 | alreadyStarted={alreadyStarted}");

        alreadyStarted = false;

        Debug.Log("카드 선택 과정의 시작 알림");

        if (alreadyStarted)
        {
            Debug.LogWarning("[TryStartCardSelection] 이미 시작됨 → 중단");
            return;
        }
        if (!PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("IsFirstSelector", out object selectorObj)) return;

        alreadyStarted = true;

        int selectorActorNum = (int)selectorObj;

        isMyTurn = (PhotonNetwork.LocalPlayer.ActorNumber == selectorActorNum);

        if (isMyTurn)
        {
            Debug.Log("내가 먼저 선택자 → 카드 선택 시작");

            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("isWinner", out object isWinnerObj)
                && (bool)isWinnerObj == true)
            {
                Debug.Log("나는 승자 → 카드 선택 생략");

                ExitGames.Client.Photon.Hashtable props = new();
                props["Select"] = true;
                PhotonNetwork.LocalPlayer.SetCustomProperties(props);

                DOVirtual.DelayedCall(2f, () =>
                {
                    photonView.RPC(nameof(RPC_SwitchTurnToOther), RpcTarget.All);
                });
                return;
            }

            // 내가 먼저 선택하는 사람 (패자 or 첫 선택)
            int[] selectedIndexes = cardSelectManager.GetRandomCardIndexes().ToArray();
            photonView.RPC(nameof(RPC_SyncMasterCanvas), RpcTarget.All, selectedIndexes);
        }
        else
        {
            Debug.Log("상대방이 먼저 선택함 → 관전 대기");
        }
    }

    [PunRPC]
    public void RPC_SyncMasterCanvas(int[] indexes)
    {
        MasterCanvas.gameObject.SetActive(true);
        ClientCanvas.gameObject.SetActive(false);
        cardSelectManager.UpdateCharacterVisibility();
        bool canInteract = PhotonNetwork.LocalPlayer.ActorNumber ==
                           (int)PhotonNetwork.CurrentRoom.CustomProperties["IsFirstSelector"];

        cardSelectManager.SpawnCardsFromIndexes(indexes, canInteract);
    }

    [PunRPC]
    public void RPC_SwitchTurnToOther()
    {
        // if (cardSelectManager.HasSelected()) //  이미 선택한 사람은 턴 넘어가도 아무것도 하지 않음
        // {
        //     Debug.Log("이미 선택한 플레이어는 무시");
        //     return;
        // }

        Debug.Log("턴이 반대 플레이어로 전환됨");

        MasterCanvas.gameObject.SetActive(false);
        ClientCanvas.gameObject.SetActive(true);

        // 턴 변경
        // isMyTurn = !isMyTurn; //  현재 턴 주체 변경

        if (!isMyTurn)
        {
            // 지금 내 차례가 되었다 → 승패 판정
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("isWinner", out object isWinnerObj)
                && (bool)isWinnerObj == true)
            {
                Debug.Log("나는 승자 → 카드 선택 생략");

                ExitGames.Client.Photon.Hashtable props = new();
                props["Select"] = true;
                PhotonNetwork.LocalPlayer.SetCustomProperties(props);
                return;
            }

            // 내가 패자거나 첫 턴 → 선택 가능
            int[] selectedIndexes = cardSelectManager.GetRandomCardIndexes().ToArray();
            photonView.RPC(nameof(RPC_SyncClientCanvas), RpcTarget.All, selectedIndexes);
        }
    }

    [PunRPC]
    public void RPC_SyncClientCanvas(int[] indexes)
    {
        MasterCanvas.gameObject.SetActive(false);
        ClientCanvas.gameObject.SetActive(true);
        cardSelectManager.UpdateCharacterVisibility();
        bool canInteract = !isMyTurn; // 두 번째 차례인 사람만 선택 가능
        cardSelectManager.SpawnClientCardsFromIndexes(indexes, canInteract);
    }

    public bool IsMyTurn()
    {
        return isMyTurn;
    }

    public bool IsMasterCanvasActive()
    {
        return MasterCanvas != null && MasterCanvas.gameObject.activeSelf;
    }

    public bool IsClientCanvasActive()
    {
        return ClientCanvas != null && ClientCanvas.gameObject.activeSelf;
    }

    public void ResetCardSelectionState()
    {
        Debug.Log("캔버스 컨트롤러의 정보 카드선택 초기화");
        alreadyStarted = false;
        isMyTurn = false;

        MasterCanvas.gameObject.SetActive(false);
        ClientCanvas.gameObject.SetActive(false);


    }

    public void DecideNextSelector()
    {
        Debug.Log("임시 테스트용 선,후 선택 순서 초기화");
        int nextSelector = Random.Range(0, 2) == 0
            ? PhotonNetwork.PlayerList[0].ActorNumber
            : PhotonNetwork.PlayerList[1].ActorNumber;

        ExitGames.Client.Photon.Hashtable props = new();
        props["IsFirstSelector"] = nextSelector;
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }
}
