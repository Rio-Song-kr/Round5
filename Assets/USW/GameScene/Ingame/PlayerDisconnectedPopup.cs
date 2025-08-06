using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerDisconnectedPopup : MonoBehaviour
{
    [SerializeField] private GameObject popupPanel;

    [SerializeField] private Button confirmButton;
    
    private void OnEnable()
    {
        InGameManager.OnPlayerDisconnected += ShowDisconnectedPopup;
    }

    private void OnDisable()
    {
        InGameManager.OnPlayerDisconnected -= ShowDisconnectedPopup;
    }

    private void Awake()
    {
        confirmButton.onClick.AddListener(OnConfirmClick);
        popupPanel.SetActive(false);
    }

    void ShowDisconnectedPopup()
    {
        popupPanel.SetActive(true);
        
        // 게임 일시정지 
        Time.timeScale = 0;
    }

    void OnConfirmClick()
    {
        StartCoroutine(DisconnectAndGoToLobby());
    }

    private IEnumerator DisconnectAndGoToLobby()
    {
        PhotonNetwork.Disconnect();

        while (PhotonNetwork.IsConnected)
        {
            yield return null;
        }

        SceneManager.LoadScene("LobbyScene");
    }
}
