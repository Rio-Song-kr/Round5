using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Random = UnityEngine.Random;

public class SinglePanel : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject singlePanel;
    [SerializeField] private Button backButton;
    [SerializeField] private Button weaponSceneButton;
    [SerializeField] private Button ropeSceneButton;
    [SerializeField] private Animator singleAnimator;
    [SerializeField] private Animator mainMenuAnimator;
    [SerializeField] private float animationLength = 1f;
    
    private bool isLoadingSinglePlayer = false;
    private string pendingSceneName = "";

    private void Start()
    {
        
        PhotonNetwork.AutomaticallySyncScene = false;
        Init();
        PhotonNetwork.AutomaticallySyncScene = false;
    }

    void Init()
    {
        if (backButton)
        {
            backButton.onClick.AddListener(OnBackButtonClick);
        }

        if (ropeSceneButton)
        {
            ropeSceneButton.onClick.AddListener(OnRopeButtonClick);
        }

        if (weaponSceneButton)
        {
            weaponSceneButton.onClick.AddListener(OnWeaponButtonClick);
        }
    }

    public void OnBackButtonClick()
    {
        if (PhotonNetwork.InRoom)
        {
            string roomName = PhotonNetwork.CurrentRoom.Name;
            if (roomName.StartsWith("SM_"))
            {
                PhotonNetwork.LeaveRoom();
            }
        }

        if (singleAnimator)
        {
            singleAnimator.SetTrigger("SingleButton_BackTrigger");
        }

        if (mainMenuAnimator)
        {
            mainMenuAnimator.SetTrigger("PlayWelcomeAgain");
        }
    }
    
    void OnBackButtonClicked()
    {
        if (singlePanel)
        {
            singlePanel.SetActive(false);
        }
    }

    public void OnRopeButtonClick()
    {
        StartCoroutine(CreateSinglePlayerRoomAndLoadScene("USW_RopePlayMode"));
    }

    public void OnWeaponButtonClick()
    {
        StartCoroutine(CreateSinglePlayerRoomAndLoadScene("KDJ_WeaponTestScene"));
    }

    /// <summary>
    /// 싱글플레이어 방 생성 후 씬 로드
    /// </summary>
    IEnumerator CreateSinglePlayerRoomAndLoadScene(string sceneName)
    {
        isLoadingSinglePlayer = true;
        pendingSceneName = sceneName;

        if (singleAnimator)
        {
            singleAnimator.SetTrigger("SingleButton_SceneTrigger");
        }
        
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }

       
        yield return new WaitForSeconds(0.5f);
        
        TryCreateSinglePlayerRoom();

        yield return new WaitForSeconds(animationLength);
    }

    /// <summary>
    /// 싱글플레이어 전용 방 생성
    /// </summary>
    private void CreateSinglePlayerRoom()
    {
        string singlePlayerRoomName = "SM_" + Random.Range(100000, 999999).ToString();
        
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 1; 
        roomOptions.IsVisible = false; 
        roomOptions.IsOpen = false; 
        
        // 싱글플레이어 식별용 커스텀 프로퍼티
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props["SinglePlayer"] = true;
        props["SceneName"] = pendingSceneName;
        roomOptions.CustomRoomProperties = props;

        PhotonNetwork.CreateRoom(singlePlayerRoomName, roomOptions);
    }

    /// <summary>
    /// 퍼블릭 메서드
    /// </summary>
    public void ShowSinglePanel()
    {
        if (singlePanel)
        {
            singlePanel.SetActive(true);
        }
    }

    private bool CanCreateRoom()
    {
        return PhotonNetwork.IsConnectedAndReady && 
               !PhotonNetwork.InRoom && 
               (PhotonNetwork.InLobby || PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.ConnectedToMasterServer);
    }

    void TryCreateSinglePlayerRoom()
    {
        if (CanCreateRoom())
        {
            CreateSinglePlayerRoom();
        }
    }
    
    #region Photon Callbacks

    public override void OnCreatedRoom()
    {
        
    }

    public override void OnJoinedRoom()
    {
        if (isLoadingSinglePlayer && PhotonNetwork.CurrentRoom.Name.StartsWith("SM_"))
        {
            StartCoroutine(LoadSceneAfterDelay());
        }
    }
    
    IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        
        // 연결 상태 재확인
        if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient && !string.IsNullOrEmpty(pendingSceneName))
        {
            SceneManager.LoadScene(pendingSceneName);
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        if (isLoadingSinglePlayer)
        {
            Invoke("CreateSinglePlayerRoom", 1f);
        }
    }

    public override void OnLeftRoom()
    {
        isLoadingSinglePlayer = false;
        pendingSceneName = "";
    }

    public override void OnConnectedToMaster()
    {
        if (isLoadingSinglePlayer)
        {
            TryCreateSinglePlayerRoom();
        }
    }

    #endregion
}