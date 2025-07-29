using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomManager : MonoBehaviourPunCallbacks
{
    [Header("Room Code UI")]
    [SerializeField] private GameObject roomCodePanel;
    [SerializeField] private TextMeshProUGUI roomCodeText;
    [SerializeField] private Button generateCodeButton;
    [SerializeField] private Button joinRoomButton;
    
    [Header("Panel References")]
    [SerializeField] private GameObject localPanel; 
    
    private string currentRoomCode = "0000";
    private bool isInRoom = false;
    private bool hasGeneratedCode = false;
    
    private void Start()
    {
        InitializeUI();
        
        // Photon 연결
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    
    private void Update()
    {
        // ESC 키 처리
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (roomCodePanel != null && roomCodePanel.activeInHierarchy)
            {
                CloseRoomCodePanel();
            }
        }
    }
    
    /// <summary>
    /// 초기화
    /// </summary>
    private void InitializeUI()
    {

        if (roomCodeText != null) roomCodeText.text = "ROOMCODE: " + currentRoomCode;
        if (roomCodePanel != null) roomCodePanel.SetActive(false);
        
        if (generateCodeButton != null) generateCodeButton.onClick.AddListener(OnGenerateCodeClick);
        if (joinRoomButton != null) joinRoomButton.onClick.AddListener(OnJoinRoomClick);
    }
    
    #region Button Events
    
    public void OnGenerateCodeClick()
    {
        if (isInRoom)
        {
            PhotonNetwork.LeaveRoom();
            Invoke("CreateRandomRoom", 0.5f);
        }
        else
        {
            if (PhotonNetwork.IsConnected)
            {
                CreateRandomRoom();
            }
            else
            {
                PhotonNetwork.ConnectUsingSettings();
                Invoke("CreateRandomRoom", 1f);
            }
        }
        
        
        ShowRoomCodePanel();
    }
    
    public void OnJoinRoomClick()
    {
        if (isInRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            if (PopupManager.Instance)
            {
                PopupManager.Instance.ShowRoomCodeInputPopup(TryJoinRoom);
            }
        }
    }
    
    private void CloseRoomCodePanel()
    {
        if (isInRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        
        if (roomCodePanel)
        {
            roomCodePanel.SetActive(false);
        }
        
       
        if (localPanel)
        {
            localPanel.SetActive(true);
        }
    }
    
    #endregion
    
    #region Room Management
    
    private void CreateRandomRoom()
    {
        currentRoomCode = GenerateRandomRoomCode();
        
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        roomOptions.IsVisible = false;
        roomOptions.IsOpen = true;
        
        PhotonNetwork.CreateRoom(currentRoomCode, roomOptions);
    }
    
    private string GenerateRandomRoomCode()
    {
        return Random.Range(1000, 9999).ToString();
    }
    
    private void TryJoinRoom(string roomCode)
    {
        if (!PhotonNetwork.IsConnected) 
        {
            ShowMessage("서버에 연결되지 않았습니다.");
            return;
        }
        
        PhotonNetwork.JoinRoom(roomCode);
    }
    
    #endregion
    
    #region UI Management
    
    private void ShowMessage(string message)
    {
        Debug.Log(message);
        
        if (PopupManager.Instance)
        {
            PopupManager.Instance.ShowPopup(message);
        }
    }
    
    #endregion
    
    #region Photon Callbacks
    
    public override void OnCreatedRoom()
    {
        isInRoom = true;
        hasGeneratedCode = true;
        currentRoomCode = PhotonNetwork.CurrentRoom.Name;
        
        if(roomCodeText != null) 
        {
            roomCodeText.text = "ROOMCODE: " + currentRoomCode;
        }
    }
    
    public override void OnJoinedRoom()
    {
        isInRoom = true;
        currentRoomCode = PhotonNetwork.CurrentRoom.Name;
        
        if(roomCodeText != null) 
        {
            roomCodeText.text = "ROOMCODE: " + currentRoomCode;
        }
        
        ShowRoomCodePanel();
    }
    
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        ShowMessage($"방 생성 실패: {message}");
        Invoke("CreateRandomRoom", 1f);
    }
    
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        string errorMessage = "";
        switch (returnCode)
        {
            case 32758: 
                errorMessage = "해당 방이 존재하지 않습니다.";
                break;
            case 32765:  
                errorMessage = "방이 가득 찼습니다.";
                break;
            default:
                errorMessage = $"방 참가 실패: {message}";
                break;
        }
        ShowMessage(errorMessage);
    }
    
    public override void OnLeftRoom()
    {
        isInRoom = false;
        
        if (roomCodeText) 
            roomCodeText.text = "ROOMCODE: 0000";
    }
    
    #endregion
    
    // 퍼블릭 메서드
    public void ShowRoomCodePanel()
    {
        if (localPanel)
        {
            localPanel.SetActive(false);
        }
        
        if (roomCodePanel)
        {
            roomCodePanel.SetActive(true);
        }
    }
}