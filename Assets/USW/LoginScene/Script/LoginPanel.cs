using Firebase.Auth;
using Firebase.Extensions;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System;

/// <summary>
/// 로그인 패널을 관리하는 클래스
/// Firebase 인증과 Photon 네트워크 연결을 처리하고 로비 씬으로 전환합니다.
/// </summary>
public class LoginPanel : MonoBehaviourPun
{
    #region UI 컴포넌트
    [Header("Panel References")]
    [SerializeField] GameObject signUpPanel;

    [Header("Input Fields")]
    [SerializeField] TMP_InputField emailInput;
    [SerializeField] TMP_InputField passInput;

    [Header("Buttons")]
    [SerializeField] Button signUpButton;
    [SerializeField] Button loginButton;
    
    #endregion

    #region 로그인 상태 변수
    private bool isFirebaseLoggedIn = false;
    private bool isPhotonConnected = false;
    private bool isCustomPropertiesSet = false;
    #endregion
    
    #region 타임아웃 관리
    private Coroutine connectionTimeoutCoroutine;
    private const float CONNECTION_TIMEOUT = 15f;
    
    private bool isPeriodicCheckRunning = false;
    #endregion

    #region Unity 라이프사이클
    private void Awake()
    {
        SetupButtonListeners();
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void Start()
    {
        // Firebase Auth 상태 변경 이벤트 등록
        if (FirebaseManager.Auth != null)
        {
            FirebaseManager.Auth.StateChanged += OnAuthStateChanged;
        }
        
        StartCoroutine(PeriodicConnectionCheck());
    }

    private void OnDestroy()
    {
        // 이벤트 리스너 해제
        if (FirebaseManager.Auth != null)
        {
            FirebaseManager.Auth.StateChanged -= OnAuthStateChanged;
        }
        
        PhotonNetwork.RemoveCallbackTarget(this);
        
        // 진행 중인 코루틴 정리
        if (connectionTimeoutCoroutine != null)
        {
            StopCoroutine(connectionTimeoutCoroutine);
        }
    }

    private void OnEnable()
    {
        // 패널이 활성화될 때 주기적 연결 체크 시작
        if (!isPeriodicCheckRunning)
        {
            StartCoroutine(PeriodicConnectionCheck());
        }
    }
    #endregion

    #region UI 설정
    /// <summary>
    /// 버튼 이벤트 리스너를 설정합니다.
    /// </summary>
    private void SetupButtonListeners()
    {
        signUpButton.onClick.AddListener(SignUp);
        loginButton.onClick.AddListener(Login);
    }
    #endregion
    
    #region Firebase 인증 이벤트
    /// <summary>
    /// Firebase 인증 상태 변경 시 호출되는 콜백
    /// </summary>
    private void OnAuthStateChanged(object sender, EventArgs eventArgs)
    {
        FirebaseAuth auth = sender as FirebaseAuth;
        if (auth != null)
        {
            if (auth.CurrentUser == null)
            {
                // 로그아웃 시 연결 해제 및 상태 초기화
                if (PhotonNetwork.IsConnected)
                {
                    PhotonNetwork.Disconnect();
                }
                
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ClearUserInfo();
                }
                
                ResetLoginStates();
            }
        }
    }

    /// <summary>
    /// 로그인 상태를 초기화합니다.
    /// </summary>
    private void ResetLoginStates()
    {
        isFirebaseLoggedIn = false;
        isPhotonConnected = false;
        isCustomPropertiesSet = false;
        
        if (connectionTimeoutCoroutine != null)
        {
            StopCoroutine(connectionTimeoutCoroutine);
            connectionTimeoutCoroutine = null;
        }
    }
    #endregion

    #region 로그인 처리
    /// <summary>
    /// 회원가입 패널로 전환합니다.
    /// </summary>
    private void SignUp()
    {
        signUpPanel.SetActive(true);
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Firebase 로그인을 시도합니다.
    /// </summary>
    private void Login()
    {
        // 입력 검증
        if (string.IsNullOrEmpty(emailInput.text) || string.IsNullOrEmpty(passInput.text))
        {
            ShowPopup("이메일과 비밀번호를 입력해주세요.");
            return;
        }

        loginButton.interactable = false;
        ResetLoginStates();

        // Firebase 로그인 시도
        FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(emailInput.text, passInput.text)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    ShowPopup("로그인이 취소되었습니다.");
                    ResetLoginUI();
                    return;
                }
                if (task.IsFaulted)
                {
                    ShowPopup($"로그인에 실패했습니다: {task.Exception?.GetBaseException()?.Message}");
                    ResetLoginUI();
                    return;
                }
            
                // 로그인 성공
                FirebaseUser user = task.Result.User;
                
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetUserInfo(user);
                }

