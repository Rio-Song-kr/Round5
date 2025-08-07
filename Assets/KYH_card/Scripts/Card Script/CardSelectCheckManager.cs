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
        if (cardSelectPanels.TryGetValue(player.ActorNumber, out var panel))
        {
            panel.Init(player);
            return;
        }

        PhotonNetwork.AutomaticallySyncScene = true;
        var obj = Instantiate(cardSelectPanelPrefabs);
        obj.transform.SetParent(cardSelectPanelContent1);
        var Panel = obj.GetComponent<CardSelectPanelItem>();
        // �ʱ�ȭ
        Panel.Init(player);
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
            var Panel = obj.GetComponent<CardSelectPanelItem>();
            // �ʱ�ȭ
            Panel.Init(player);
            cardSelectPanels.Add(player.ActorNumber, Panel);
        }
    }

    public bool AllPlayerCardSelectCheck()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            // �������� ���� �÷��̾� �߰�
            if (!player.CustomProperties.TryGetValue("Select", out object value) || !(bool)value)
            {
                var other = PhotonNetwork.PlayerList
                    .FirstOrDefault(p => p != PhotonNetwork.LocalPlayer);

                // Debug.Log($"���� ���� �� �� �÷��̾�: {other.NickName}");


                return false;
            }
        }

        return true;
    }
}