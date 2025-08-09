using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

/// <summary>
/// 게임 전체의 사용자 인증 및 연결 상태를 관리하는 싱글톤 매니저
/// Firebase 인증과 Photon 네트워크 연결을 동기화하고 사용자 정보를 관리합니다.
/// </summary>
public class GameManager : MonoBehaviour
{
    #region 싱글톤
    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }
    #endregion

    #region 사용자 정보 프로퍼티
    public string UserID { get; private set; }
    public string UserEmail { get; private set; }
    public string UserName { get; private set; }
    public bool IsLoggedIn { get; private set; }
    public bool IsFirebaseLoggedIn { get; private set; }
    public bool IsPhotonConnected { get; private set; }

    // 250809 김동진 추가
    public float MasterVolume { get; set; } = 1f;
    public float BGMVolume { get; set; } = 1f;
    public float SFXVolume { get; set; } = 1f;

    #endregion

    #region 내부 상태 변수
    private bool isUserInfoSet = false;
    private bool isCleaningUp = false;
    #endregion

    #region Unity 라이프사이클
    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        GetSoundValue();
        StartCoroutine(SetupFirebaseAuthListener());
        FirebaseManager.OnUserLoggedOut += OnFirebaseUserLoggedOut;
    }

    private void OnDestroy()
    {
        // 이벤트 리스너 해제
        if (FirebaseManager.Auth != null)
        {
            FirebaseManager.Auth.StateChanged -= OnFirebaseAuthStateChanged;
        }
        FirebaseManager.OnUserLoggedOut -= OnFirebaseUserLoggedOut;
    }
    #endregion

    #region Firebase 인증 설정
    /// <summary>
    /// Firebase Auth 리스너를 설정합니다.
    /// Firebase가 초기화될 때까지 대기합니다.
    /// </summary>
    private IEnumerator SetupFirebaseAuthListener()
    {
        float waitTime = 0f;
        while (FirebaseManager.Auth == null && waitTime < 10f)
        {
            yield return new WaitForSeconds(0.1f);
            waitTime += 0.1f;
        }

        if (FirebaseManager.Auth != null)
        {
            FirebaseManager.Auth.StateChanged += OnFirebaseAuthStateChanged;
        }
    }

    /// <summary>
    /// Firebase 인증 상태 변경 시 호출되는 콜백
    /// </summary>
    private void OnFirebaseAuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (isCleaningUp) return;
        
        FirebaseAuth auth = sender as FirebaseAuth;
        if (auth != null)
        {
            if (auth.CurrentUser != null)
            {
                SetUserInfo(auth.CurrentUser);
            }
            else
            {
                PerformLogoutCleanup();
            }
        }
    }
    
    /// <summary>
    /// Firebase 로그아웃 이벤트 처리
    /// </summary>
    private void OnFirebaseUserLoggedOut()
    {
        PerformLogoutCleanup();
    }
    #endregion

    #region 사용자 정보 관리
    /// <summary>
    /// Firebase 사용자 정보를 설정합니다.
    /// </summary>
    public void SetUserInfo(FirebaseUser user)
    {
        if (user != null && !isCleaningUp)
        {
            UserID = user.UserId;
            UserEmail = user.Email;
            UserName = string.IsNullOrEmpty(user.DisplayName) ? "사용자" : user.DisplayName;
            IsLoggedIn = true;
            IsFirebaseLoggedIn = true;
            isUserInfoSet = true;
        }
        else if (!isCleaningUp)
        {
            ClearUserInfo();
        }
    }

    /// <summary>
    /// 사용자 정보를 초기화합니다.
    /// </summary>
    public void ClearUserInfo()
    {
        if (isCleaningUp) return;
        
        UserID = null;
        UserEmail = null;
        UserName = null;
        IsLoggedIn = false;
        IsFirebaseLoggedIn = false;
        IsPhotonConnected = false;
        isUserInfoSet = false;
    }

    /// <summary>
    /// 사용자 이름을 업데이트합니다.
    /// </summary>
    public void UpdateUserName(string newUserName)
    {
        if (!string.IsNullOrEmpty(newUserName) && !isCleaningUp)
        {
            UserName = newUserName;
            
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.NickName = newUserName;
            }
        }
    }

    private void GetSoundValue()
    {
        MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        BGMVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        SoundManager.Instance.SetBGMVolume(BGMVolume * MasterVolume);
        SoundManager.Instance.SetSFXVolume(SFXVolume * MasterVolume);
    }

    #endregion

    #region 로그아웃 처리
    /// <summary>
    /// 로그아웃 시 필요한 모든 정리 작업을 수행합니다.
    /// </summary>
    private void PerformLogoutCleanup()
    {
        if (isCleaningUp) return;

        isCleaningUp = true;

        // 사용자 정보 초기화
        ClearUserInfo();

        // Photon 연결 해제 및 커스텀 프로퍼티 정리
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.LocalPlayer != null)
            {
                var emptyProps = new ExitGames.Client.Photon.Hashtable();
                emptyProps["firebaseUID"] = null;
                emptyProps["email"] = null;
                emptyProps["displayName"] = null;
                emptyProps["loginTime"] = null;
                PhotonNetwork.LocalPlayer.SetCustomProperties(emptyProps);
            }

            PhotonNetwork.Disconnect();
        }

        // 게임 상태 리셋
        ResetGameState();

        isCleaningUp = false;
    }
    
    /// <summary>
    /// 게임 상태를 리셋합니다.
    /// </summary>
    private void ResetGameState()
    {
        // 필요시 게임별 상태 리셋 로직 추가
    }
    #endregion

    #region Photon 연결 관리
    /// <summary>
    /// Photon 연결 상태를 설정하고 동기화를 확인합니다.
    /// </summary>
    public void SetPhotonConnectionStatus(bool connected)
    {
        bool wasConnected = IsPhotonConnected;
        IsPhotonConnected = connected;
        
        if (wasConnected != connected && connected && !isCleaningUp)
        {
            CheckFirebasePhotonSync();
        }
    }

    /// <summary>
    /// Firebase와 Photon 간의 사용자 정보 동기화를 확인합니다.
    /// </summary>
    private void CheckFirebasePhotonSync()
    {
        if (!IsLoggedIn || isCleaningUp) return;

        bool needsForceSync = false;

        // AuthValues 동기화 확인
        if (PhotonNetwork.AuthValues != null)
        {
            string photonUserId = PhotonNetwork.AuthValues.UserId;
            if (photonUserId != UserID)
            {
                needsForceSync = true;
            }
        }
        else
        {
            needsForceSync = true;
        }

        // 커스텀 프로퍼티 동기화 확인
        if (PhotonNetwork.LocalPlayer != null)
        {
            if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("firebaseUID"))
            {
                string customUID = PhotonNetwork.LocalPlayer.CustomProperties["firebaseUID"]?.ToString();
                if (customUID != UserID)
                {
                    needsForceSync = true;
                }
            }
            else
            {
                needsForceSync = true;
            }
        }

        // 불일치 발견 시 강제 동기화
        if (needsForceSync && PhotonNetwork.IsConnected)
        {
            StartCoroutine(ForcePhotonSync());
        }
    }

    /// <summary>
    /// Firebase-Photon 강제 동기화를 수행합니다.
    /// </summary>
    private IEnumerator ForcePhotonSync()
    {
        yield return new WaitForSeconds(0.5f);
        
        if (!isCleaningUp && PhotonNetwork.IsConnected && !string.IsNullOrEmpty(UserID))
        {
            // AuthValues 업데이트
            PhotonNetwork.AuthValues = new AuthenticationValues(UserID);
            PhotonNetwork.NickName = GetPhotonPlayerName();
            
            // 커스텀 프로퍼티 업데이트
            var customProps = GetUserCustomProperties();
            PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
            
            // 재확인
            yield return new WaitForSeconds(1f);
            CheckFirebasePhotonSync();
        }
    }

    /// <summary>
    /// Firebase-Photon 강제 동기화를 외부에서 호출할 수 있는 메서드
    /// </summary>
    public void ForceSyncFirebasePhoton()
    {
        if (isCleaningUp) return;
        
        if (!IsLoggedIn || !PhotonNetwork.IsConnected) return;

        PhotonNetwork.AuthValues = new AuthenticationValues(UserID);
        PhotonNetwork.NickName = GetPhotonPlayerName();
        
        var customProps = GetUserCustomProperties();
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
        
        StartCoroutine(CheckSyncResult());
    }

    /// <summary>
    /// 동기화 결과를 확인합니다.
    /// </summary>
    private IEnumerator CheckSyncResult()
    {
        yield return new WaitForSeconds(1f);
        if (!isCleaningUp)
        {
            CheckFirebasePhotonSync();
        }
    }
    #endregion

    #region Photon 사용자 정보 헬퍼 메서드
    /// <summary>
    /// Photon에서 사용할 플레이어 이름을 반환합니다.
    /// </summary>
    public string GetPhotonPlayerName()
    {
        if (!isUserInfoSet || isCleaningUp)
        {
            return "Guest";
        }
        
        return string.IsNullOrEmpty(UserName) ? "Guest" : UserName;
    }

    /// <summary>
    /// Photon 커스텀 프로퍼티를 생성합니다.
    /// </summary>
    public ExitGames.Client.Photon.Hashtable GetUserCustomProperties()
    {
        if (!isUserInfoSet || isCleaningUp)
        {
            return new ExitGames.Client.Photon.Hashtable();
        }

        var props = new ExitGames.Client.Photon.Hashtable();
        props["firebaseUID"] = UserID;
        props["email"] = UserEmail;
        props["displayName"] = UserName;
        props["loginTime"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        return props;
    }

    /// <summary>
    /// 플레이어의 Firebase UID를 가져옵니다.
    /// </summary>
    public string GetPlayerFirebaseUID(Player player)
    {
        if (player?.CustomProperties != null && 
            player.CustomProperties.TryGetValue("firebaseUID", out object uid))
        {
            return uid.ToString();
        }
        
        return null;
    }
    #endregion

    #region 상태 확인 메서드
    /// <summary>
    /// 모든 시스템이 완전히 로그인되었는지 확인합니다.
    /// </summary>
    public bool IsFullyLoggedIn()
    {
        return IsFirebaseLoggedIn && IsPhotonConnected && !string.IsNullOrEmpty(UserID) && 
               isUserInfoSet && !isCleaningUp;
    }


    #endregion

    #region 매니저 제거
    /// <summary>
    /// 매니저를 완전히 제거합니다.
    /// </summary>
    public void DestroyManager()
    {
        if (!isCleaningUp)
        {
            PerformLogoutCleanup();
        }
        
        if (instance == this)
        {
            instance = null;
        }
        
        Destroy(gameObject);
    }
    #endregion
}