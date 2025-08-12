using System;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
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

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            ItsFreakinHardToCreateNewVoidName();
        }
    }

    void ItsFreakinHardToCreateNewVoidName()
    {
        CardManager.Instance.ClearLists();

        SceneManager.LoadScene("USW/LobbyScene/LobbyScene");
    }

    public override void OnLeftRoom()
    {
        ItsFreakinHardToCreateNewVoidName();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (popupPanel.activeSelf)
        {
            Time.timeScale = 1;
            ItsFreakinHardToCreateNewVoidName();
        }
    }
}