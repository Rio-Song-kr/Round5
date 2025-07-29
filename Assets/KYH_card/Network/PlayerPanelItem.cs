using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPanelItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private TextMeshProUGUI readyText;
    [SerializeField] private Image HostImage;
    [SerializeField] private Image readyButtonImage;
    [SerializeField] private Button ReadyButton;

    private bool isReady;
    public void Init(Player player)
    {
        nicknameText.text = player.NickName;

        HostImage.enabled = player.IsMasterClient;
        ReadyButton.interactable = player.IsLocal;

        if (!player.IsLocal)
        {
            return;
        }

        isReady = false;

        ReadyPropertyUpdate(PhotonNetwork.LocalPlayer);

        ReadyButton.onClick.RemoveListener(ReadyButtonClick);
        ReadyButton.onClick.AddListener(ReadyButtonClick);
    }

    public void ReadyButtonClick()
    {
        isReady = !isReady;

        readyText.text = isReady ? "Ready" : "Click Ready";

        readyButtonImage.color = isReady ? Color.green : Color.cyan;



        ReadyPropertyUpdate(PhotonNetwork.LocalPlayer);
    }

    public void ReadyPropertyUpdate(Player player)
    {
        ExitGames.Client.Photon.Hashtable playerProperty = new ExitGames.Client.Photon.Hashtable();
        playerProperty["Ready"] = isReady;
        if (isReady == true)
        {
            Debug.Log($"{player.NickName} 이 준비 완료됨");
        }


        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperty);
    }

    public void ReadyCheck(Player player)
    {
        if (player.CustomProperties.TryGetValue("Ready", out object value))
        {
            readyText.text = (bool)value ? "Ready" : "Click Ready";

            readyButtonImage.color = (bool)value ? Color.green : Color.cyan;
        }
    }
}
