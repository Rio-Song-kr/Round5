using DG.Tweening;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class CanvasController : MonoBehaviourPunCallbacks
{
    [SerializeField] private Canvas MasterCanvas;
    [SerializeField] private Canvas ClientCanvas;
    [SerializeField] private CardSelectManager cardSelectManager;
    private FlipCard flipCard;
    private bool isMyTurn = false;
    private bool alreadyStarted = false;
    private int _currentSelectorNumber;
    public int CurrentSelectorNumber => _currentSelectorNumber;

    private void Start()
    {
        // Debug.Log("CanvasController Start 호출");
        MasterCanvas.gameObject.SetActive(false);
        ClientCanvas.gameObject.SetActive(false);

        // Debug.Log($"CanvasController PhotonNetwork.IsMasterClient = {PhotonNetwork.IsMasterClient}");

        if (PhotonNetwork.IsMasterClient)
        {
            // ���� ������ ����
            int firstSelectorActorNum = Random.Range(0, 2) == 0
                ? PhotonNetwork.PlayerList[0].ActorNumber
                : PhotonNetwork.PlayerList[1].ActorNumber;

            photonView.RPC(nameof(SetFirstSelectorActorNumber), RpcTarget.All, firstSelectorActorNum);
            photonView.RPC(nameof(TryStartCardSelection), RpcTarget.All, firstSelectorActorNum);
        }

        // ������/Ŭ���̾�Ʈ ���� ���� �ʱ�ȭ �õ�
        // TryStartCardSelection();
    }

    [PunRPC]
    private void SetFirstSelectorActorNumber(int actorNumber) => InGameManager.Instance.SetPlayerActorNumber(actorNumber);

    [PunRPC]
    public void TryStartCardSelection(int currentSelector)
    {

        _currentSelectorNumber = currentSelector;
        // Debug.Log($"[TryStartCardSelection] ȣ��� | alreadyStarted={alreadyStarted}");


        // Debug.Log("ī�� ���� ������ ���� �˸�");

        if (alreadyStarted)
        {
            // Debug.LogWarning("[TryStartCardSelection] �̹� ���۵� �� �ߴ�");
            alreadyStarted = false;

            return;
        }

        alreadyStarted = true;

        isMyTurn = PhotonNetwork.LocalPlayer.ActorNumber == currentSelector;
        Debug.Log($"AtorNumber : {PhotonNetwork.LocalPlayer.ActorNumber} - myTurn{isMyTurn}");

        if (isMyTurn)
        {
            // Debug.Log("���� ���� ������ �� ī�� ���� ����");
            Debug.Log($"LocalPlayer : {PhotonNetwork.LocalPlayer.ActorNumber}, Selector : {currentSelector}");

            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("isWinner", out object isWinnerObj)
                && (bool)isWinnerObj == true)
            {
                // Debug.Log("���� ���� �� ī�� ���� ����");

                InGameManager.Instance.SetPlayerSelects(PhotonNetwork.LocalPlayer.ActorNumber, true);

                DOVirtual.DelayedCall(2f, () => { photonView.RPC(nameof(RPC_SwitchTurnToOther), RpcTarget.All); });
                return;
            }

            // ���� ���� �����ϴ� ��� (���� or ù ����)
            int[] selectedIndexes = cardSelectManager.GetRandomCardIndexes().ToArray();
            photonView.RPC(nameof(RPC_SyncMasterCanvas), RpcTarget.All, selectedIndexes, currentSelector);
        }
        // else
        // {
        //     Debug.Log("������ ���� ������ �� ���� ���");
        // }
    }

    [PunRPC]
    public void RPC_SyncMasterCanvas(int[] indexes, int currentSelector)
    {
        // Debug.Log("SyncMasterCanvas 호출");
        MasterCanvas.gameObject.SetActive(true);
        ClientCanvas.gameObject.SetActive(false);

        cardSelectManager.UpdateCharacterVisibility();
        bool canInteract = PhotonNetwork.LocalPlayer.ActorNumber == currentSelector;
        // (int)PhotonNetwork.CurrentRoom.CustomProperties["IsFirstSelector"];

        cardSelectManager.SpawnCardsFromIndexes(indexes, canInteract);
    }

    [PunRPC]
    public void RPC_SwitchTurnToOther()
    {
        // if (cardSelectManager.HasSelected()) //  �̹� ������ ����� �� �Ѿ�� �ƹ��͵� ���� ����
        // {
        //     Debug.Log("�̹� ������ �÷��̾�� ����");
        //     return;
        // }

        // Debug.Log("���� �ݴ� �÷��̾�� ��ȯ��");

        MasterCanvas.gameObject.SetActive(false);
        ClientCanvas.gameObject.SetActive(true);

        // �� ����
        // isMyTurn = !isMyTurn; //  ���� �� ��ü ����

        if (!isMyTurn)
        {
            // ���� �� ���ʰ� �Ǿ��� �� ���� ����
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("isWinner", out object isWinnerObj)
                && (bool)isWinnerObj == true)
            {
                Debug.Log("���� ���� �� ī�� ���� ����");

                InGameManager.Instance.SetPlayerSelects(PhotonNetwork.LocalPlayer.ActorNumber, true);
                return;
            }

            // ���� ���ڰų� ù �� �� ���� ����
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
        bool canInteract = !isMyTurn; // �� ��° ������ ����� ���� ����
        cardSelectManager.SpawnClientCardsFromIndexes(indexes, canInteract);
    }

    public bool IsMyTurn() => isMyTurn;

    public bool IsMasterCanvasActive() => MasterCanvas != null && MasterCanvas.gameObject.activeSelf;

    public bool IsClientCanvasActive() => ClientCanvas != null && ClientCanvas.gameObject.activeSelf;

    public void ResetCardSelectionState()
    {
        // Debug.Log("ĵ���� ��Ʈ�ѷ��� ���� ī�弱�� �ʱ�ȭ");
        alreadyStarted = false;
        isMyTurn = false;

        MasterCanvas.gameObject.SetActive(false);
        ClientCanvas.gameObject.SetActive(false);
    }

    public void DecideNextSelector()
    {
        // Debug.Log("�ӽ� �׽�Ʈ�� ��,�� ���� ���� �ʱ�ȭ");
        int nextSelector = PhotonNetwork.PlayerList[0].ActorNumber;

        photonView.RPC(nameof(TryStartCardSelection), RpcTarget.All, nextSelector);
    }
}