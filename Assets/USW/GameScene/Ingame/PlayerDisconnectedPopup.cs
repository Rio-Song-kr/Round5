using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerDisconnectedPopup : MonoBehaviourPunCallbacks
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
        Time.timeScale = 1;
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
                
        //카드 상태 초기화.
        CardManager.Instance.ClearLists();

        SceneManager.LoadScene("USW/LobbyScene/LobbyScene");
    }
}
