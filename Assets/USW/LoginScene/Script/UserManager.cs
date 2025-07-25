using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.SceneManagement;
using Photon.Pun;

/// <summary>
/// 사용자 정보 관리 및 프로필 관련 기능을 담당하는 싱글톤 매니저
/// Firebase Database와 연동하여 사용자 프로필, 닉네임 변경, 계정 탈퇴 등을 처리합니다.
/// </summary>
public class UserManager : MonoBehaviour
{
    #region 싱글톤
    private static UserManager instance;
    public static UserManager Instance { get { return instance; } }
    #endregion
    
    #region Firebase 연동 변수
    private DatabaseReference userRef;
    private bool isFirebaseEnabled = false;
    private bool isInitialized = false;
    #endregion
    
    #region 사용자 정보 캐시
    private string currentUID = "";
    private string currentEmail = "";
    private string currentDisplayName = "";
    #endregion
    
    #region 이벤트
    public static event Action<string> OnDisplayNameChanged;
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
        StartCoroutine(WaitForInitialization());
    }
    #endregion
    
    #region 초기화
    /// <summary>
    /// GameManager와 FirebaseManager가 준비될 때까지 대기한 후 초기화합니다.
    /// </summary>
    private IEnumerator WaitForInitialization()
    {
        // 의존성 시스템들이 준비될 때까지 대기
        while (GameManager.Instance == null || 
               !FirebaseManager.IsFullyInitialized() ||
               !GameManager.Instance.IsFirebaseLoggedIn)
        {
            yield return new WaitForSeconds(0.5f);
        }
        
        InitializeUserManager();
    }
    
    /// <summary>
    /// UserManager를 초기화하고 Firebase Database 연결을 설정합니다.
    /// </summary>
    private void InitializeUserManager()
    {
        // Firebase Database 연결 설정
        if (GameManager.Instance.IsFirebaseLoggedIn && FirebaseManager.Database != null)
        {
            userRef = FirebaseManager.GetUserRef(GameManager.Instance.UserID);
            
            if (userRef != null)
            {
                isFirebaseEnabled = true;
            }
        }
        
        // 현재 사용자 정보 업데이트
        UpdateCurrentUserInfo();
        isInitialized = true;
    }
    
    /// <summary>
    /// GameManager에서 현재 사용자 정보를 가져와 캐시를 업데이트합니다.
    /// </summary>
    private void UpdateCurrentUserInfo()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsLoggedIn)
        {
            currentUID = GameManager.Instance.UserID;
            currentEmail = GameManager.Instance.UserEmail;
            currentDisplayName = GameManager.Instance.UserName;
        }
    }
    #endregion
    
    #region 공개 조회 메서드
    /// <summary>
    /// 현재 사용자의 Firebase UID를 반환합니다.
    /// </summary>
    public string GetUID()
    {
        return currentUID;
    }
    
    /// <summary>
    /// 현재 사용자의 표시 이름을 반환합니다.
    /// </summary>
    public string GetDisplayName()
    {
        return string.IsNullOrEmpty(currentDisplayName) ? "Guest" : currentDisplayName;
    }
    
    /// <summary>
    /// 현재 사용자의 이메일을 반환합니다.
    /// </summary>
    public string GetEmail()
    {
        return currentEmail;
    }
    
    /// <summary>
    /// UserManager가 초기화되었는지 확인합니다.
    /// </summary>
    public bool IsReady()
    {
        return isInitialized;
    }
    #endregion
    
    #region 닉네임 변경
    /// <summary>
    /// 사용자의 표시 이름을 변경합니다.
    /// Firebase Auth와 Database 모두 업데이트합니다.
    /// </summary>
    /// <param name="newDisplayName">닉네임 표시</param>
    /// <param name="onComplete">완료 콜백 (성공 여부)</param>
    public void UpdateDisplayName(string newDisplayName, System.Action<bool> onComplete = null)
    {
        // 유효성 검사
        if (!isInitialized || string.IsNullOrEmpty(newDisplayName))
        {
            onComplete?.Invoke(false);
            return;
        }
        
        if (newDisplayName.Length < 2 || newDisplayName.Length > 10)
        {
            onComplete?.Invoke(false);
            return;
        }
        
        StartCoroutine(UpdateDisplayNameProcess(newDisplayName, onComplete));
    }
    
    /// <summary>
    /// 닉네임 변경 과정을 처리합니다.
    /// </summary>
    private IEnumerator UpdateDisplayNameProcess(string newDisplayName, System.Action<bool> onComplete)
    {
        bool success = true;
        
        // GameManager 사용자 정보 업데이트
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateUserName(newDisplayName);
        }
        
        // Firebase 업데이트
        if (isFirebaseEnabled)
        {
            // Firebase Auth 프로필 업데이트
            bool authUpdateComplete = false;
            bool authUpdateSuccess = false;
            
            FirebaseManager.UpdateUserProfile(newDisplayName, (result) =>
            {
                authUpdateSuccess = result;
                authUpdateComplete = true;
            });
            
            // Auth 업데이트 완료 대기
            while (!authUpdateComplete)
            {
                yield return new WaitForSeconds(0.1f);
            }
            
            if (!authUpdateSuccess)
            {
                success = false;
            }
            
            // Firebase Database 프로필 업데이트
            if (authUpdateSuccess && userRef != null)
            {
                var profileData = new System.Collections.Generic.Dictionary<string, object>
                {
                    ["displayName"] = newDisplayName,
                    ["lastUpdated"] = ServerValue.Timestamp
                };
                
                bool dbUpdateComplete = false;
                
                userRef.Child("profile").UpdateChildrenAsync(profileData).ContinueWithOnMainThread(task =>
                {
                    if (task.IsFaulted)
                    {
                        // Database 업데이트 실패는 경고만 (Auth는 성공했으므로)
                    }
                    dbUpdateComplete = true;
                });
                
                // Database 업데이트 완료 대기 (최대 3초)
                float waitTime = 0f;
                while (!dbUpdateComplete && waitTime < 3f)
                {
                    yield return new WaitForSeconds(0.1f);
                    waitTime += 0.1f;
                }
            }
        }
        
        // 성공시 캐시 업데이트 및 이벤트 발생
        if (success)
        {
            currentDisplayName = newDisplayName;
            OnDisplayNameChanged?.Invoke(newDisplayName);
        }
        
        onComplete?.Invoke(success);
    }
    #endregion
    
    #region 계정 탈퇴
    /// <summary>
    /// 사용자 계정을 완전히 삭제합니다.
    /// Firebase Database 데이터 삭제 → Firebase Auth 계정 삭제 → 로컬 데이터 정리
    /// </summary>
    /// <param name="onComplete">완료 콜백 (성공 여부)</param>
    public void DeleteAccount(System.Action<bool> onComplete = null)
    {
        if (!isInitialized)
        {
            onComplete?.Invoke(false);
            return;
        }
        
        StartCoroutine(DeleteAccountProcess(onComplete));
    }
    
    /// <summary>
    /// 계정 삭제 과정을 처리합니다.
    /// </summary>
    private IEnumerator DeleteAccountProcess(System.Action<bool> onComplete)
    {
        bool success = true;
        
        // 1. Firebase Database 사용자 데이터 삭제
        if (isFirebaseEnabled && userRef != null)
        {
            bool dataDeleteComplete = false;
            
            userRef.RemoveValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    success = false;
                }
                dataDeleteComplete = true;
            });
            
            // 데이터 삭제 완료 대기
            while (!dataDeleteComplete)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        // 2. Firebase Auth 계정 삭제
        if (isFirebaseEnabled)
        {
            bool authDeleteComplete = false;
            
            FirebaseManager.DeleteCurrentUserAccount((result) =>
            {
                if (!result)
                {
                    success = false;
                }
                authDeleteComplete = true;
            });
            
            // 계정 삭제 완료 대기
            while (!authDeleteComplete)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        // 3. 로컬 데이터 완전 삭제
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        
        // 4. 네트워크 연결 해제
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
        
        // 5. GameManager 정보 초기화
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ClearUserInfo();
        }
        
        // 완료 콜백 호출
        onComplete?.Invoke(success);
        
        // 6. 로그인 씬으로 이동
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("LoginScene");
    }
    #endregion
}