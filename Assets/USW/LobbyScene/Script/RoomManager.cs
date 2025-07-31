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
    private bool isLoadingScene = false;
    
    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true; 
        
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
        if (isLoadingScene) return; // 혹시나 로딩중이면 버튼 비활성화 
        
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
        if (isLoadingScene) return; 
        
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
        if (isLoadingScene) return;
        
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
    
    /// <summary>
    /// 플레이어 수 체크 및 게임 시작
    /// </summary>
    private void CheckPlayersAndStartGame()
    {
        if (!PhotonNetwork.IsMasterClient || isLoadingScene) return;
        
        int currentPlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        
        if (currentPlayerCount >= 2)
        {
            // 2명이 모두 들어왔을 때 게임 시작
            StartCoroutine(StartGameSequence());
        }
    }
    
    /// <summary>
    /// 게임 시작 순서
    /// </summary>
    private IEnumerator StartGameSequence()
    {
        isLoadingScene = true;
        
        // 버튼들 비활성화
        if (generateCodeButton != null) generateCodeButton.interactable = false;
        if (joinRoomButton != null) joinRoomButton.interactable = false;
        
        yield return new WaitForSeconds(1f);
        
        // 모든 클라이언트가 함께 씬 로드
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("TempLoadingScene"); // 비동기 로딩 씬으로 이동
        }
    }
    
    #endregion
    
    #region UI Management
    
    private void ShowMessage(string message)
    {
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
        
        if(roomCodeText) 
        {
            roomCodeText.text = "ROOMCODE: " + currentRoomCode;
        }
    }
    
    public override void OnJoinedRoom()
    {
        isInRoom = true;
        currentRoomCode = PhotonNetwork.CurrentRoom.Name;
        
        if(roomCodeText) 
        {
            roomCodeText.text = "ROOMCODE: " + currentRoomCode;
        }
        
        ShowRoomCodePanel();
        CheckPlayersAndStartGame();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        CheckPlayersAndStartGame();
    }

    // 플레이어가 랜뽑했을때
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        isLoadingScene = false;
        
        if(generateCodeButton) generateCodeButton.interactable = true;
        if(joinRoomButton) joinRoomButton.interactable = true;
  
        
        CheckPlayersAndStartGame();
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
        
        if (generateCodeButton) generateCodeButton.interactable = true;
        if (joinRoomButton) joinRoomButton.interactable = true;
        
     
        
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