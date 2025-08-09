using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

public class CardSelectPanelItem : MonoBehaviour
{
    [SerializeField] private bool hasSelected;

    [SerializeField] private TextMeshProUGUI NicknameText;
    [SerializeField] private TextMeshProUGUI currentSelect;

    public bool Init(Player player)
    {
        NicknameText.text = player.NickName;

        hasSelected = false;

        return false;
        // CardSelectCheckUpdate(PhotonNetwork.LocalPlayer);
    }

    public bool OnCardSelected()
    {
        if (hasSelected == true)
        {
            return true;
        }

        hasSelected = true;

        return true;
        // Debug.Log("�� ī�� ���� �Ϸ��");

        // CardSelectCheckUpdate(PhotonNetwork.LocalPlayer);
    }

    // public void CardSelectCheckUpdate(Player player)
    // {
    //     // var selectProperty = new ExitGames.Client.Photon.Hashtable();
    //     // selectProperty["Select"] = hasSelected;
    //     //
    //     // PhotonNetwork.LocalPlayer.SetCustomProperties(selectProperty);
    // }

    public void SelectCheck(Player player)
    {
        // if (player.CustomProperties.TryGetValue("Select", out object value))
        // {
        //     currentSelect.text = (bool)value ? "Select" : "Please Selcet Card";
        //
        //     currentSelect.color = (bool)value ? Color.green : Color.red;
        // }

        bool selected = InGameManager.Instance.PlayerSelection[player.ActorNumber];
        currentSelect.text = selected ? "Select" : "Please Selcet Card";

        currentSelect.color = selected ? Color.green : Color.red;
    }

    public void ResethasSelected()
    {
        hasSelected = false;
    }
}