                isFirebaseLoggedIn = true;
                
                StartCoroutine(DelayedPhotonConnect());
            });
    }

    /// <summary>
    /// 회원가입 후 자동 로그인을 위한 메서드
    /// </summary>
    public void SetCredentialsAndLogin(string email, string password)
    {
        emailInput.text = email;
        passInput.text = password;
        
        StartCoroutine(DelayedAutoLogin());
    }

    /// <summary>
    /// 지연된 자동 로그인을 수행합니다.
    /// </summary>
    private IEnumerator DelayedAutoLogin()
    {
        yield return new WaitForSeconds(0.5f);
        Login();
    }

    /// <summary>
    /// 로그인 UI를 리셋합니다.
    /// </summary>
    private void ResetLoginUI()
    {
        loginButton.interactable = true;
        ResetLoginStates();
    }
    #endregion

    #region Photon 연결 처리
    /// <summary>
    /// 지연 후 Photon 연결을 시도합니다.
    /// </summary>
    private IEnumerator DelayedPhotonConnect()
    {
        yield return new WaitForSeconds(0.5f);
        ConnectToPhoton();
    }

    /// <summary>
    /// Photon 네트워크에 연결합니다.
    /// </summary>
    private void ConnectToPhoton()
    {
        // 기존 연결이 있다면 해제 후 재연결
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
            StartCoroutine(WaitForDisconnectAndReconnect());
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.IsLoggedIn)
        {
            // AuthValues 설정
            PhotonNetwork.AuthValues = new AuthenticationValues(GameManager.Instance.UserID);
            PhotonNetwork.NickName = GameManager.Instance.GetPhotonPlayerName();
            
            // 연결 시도
            bool connectResult = PhotonNetwork.ConnectUsingSettings();
            
            if (connectResult)
            {
                connectionTimeoutCoroutine = StartCoroutine(ConnectionTimeoutCheck());
                StartCoroutine(MonitorPhotonConnection());
            }
            else
            {
                ShowPopup("네트워크 연결에 실패했습니다.");
                ResetLoginUI();
            }
        }
        else
        {
            ShowPopup("사용자 정보가 올바르지 않습니다.");
            ResetLoginUI();
        }
    }

    /// <summary>
    /// 기존 연결 해제를 대기하고 재연결합니다.
    /// </summary>
    private IEnumerator WaitForDisconnectAndReconnect()
    {
        float waitTime = 0f;
        while (PhotonNetwork.IsConnected && waitTime < 3f)
        {
            yield return new WaitForSeconds(0.1f);
            waitTime += 0.1f;
        }
        
        if (!PhotonNetwork.IsConnected)
        {
            ConnectToPhoton();
        }
        else
        {
            ShowPopup("기존 연결을 해제할 수 없습니다.");
            ResetLoginUI();
        }
    }

    /// <summary>
    /// 연결 타임아웃을 체크합니다.
    /// </summary>
    private IEnumerator ConnectionTimeoutCheck()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < CONNECTION_TIMEOUT && !PhotonNetwork.IsConnected)
        {
            yield return new WaitForSeconds(0.5f);
            elapsedTime += 0.5f;
        }
        
        if (!PhotonNetwork.IsConnected)
        {
            ShowPopup("네트워크 연결에 시간이 너무 오래 걸립니다. 다시 시도해주세요.");
            ResetLoginUI();
        }
    }

    /// <summary>
    /// Photon 연결 상태를 모니터링합니다.
    /// </summary>
    private IEnumerator MonitorPhotonConnection()
    {
        ClientState lastState = PhotonNetwork.NetworkClientState;
        
        while (!PhotonNetwork.IsConnected && connectionTimeoutCoroutine != null)
        {
            ClientState currentState = PhotonNetwork.NetworkClientState;
            
            if (currentState != lastState)
            {
                lastState = currentState;
                
                // 연결되었지만 콜백이 호출되지 않은 경우 수동 처리
                if (currentState == ClientState.ConnectedToMasterServer && !isPhotonConnected)
                {
                    OnConnectedToMaster();
                }
            }
            
            yield return new WaitForSeconds(0.2f);
        }
    }

    /// <summary>
    /// Photon 재연결을 시도합니다.
    /// </summary>
    private IEnumerator ReconnectToPhoton()
    {
        yield return new WaitForSeconds(2f);
        
        if (isFirebaseLoggedIn && !PhotonNetwork.IsConnected)
        {
            ConnectToPhoton();
        }
    }
    #endregion

    #region Photon 커스텀 프로퍼티 설정
    /// <summary>
    /// Photon 커스텀 프로퍼티를 설정합니다.
    /// </summary>
    private void SetupPhotonCustomProperties()
    {
        if (GameManager.Instance != null && PhotonNetwork.IsConnected && PhotonNetwork.LocalPlayer != null)
        {
            // 기존 프로퍼티 정리 후 새로 설정
            var emptyProps = new ExitGames.Client.Photon.Hashtable();
            emptyProps["firebaseUID"] = null;
            emptyProps["email"] = null;
            emptyProps["displayName"] = null;
            emptyProps["loginTime"] = null;
            PhotonNetwork.LocalPlayer.SetCustomProperties(emptyProps);
            
            // 잠시 대기 후 새 프로퍼티 설정
            StartCoroutine(SetNewCustomProperties());
        }
    }

    /// <summary>
    /// 새로운 커스텀 프로퍼티를 설정합니다.
    /// </summary>
    private IEnumerator SetNewCustomProperties()
    {
        yield return new WaitForSeconds(0.3f);
        
        if (GameManager.Instance != null && PhotonNetwork.IsConnected && PhotonNetwork.LocalPlayer != null)
        {
            var customProps = GameManager.Instance.GetUserCustomProperties();
            PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
            
            StartCoroutine(WaitForCustomPropertiesSet());
        }
    }

    /// <summary>
    /// 커스텀 프로퍼티 설정 완료를 대기합니다.
    /// </summary>
    private IEnumerator WaitForCustomPropertiesSet()
    {
        float waitTime = 0f;
        const float maxWait = 8f;
        
        while (waitTime < maxWait)
        {
            if (PhotonNetwork.LocalPlayer?.CustomProperties != null && 
                PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("firebaseUID"))
            {
                string setUID = PhotonNetwork.LocalPlayer.CustomProperties["firebaseUID"]?.ToString();
                if (setUID == GameManager.Instance?.UserID)
                {
                    isCustomPropertiesSet = true;
                    CheckLoginComplete();
                    yield break;
                }
                else
                {
                    // 불일치 시 재시도
                    if (waitTime > 3f)
                    {
                        var customProps = GameManager.Instance.GetUserCustomProperties();
                        PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
                    }
                }
            }
            
            yield return new WaitForSeconds(0.2f);
            waitTime += 0.2f;
        }
        
        // 시간 초과시에도 진행
        isCustomPropertiesSet = true;
        CheckLoginComplete();
    }

    /// <summary>
    /// 지연 후 커스텀 프로퍼티를 설정합니다.
    /// </summary>
    private IEnumerator DelayedSetupCustomProperties()
    {
        yield return new WaitForSeconds(0.5f);
        SetupPhotonCustomProperties();
    }
    #endregion

    #region 로그인 완료 처리
    /// <summary>
    /// 모든 로그인 조건이 충족되었는지 확인합니다.
    /// </summary>
    private void CheckLoginComplete()
    {
        if (isFirebaseLoggedIn && isPhotonConnected && isCustomPropertiesSet)
        {
            if (connectionTimeoutCoroutine != null)
            {
                StopCoroutine(connectionTimeoutCoroutine);
                connectionTimeoutCoroutine = null;
            }
            
            // 입력 필드 초기화
            emailInput.text = "";
            passInput.text = "";
            
            StartCoroutine(CheckAllSystemsReadyAndMoveToLobby());
        }
    }

    /// <summary>
    /// 모든 시스템이 준비되었는지 확인하고 로비로 이동합니다.
    /// </summary>
    private IEnumerator CheckAllSystemsReadyAndMoveToLobby()
    {
        Debug.Log("초기화 되었는지 확인");
        // Firebase Database 초기화 대기
        yield return StartCoroutine(WaitForFirebaseDatabase());
        
        // GameManager 완전 준비 대기
        yield return StartCoroutine(WaitForGameManagerReady());
        
        // 최종 상태 확인 후 씬 전환
        if (AreAllSystemsReady())
        {
            emailInput.text = "";
            passInput.text = "";
            
            yield return new WaitForSeconds(0.5f);
            SceneManager.LoadScene("LobbyScene");
        }
        else
        {
            ShowPopup("시스템 초기화에 실패했습니다. 다시 시도해주세요.");
            ResetLoginUI();
        }
    }

    /// <summary>
    /// Firebase Database 초기화를 대기합니다.
    /// </summary>
    private IEnumerator WaitForFirebaseDatabase()
    {
        float waitTime = 0f;
        const float maxWait = 10f;
        
        while (waitTime < maxWait)
        {
            if (FirebaseManager.IsFullyInitialized())
            {
                yield break;
            }
            
            // Database가 실패해도 Auth가 준비되면 진행
            if (FirebaseManager.IsFirebaseReady && FirebaseManager.IsAuthReady)
            {
                yield break;
            }
            
            yield return new WaitForSeconds(0.1f);
            waitTime += 0.1f;
        }
    }

    /// <summary>
    /// GameManager 준비를 대기합니다.
    /// </summary>
    private IEnumerator WaitForGameManagerReady()
    {
        float waitTime = 0f;
        const float maxWait = 5f;
        
        while (waitTime < maxWait)
        {
            if (GameManager.Instance != null && 
                GameManager.Instance.IsFirebaseLoggedIn && 
                GameManager.Instance.IsPhotonConnected &&
                !string.IsNullOrEmpty(GameManager.Instance.UserID))
            {
                yield break;
            }
            
            yield return new WaitForSeconds(0.1f);
            waitTime += 0.1f;
        }
    }

    /// <summary>
    /// 모든 시스템이 준비되었는지 확인합니다.
    /// </summary>
    private bool AreAllSystemsReady()
    {
        // 필수 조건들
        bool gameManagerReady = GameManager.Instance != null && 
                               GameManager.Instance.IsFirebaseLoggedIn && 
                               GameManager.Instance.IsPhotonConnected &&
                               !string.IsNullOrEmpty(GameManager.Instance.UserID);
        
        bool firebaseReady = FirebaseManager.IsFirebaseReady && FirebaseManager.IsAuthReady;
        
        bool loginStatesReady = isFirebaseLoggedIn && isPhotonConnected && isCustomPropertiesSet;
        
        return gameManagerReady && firebaseReady && loginStatesReady;
    }
    #endregion

    #region 주기적 연결 체크
    /// <summary>
    /// 주기적으로 연결 상태를 체크합니다.
    /// </summary>
    private IEnumerator PeriodicConnectionCheck()
    {
        isPeriodicCheckRunning = true;
    
        while (true)
        {
            yield return new WaitForSeconds(2f);
        
            if (isFirebaseLoggedIn && !isPhotonConnected)
            {
                if (PhotonNetwork.IsConnected && PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
                {
                    OnConnectedToMaster();
                    break;
                }
            }
        
            if (isFirebaseLoggedIn && isPhotonConnected && isCustomPropertiesSet)
            {
                break;
            }
        }
    
        isPeriodicCheckRunning = false;
    }
    #endregion

    #region Photon 콜백 메서드
    /// <summary>
    /// Photon 마스터 서버 연결 완료 시 호출
    /// </summary>
    public void OnConnectedToMaster()
    {
        isPhotonConnected = true;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPhotonConnectionStatus(true);
            
            // 연결 직후 강제 동기화
            GameManager.Instance.ForceSyncFirebasePhoton();
        }
        
        StartCoroutine(DelayedSetupCustomProperties());
    }

    /// <summary>
    /// Photon 연결 해제 시 호출
    /// </summary>
    public void OnDisconnected(DisconnectCause cause)
    {
        isPhotonConnected = false;
        isCustomPropertiesSet = false;
        
        if (connectionTimeoutCoroutine != null)
        {
            StopCoroutine(connectionTimeoutCoroutine);
            connectionTimeoutCoroutine = null;
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPhotonConnectionStatus(false);
        }
        
        // 자동 로그아웃이 아닌 경우 재연결 시도
        if (isFirebaseLoggedIn && cause != DisconnectCause.DisconnectByClientLogic)
        {
            StartCoroutine(ReconnectToPhoton());
        }
    }

    /// <summary>
    /// Photon 연결 실패 시 호출
    /// </summary>
    public void OnConnectFailed()
    {
        ShowPopup("네트워크 연결에 실패했습니다. 인터넷 연결을 확인해주세요.");
        ResetLoginUI();
    }

    /// <summary>
    /// 지역 목록 수신 시 호출
    /// </summary>
    public void OnRegionListReceived(RegionHandler regionHandler)
    {
        // 지역 목록 수신 처리 (필요시 구현)
    }
    
    /// <summary>
    /// 커스텀 인증 실패 시 호출
    /// </summary>
    public void OnCustomAuthenticationFailed(string debugMessage)
    {
        ShowPopup("사용자 인증에 실패했습니다.");
        ResetLoginUI();
    }
    #endregion

    #region UI 피드백
    /// <summary>
    /// 팝업 메시지를 표시합니다.
    /// </summary>
    private void ShowPopup(string message)
    {
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.ShowPopup(message);
        }
    }
    #endregion
}