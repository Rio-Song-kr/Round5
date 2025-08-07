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

    public void Init(Player player)
    {
        NicknameText.text = player.NickName;

        hasSelected = false;

        CardSelectCheckUpdate(PhotonNetwork.LocalPlayer);
    }

    public void OnCardSelected()
    {
        if (hasSelected == true)
        {
            return;
        }

        hasSelected = true;
        // Debug.Log("�� ī�� ���� �Ϸ��");

        CardSelectCheckUpdate(PhotonNetwork.LocalPlayer);
    }

    public void CardSelectCheckUpdate(Player player)
    {
        var selectProperty = new ExitGames.Client.Photon.Hashtable();
        selectProperty["Select"] = hasSelected;

        PhotonNetwork.LocalPlayer.SetCustomProperties(selectProperty);
    }

    public void SelectCheck(Player player)
    {
        if (player.CustomProperties.TryGetValue("Select", out object value))
        {
            currentSelect.text = (bool)value ? "Select" : "Please Selcet Card";

            currentSelect.color = (bool)value ? Color.green : Color.red;
        }
    }

    public void ResethasSelected()
    {
        hasSelected = false;
    }
}