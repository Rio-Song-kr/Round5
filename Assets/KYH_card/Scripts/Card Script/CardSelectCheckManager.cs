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
        if (cardSelectPanels.TryGetValue(player.ActorNumber, out CardSelectPanelItem panel))
        {
            panel.Init(player);
            return;
        }

        PhotonNetwork.AutomaticallySyncScene = true;
        GameObject obj = Instantiate(cardSelectPanelPrefabs);
        obj.transform.SetParent(cardSelectPanelContent1);
        CardSelectPanelItem Panel = obj.GetComponent<CardSelectPanelItem>();
        // 초기화
        Panel.Init(player);
        cardSelectPanels.Add(player.ActorNumber, Panel);

    }

    public void cardSelectPanelSpawn()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        // 내가 새로 입장 했을 떄 호출
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject obj = Instantiate(cardSelectPanelPrefabs);
            obj.transform.SetParent(cardSelectPanelContent1);
            CardSelectPanelItem Panel = obj.GetComponent<CardSelectPanelItem>();
            // 초기화
            Panel.Init(player);
            cardSelectPanels.Add(player.ActorNumber, Panel);
        }


    }


    public bool AllPlayerCardSelectCheck()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            // 선택하지 않은 플레이어 발견
            if (!player.CustomProperties.TryGetValue("Select", out object value) || !(bool)value)
            {
                Player other = PhotonNetwork.PlayerList
                .FirstOrDefault(p => p != PhotonNetwork.LocalPlayer);

                Debug.Log($"아직 선택 안 한 플레이어: {other.NickName}");


                return false;
            }
        }

        return true;
    }

    // public override void OnPlayerPropertiesUpdate(Player target, ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    // {
    //     base.OnPlayerPropertiesUpdate(target, propertiesThatChanged);
    //
    //     if (propertiesThatChanged.ContainsKey("Select"))
    //     {
    //         if (AllPlayerCardSelectCheck() == true)
    //         {
    //             Debug.Log(" 모든 플레이어 카드 선택 완료 → Game Scene 로드");
    //             PhotonNetwork.LoadLevel("Game Scene");
    //         }
    //     }
    // }
}

