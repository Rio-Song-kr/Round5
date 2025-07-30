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
        if (FirebaseManager.Auth != null)
        {
            FirebaseManager.Auth.StateChanged += OnAuthStateChanged;
        }
        
        StartCoroutine(PeriodicConnectionCheck());
    }

    private void OnDestroy()
    {
        if (FirebaseManager.Auth != null)
        {
            FirebaseManager.Auth.StateChanged -= OnAuthStateChanged;
        }
        
        PhotonNetwork.RemoveCallbackTarget(this);
        
        if (connectionTimeoutCoroutine != null)
        {
            StopCoroutine(connectionTimeoutCoroutine);
        }
    }

    private void OnEnable()
    {
        if (!isPeriodicCheckRunning)
        {
            StartCoroutine(PeriodicConnectionCheck());
        }
    }
    #endregion

    #region UI 설정
    private void SetupButtonListeners()
    {
        signUpButton.onClick.AddListener(SignUp);
        loginButton.onClick.AddListener(Login);
    }
    #endregion
    
    #region Firebase 인증 이벤트
    private void OnAuthStateChanged(object sender, EventArgs eventArgs)
    {
        FirebaseAuth auth = sender as FirebaseAuth;
        if (auth != null)
        {
            if (auth.CurrentUser == null)
            {
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
    private void SignUp()
    {
        signUpPanel.SetActive(true);
        gameObject.SetActive(false);
    }

    private void Login()
    {
        if (string.IsNullOrEmpty(emailInput.text) || string.IsNullOrEmpty(passInput.text))
        {
            ShowPopup("이메일과 비밀번호를 입력해주세요.");
            return;
        }

        loginButton.interactable = false;
        ResetLoginStates();

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
            
                FirebaseUser user = task.Result.User;
                
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetUserInfo(user);
                }

                isFirebaseLoggedIn = true;
                
                StartCoroutine(DelayedForceCheck());
                
                StartCoroutine(DelayedPhotonConnect());
            });
    }

    public void SetCredentialsAndLogin(string email, string password)
    {
        emailInput.text = email;
        passInput.text = password;
        
        StartCoroutine(DelayedAutoLogin());
    }

    private IEnumerator DelayedAutoLogin()
    {
        yield return new WaitForSeconds(0.5f);
        Login();
    }

    private void ResetLoginUI()
    {
        loginButton.interactable = true;
        ResetLoginStates();
    }
    #endregion

    #region Photon 연결 처리
    private IEnumerator DelayedPhotonConnect()
    {
        yield return new WaitForSeconds(0.5f);
        ConnectToPhoton();
    }

    private void ConnectToPhoton()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
            StartCoroutine(WaitForDisconnectAndReconnect());
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.IsLoggedIn)
        {
            PhotonNetwork.AuthValues = new AuthenticationValues(GameManager.Instance.UserID);
            PhotonNetwork.NickName = GameManager.Instance.GetPhotonPlayerName();
            
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

    private IEnumerator MonitorPhotonConnection()
    {
        ClientState lastState = PhotonNetwork.NetworkClientState;
        
        while (!PhotonNetwork.IsConnected && connectionTimeoutCoroutine != null)
        {
            ClientState currentState = PhotonNetwork.NetworkClientState;
            
            if (currentState != lastState)
            {
                lastState = currentState;
                
                if (currentState == ClientState.ConnectedToMasterServer && !isPhotonConnected)
                {
                    OnConnectedToMaster();
                }
            }
            
            yield return new WaitForSeconds(0.2f);
        }
    }

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
    private void SetupPhotonCustomProperties()
    {
        if (GameManager.Instance != null && PhotonNetwork.IsConnected && PhotonNetwork.LocalPlayer != null)
        {
            var emptyProps = new ExitGames.Client.Photon.Hashtable();
            emptyProps["firebaseUID"] = null;
            emptyProps["email"] = null;
            emptyProps["displayName"] = null;
            emptyProps["loginTime"] = null;
            PhotonNetwork.LocalPlayer.SetCustomProperties(emptyProps);
            
            StartCoroutine(SetNewCustomProperties());
        }
    }

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
                string expectedUID = GameManager.Instance?.UserID;
                
                if (setUID == expectedUID)
                {
                    isCustomPropertiesSet = true;
                    CheckLoginComplete();
                    yield break;
                }
                else
                {
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
        
        isCustomPropertiesSet = true;
        CheckLoginComplete();
    }

    private IEnumerator DelayedSetupCustomProperties()
    {
        yield return new WaitForSeconds(0.5f);
        SetupPhotonCustomProperties();
    }
    #endregion

    #region 로그인 완료 처리 - 5초 재체크 방식
    private bool systemCheckResult = false;

    private void CheckLoginComplete()
    {
        if (isFirebaseLoggedIn && isPhotonConnected && isCustomPropertiesSet)
        {
            if (connectionTimeoutCoroutine != null)
            {
                StopCoroutine(connectionTimeoutCoroutine);
                connectionTimeoutCoroutine = null;
            }
            
            emailInput.text = "";
            passInput.text = "";
            
            StartCoroutine(CheckAllSystemsReadyAndMoveToLobby());
        }
    }

    private IEnumerator CheckAllSystemsReadyAndMoveToLobby()
    {
        // 첫 번째 체크 시도
        yield return StartCoroutine(AttemptSystemCheck());
        
        if (systemCheckResult)
        {
            yield return MoveToLobby();
            yield break;
        }
        
        // 5초 대기 후 재시도
        yield return new WaitForSeconds(5f);
        
        yield return StartCoroutine(AttemptSystemCheck());
        
        if (systemCheckResult)
        {
            yield return MoveToLobby();
        }
        else
        {
            ShowPopup("시스템 초기화에 실패했습니다. 다시 시도해주세요.");
            ResetLoginUI();
        }
    }

    private IEnumerator AttemptSystemCheck()
    {
        systemCheckResult = false;
        
        // Firebase Database 초기화 확인
        float elapsed = 0f;
        while (elapsed < 3f && !FirebaseManager.IsFullyInitialized())
        {
            if (FirebaseManager.IsFirebaseReady && FirebaseManager.IsAuthReady)
            {
                break;
            }
        
            yield return new WaitForSeconds(0.2f);
            elapsed += 0.2f;
        }
    
        // GameManager 상태 확인
        elapsed = 0f;
        while (elapsed < 2f)
        {
            if (GameManager.Instance != null && 
                GameManager.Instance.IsFirebaseLoggedIn && 
                GameManager.Instance.IsPhotonConnected &&
                !string.IsNullOrEmpty(GameManager.Instance.UserID))
            {
                systemCheckResult = true;
                yield break;
            }
        
            yield return new WaitForSeconds(0.2f);
            elapsed += 0.2f;
        }
    }
    
    private IEnumerator MoveToLobby()
    {
        emailInput.text = "";
        passInput.text = "";
    
        yield return new WaitForSeconds(0.5f);
        
        SceneManager.LoadScene("LobbyScene");
    }
    #endregion

    #region 주기적 연결 체크
     private IEnumerator PeriodicConnectionCheck()
    {
        isPeriodicCheckRunning = true;
        
        int checkCount = 0;
        const int maxChecks = 60; 
    
        while (checkCount < maxChecks)
        {
            checkCount++;
            
            yield return new WaitForSeconds(2f);
            
            // Firebase 로그인은 됐지만 Photon 연결이 안된 경우
            if (isFirebaseLoggedIn && !isPhotonConnected)
            {
                if (PhotonNetwork.IsConnected && PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
                {
                    OnConnectedToMaster();
                    continue; // 다시 체크
                }
            }
            
            // Photon은 연결됐지만 프로퍼티가 설정 안된 경우
            if (isFirebaseLoggedIn && isPhotonConnected && !isCustomPropertiesSet)
            {
                SetupPhotonCustomProperties();
                continue;
            }
        
            // 모든 조건이 만족된 경우
            if (isFirebaseLoggedIn && isPhotonConnected && isCustomPropertiesSet)
            {
                break;
            }
            
            // Firebase 로그인됐지만 너무 오래 걸리는 경우 강제 체크
            if (isFirebaseLoggedIn && checkCount > 15) // 30초 후
            {
                CheckLoginComplete();
                break;
            }
        }
        
        if (checkCount >= maxChecks)
        {
            if (isFirebaseLoggedIn)
            {
                ShowPopup("네트워크 연결에 문제가 있습니다. 다시 시도해주세요.");
                ResetLoginUI();
            }
        }
    
        isPeriodicCheckRunning = false;
    }
    #endregion

    #region Photon 콜백 메서드
    public void OnConnectedToMaster()
    {
        isPhotonConnected = true;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPhotonConnectionStatus(true);
            GameManager.Instance.ForceSyncFirebasePhoton();
        }
        
        StartCoroutine(DelayedSetupCustomProperties());
    }

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
        
        if (isFirebaseLoggedIn && cause != DisconnectCause.DisconnectByClientLogic)
        {
            StartCoroutine(ReconnectToPhoton());
        }
    }

    public void OnConnectFailed()
    {
        ShowPopup("네트워크 연결에 실패했습니다. 인터넷 연결을 확인해주세요.");
        ResetLoginUI();
    }
    
    public void OnCustomAuthenticationFailed(string debugMessage)
    {
        ShowPopup("사용자 인증에 실패했습니다.");
        ResetLoginUI();
    }
    #endregion

    #region UI 피드백
    private void ShowPopup(string message)
    {
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.ShowPopup(message);
        }
    }
    #endregion
    
    #region Photon 콜백 문제 보완
    /// <summary>
    /// Photon 연결 상태를 강제로 체크하고 필요시 수동 처리
    /// </summary>
    private void ForceCheckPhotonConnection()
    {
        // Photon은 연결되어 있지만 콜백이 호출되지 않은 경우
        if (PhotonNetwork.IsConnected && 
            PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer && 
            !isPhotonConnected)
        {
            OnConnectedToMaster();
        }
        
        // 모든 조건이 만족되었는데 CheckLoginComplete가 호출되지 않은 경우
        if (isFirebaseLoggedIn && isPhotonConnected && isCustomPropertiesSet)
        {
            CheckLoginComplete();
        }
    }
    
    /// <summary>
    /// 로그인 후 일정 시간 후 강제 상태 체크
    /// </summary>
    private IEnumerator DelayedForceCheck()
    {
        yield return new WaitForSeconds(10f);
        
        if (isFirebaseLoggedIn && (!isPhotonConnected || !isCustomPropertiesSet))
        {
            ForceCheckPhotonConnection();
            
            yield return new WaitForSeconds(3f);
            
            if (isFirebaseLoggedIn && isPhotonConnected && isCustomPropertiesSet)
            {
                CheckLoginComplete();
            }
            else
            {
                StartCoroutine(CheckAllSystemsReadyAndMoveToLobby());
            }
        }
    }
    #endregion
}