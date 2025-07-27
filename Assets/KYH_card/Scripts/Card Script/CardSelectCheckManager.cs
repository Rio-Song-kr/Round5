using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CardSelectCheckManager : MonoBehaviourPunCallbacks
{
    private bool hasSelected = false;

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public void OnCardSelected()
    {
        if (hasSelected) return;

        hasSelected = true;
        Debug.Log("내 카드 선택 완료됨");

        CardSelectCheckUpdate();
    }

    public void CardSelectCheckUpdate()
    {
        ExitGames.Client.Photon.Hashtable selectProperty = new ExitGames.Client.Photon.Hashtable
        {
            { "hasSelect", hasSelected }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(selectProperty);
    }

    public bool AllPlayerCardSelectCheck()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!player.CustomProperties.TryGetValue("hasSelect", out object value) || !(bool)value)
            {
                Debug.Log($"{player.NickName} 아직 선택 안함");
                return false;
            }
        }

        return true;
    }

    public override void OnPlayerPropertiesUpdate(Player target, ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        base.OnPlayerPropertiesUpdate(target, propertiesThatChanged);

        if (propertiesThatChanged.ContainsKey("hasSelect"))
        {
            if (PhotonNetwork.IsMasterClient && AllPlayerCardSelectCheck())
            {
                Debug.Log(" 모든 플레이어 카드 선택 완료 → Game Scene 로드");
                PhotonNetwork.LoadLevel("Game Scene");
            }
        }
    }
}

