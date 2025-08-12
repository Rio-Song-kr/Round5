using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardSelectCheckManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject cardSelectPanelPrefabs;
    [SerializeField] private Transform cardSelectPanelContent1;
    [SerializeField] private Transform cardSelectPanelContent2;

    public Dictionary<int, CardSelectPanelItem> cardSelectPanels = new Dictionary<int, CardSelectPanelItem>();

    //  void Awake()
    //  {
    //      PhotonNetwork.AutomaticallySyncScene = true;
    //  }

    public void CardSelectPanelSpawn(Player player)
    {
        bool isSelected;
        if (cardSelectPanels.TryGetValue(player.ActorNumber, out var panel))
        {
            isSelected = panel.Init(player);

            InGameManager.Instance.SetPlayerSelects(PhotonNetwork.LocalPlayer.ActorNumber, isSelected);
            return;
        }

        PhotonNetwork.AutomaticallySyncScene = true;
        var obj = Instantiate(cardSelectPanelPrefabs);
        obj.transform.SetParent(cardSelectPanelContent1);
        var Panel = obj.GetComponent<CardSelectPanelItem>();
        // �ʱ�ȭ

        isSelected = panel.Init(player);

        InGameManager.Instance.SetPlayerSelects(PhotonNetwork.LocalPlayer.ActorNumber, isSelected);
        cardSelectPanels.Add(player.ActorNumber, Panel);
    }

    public void cardSelectPanelSpawn()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        // ���� ���� ���� ���� �� ȣ��
        foreach (var player in PhotonNetwork.PlayerList)
        {
            var obj = Instantiate(cardSelectPanelPrefabs);
            obj.transform.SetParent(cardSelectPanelContent1);
            var panel = obj.GetComponent<CardSelectPanelItem>();
            // �ʱ�ȭ
            panel.Init(player);

            bool isSelected = panel.Init(player);

            InGameManager.Instance.SetPlayerSelects(PhotonNetwork.LocalPlayer.ActorNumber, isSelected);
            cardSelectPanels.Add(player.ActorNumber, panel);
        }
    }

    public bool AllPlayerCardSelectCheck()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            // Debug.Log($"{player.ActorNumber} - {InGameManager.Instance.PlayerSelection[player.ActorNumber]}");

            if (!InGameManager.Instance.PlayerSelection[player.ActorNumber])
                return false;
        }

        return true;
    }
}