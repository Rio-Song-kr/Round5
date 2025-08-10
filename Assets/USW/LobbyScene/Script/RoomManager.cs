using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviourPunCallbacks
{
    [Header("Room Code UI")]
    [SerializeField] private GameObject roomCodePanel;
    [SerializeField] private TextMeshProUGUI roomCodeText;
    [SerializeField] private Button generateCodeButton;
    [SerializeField] private Button joinRoomButton;

    [Header("Quick Match UI")]
    [SerializeField] private Button quickMatchButton;
    [SerializeField] private GameObject quickMatchPanel;
    [SerializeField] private TextMeshProUGUI quickMatchStatusText;

    [Header("Panel References")]
    [SerializeField] private GameObject localPanel;
    [SerializeField] private GameObject singlePanel;

    private LoadingTextAnimation statusTextAnimation;

    private string currentRoomCode = "0000";
    private bool isInRoom = false;
    private bool isLoadingScene = false;
    private bool isQuickMatching = false;

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        InitializeUI();

        if (quickMatchStatusText != null)
        {
            statusTextAnimation = quickMatchStatusText.GetComponent<LoadingTextAnimation>();
        }

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
            if (isQuickMatching)
            {
                CancelQuickMatch();
            }
            else if (roomCodePanel != null && roomCodePanel.activeInHierarchy)
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
        if (quickMatchPanel != null) quickMatchPanel.SetActive(false);

        if (generateCodeButton != null) generateCodeButton.onClick.AddListener(OnGenerateCodeClick);
        if (joinRoomButton != null) joinRoomButton.onClick.AddListener(OnJoinRoomClick);
        if (quickMatchButton != null) quickMatchButton.onClick.AddListener(OnQuickMatchClick);
    }

    #region Button Events

    public void OnGenerateCodeClick()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        Debug.Log($"Automatically SyncScene : {PhotonNetwork.AutomaticallySyncScene}");

        if (isLoadingScene || isQuickMatching) return;

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
        PhotonNetwork.AutomaticallySyncScene = true;

        if (isLoadingScene || isQuickMatching) return;

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

    /// <summary>
    /// 퀵매칭 버튼 클릭
    /// </summary>
    public void OnQuickMatchClick()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        if (isLoadingScene || isQuickMatching) return;

        if (isInRoom)
        {
            PhotonNetwork.LeaveRoom();
            Invoke("StartQuickMatch", 0.5f);
        }
        else
        {
            if (PhotonNetwork.IsConnected)
            {
                StartQuickMatch();
            }
            else
            {
                PhotonNetwork.ConnectUsingSettings();
                Invoke("StartQuickMatch", 1f);
            }
        }
    }

    private void CloseRoomCodePanel()
    {
        if (isLoadingScene) return;

        if (roomCodePanel)
        {
            roomCodePanel.SetActive(false);
        }

        var props = new ExitGames.Client.Photon.Hashtable();
        props = PhotonNetwork.CurrentRoom.CustomProperties;

        if (props.ContainsKey("SinglePlayer") && (bool)props["SinglePlayer"])
        {
            if (singlePanel)
            {
                singlePanel.SetActive(true);
            }
        }
        else
        {
            if (localPanel)
            {
                localPanel.SetActive(true);
            }
        }

        if (isInRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    #endregion

    #region Quick Match

    /// <summary>
    /// 퀵매칭 시작
    /// </summary>
    private void StartQuickMatch()
    {
        isQuickMatching = true;

        // 퀵매칭 패널로 전환
        ShowQuickMatchPanel(true);
        UpdateQuickMatchStatus("WAIT FOR THE CHALLANGER");

        // 퀵매칭 전용 방만 찾기 위한 필터 설정
        var expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
        expectedCustomRoomProperties["QuickMatch"] = true;

        // 퀵매칭 방만 랜덤 참가 시도
        PhotonNetwork.JoinRandomRoom(expectedCustomRoomProperties, 2);
    }

    /// <summary>
    /// 퀵매칭 취소
    /// </summary>
    private void CancelQuickMatch()
    {
        if (!isQuickMatching) return;

        isQuickMatching = false;

        if (isInRoom)
        {
            PhotonNetwork.LeaveRoom();
        }

        // 메인 패널로 돌아가기
        ShowQuickMatchPanel(false);
        UpdateQuickMatchStatus("");
    }

    /// <summary>
    /// 퀵매칭용 방 생성 (랜덤 매칭 실패시)
    /// </summary>
    private void CreateQuickMatchRoom()
    {
        string quickMatchRoomName = "QM_" + Random.Range(100000, 999999).ToString();

        var roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;

        // 퀵매칭 식별하기 위한 커스텀 프로퍼티 
        var props = new ExitGames.Client.Photon.Hashtable();
        props["QuickMatch"] = true;
        roomOptions.CustomRoomProperties = props;
        roomOptions.CustomRoomPropertiesForLobby = new string[] { "QuickMatch" };

        PhotonNetwork.CreateRoom(quickMatchRoomName, roomOptions);
        UpdateQuickMatchStatus("CREATING ROOM");
    }

    /// <summary>
    /// 퀵매칭 패널 표시/숨김
    /// </summary>
    private void ShowQuickMatchPanel(bool show)
    {
        if (quickMatchPanel != null)
        {
            quickMatchPanel.SetActive(show);
        }

        if (localPanel != null)
        {
            localPanel.SetActive(!show);
        }
    }

    /// <summary>
    /// 퀵매칭 상태 텍스트 업데이트
    /// </summary>
    private void UpdateQuickMatchStatus(string status)
    {
        if (quickMatchStatusText != null)
        {
            quickMatchStatusText.text = status;
        }
    }

    #endregion

    #region Room Management

    private void CreateRandomRoom()
    {
        currentRoomCode = GenerateRandomRoomCode();

        var roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        roomOptions.IsVisible = false;
        roomOptions.IsOpen = true;
        // if (PhotonNetwork.InRoom)
        // {
        //     PhotonNetwork.LeaveRoom();
        // }

        PhotonNetwork.CreateRoom(currentRoomCode, roomOptions);
    }

    private string GenerateRandomRoomCode() => Random.Range(1000, 9999).ToString();

    private void TryJoinRoom(string roomCode)
    {
        PhotonNetwork.JoinRoom(roomCode);
    }

    /// <summary>
    /// 플레이어 수 체크 및 게임 시작
    /// </summary>
    private void CheckPlayersAndStartGame()
    {
        if (!PhotonNetwork.IsMasterClient || isLoadingScene) return;

        int currentPlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;

        var props = new ExitGames.Client.Photon.Hashtable();
        props = PhotonNetwork.CurrentRoom.CustomProperties;

        if (props.ContainsKey("SinglePlayer") && (bool)props["SinglePlayer"])
        {
            if (PhotonNetwork.CurrentRoom.Name.StartsWith("SM_"))
            {
                StartCoroutine(LoadSceneAfterDelay());
            }
        }
        else
        {
            if (currentPlayerCount >= 2)
            {
                StartCoroutine(StartGameSequence());
            }
        }
    }

    /// <summary>
    /// 게임 시작 순서
    /// </summary>
    private IEnumerator StartGameSequence()
    {
        isLoadingScene = true;

        yield return new WaitForSeconds(1f);


        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("TempLoadingScene");
        }
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        var panel = singlePanel.GetComponent<SinglePanel>();

        yield return new WaitForSeconds(1f);

        // 연결 상태 재확인
        if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient && !string.IsNullOrEmpty(panel.PendingSceneName))
        {
            SceneManager.LoadScene(panel.PendingSceneName);
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

        if (isQuickMatching)
        {
            UpdateQuickMatchStatus("WAIT FOR THE CHALLANGER");
        }
        else
        {
            currentRoomCode = PhotonNetwork.CurrentRoom.Name;

            if (roomCodeText)
            {
                roomCodeText.text = "ROOMCODE: " + currentRoomCode;
            }
        }
    }

    public override void OnJoinedRoom()
    {
        isInRoom = true;
        isLoadingScene = false;

        if (isQuickMatching)
        {
            int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;

            if (playerCount >= 2)
            {
                UpdateQuickMatchStatus("MATCH FOUND!");


                Invoke("HideQuickMatchUI", 1f);
            }
        }
        else
        {
            // Private Room 입장
            currentRoomCode = PhotonNetwork.CurrentRoom.Name;

            if (roomCodeText)
            {
                roomCodeText.text = "ROOMCODE: " + currentRoomCode;
            }

            ShowRoomCodePanel();
        }

        CheckPlayersAndStartGame();
    }

    /// <summary>
    /// 퀵매칭 UI 숨기기
    /// </summary>
    private void HideQuickMatchUI()
    {
        if (isQuickMatching)
        {
            isQuickMatching = false;
            ShowQuickMatchPanel(false);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (isQuickMatching)
        {
            UpdateQuickMatchStatus("MATCH FOUND!");

            Invoke("HideQuickMatchUI", 1f);
        }

        CheckPlayersAndStartGame();
    }

    // 플레이어가 나갔을때
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        isLoadingScene = false;

        if (isQuickMatching)
        {
            UpdateQuickMatchStatus("WAIT FOR THE CHALLANGER");
        }

        CheckPlayersAndStartGame();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        if (isQuickMatching)
        {
            CancelQuickMatch();
        }
        else
        {
            Invoke("CreateRandomRoom", 1f);
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (isQuickMatching)
        {
            CancelQuickMatch();
        }
        else
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
    }

    /// <summary>
    /// 랜덤 방 참가 실패시 호출 (퀵매칭용)
    /// </summary>
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        if (isQuickMatching)
        {
            UpdateQuickMatchStatus("CREATING ROOM");
            CreateQuickMatchRoom();
        }
    }

    public override void OnLeftRoom()
    {
        isInRoom = false;
        isLoadingScene = false;

        if (isQuickMatching)
        {
            ShowQuickMatchPanel(false);
            isQuickMatching = false;
        }

        if (roomCodeText)
            roomCodeText.text = "ROOMCODE: 0000";
    }

    #endregion

    // 퍼블릭 메서드
    public void ShowRoomCodePanel()
    {
        var props = new ExitGames.Client.Photon.Hashtable();
        props = PhotonNetwork.CurrentRoom?.CustomProperties;

        if (props != null && props.ContainsKey("SinglePlayer") && (bool)props["SinglePlayer"])
        {
            if (singlePanel)
            {
                singlePanel.SetActive(false);
            }
        }
        else
        {
            if (localPanel)
            {
                localPanel.SetActive(false);
            }
        }

        if (roomCodePanel)
        {
            roomCodePanel.SetActive(true);
        }
    }
}