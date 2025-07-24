using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Extensions;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine.SceneManagement;

/// <summary>
/// Firebase 초기화 및 인증, 데이터베이스 연결을 관리하는 싱글톤 매니저
/// Firebase App, Auth, Database의 초기화와 상태 관리를 담당합니다.
/// </summary>
public class FirebaseManager : MonoBehaviour
{
    #region 싱글톤
    private static FirebaseManager instance;
    public static FirebaseManager Instance { get { return instance; } }
    #endregion

    #region Firebase 인스턴스
    private static FirebaseApp app;
    public static FirebaseApp App { get { return app; } }

    private static FirebaseAuth auth;
    public static FirebaseAuth Auth { get { return auth; } }
    
    private static FirebaseDatabase database;
    public static FirebaseDatabase Database { get { return database; } }
    #endregion
    
    #region 초기화 상태
    public static bool IsFirebaseReady { get; private set; } = false;
    public static bool IsAuthReady { get; private set; } = false;
    public static bool IsDatabaseReady { get; private set; } = false;
    #endregion
    
    #region 이벤트
    public static event Action OnFirebaseInitialized;
    public static event Action OnAllSystemsReady;
    public static event Action<FirebaseUser> OnUserDataChanged;
    public static event Action OnUserLoggedOut;
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
        StartCoroutine(InitializeFirebase());
    }
    
    private void OnDestroy()
    {
        // 이벤트 리스너 해제
        if (auth != null)
        {
            auth.StateChanged -= OnAuthStateChanged;
        }
    }
    #endregion
    
    #region Firebase 초기화
    /// <summary>
    /// Firebase 의존성을 확인하고 초기화를 시작합니다.
    /// </summary>
    private IEnumerator InitializeFirebase()
    {
        var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() => dependencyTask.IsCompleted);
        
        DependencyStatus dependencyStatus = dependencyTask.Result;
        if (dependencyStatus == DependencyStatus.Available)
        {
            app = FirebaseApp.DefaultInstance;
            IsFirebaseReady = true;
            
            InitializeAuth();
            InitializeDatabase();
            
            OnFirebaseInitialized?.Invoke();
            StartCoroutine(CheckAllSystemsReady());
        }
        else
        {
            Debug.LogError($"Firebase 의존성 오류: {dependencyStatus}");
            app = null;
            auth = null;
            database = null;
        }
    }
    
    /// <summary>
    /// Firebase Authentication을 초기화합니다.
    /// </summary>
    private void InitializeAuth()
    {
        try
        {
            auth = FirebaseAuth.DefaultInstance;
            IsAuthReady = true;
            
            if (auth != null)
            {
                auth.StateChanged += OnAuthStateChanged;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Firebase Auth 초기화 실패: {e.Message}");
            IsAuthReady = false;
        }
    }
    
    /// <summary>
    /// Firebase Realtime Database를 초기화합니다.
    /// </summary>
    private void InitializeDatabase()
    {
        try
        {
            string databaseURL = "https://isekaislimeproject-default-rtdb.asia-southeast1.firebasedatabase.app/";
        
            if (app != null)
            {
                database = FirebaseDatabase.GetInstance(app, databaseURL);
            }
            else
            {
                IsDatabaseReady = false;
                return;
            }
        
            if (database != null)
            {
                try
                {
                    // 에디터에서는 persistence 비활성화, 빌드에서는 활성화
                    #if UNITY_EDITOR
                    database.SetPersistenceEnabled(false);
                    #else
                    database.SetPersistenceEnabled(true);
                    #endif
                }
                catch
                {
                    // SetPersistenceEnabled 실패는 무시
                }
            
                IsDatabaseReady = true;
                StartCoroutine(MonitorDatabaseConnection());
            }
            else
            {
                IsDatabaseReady = false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Firebase Database 초기화 실패: {e.Message}");
            IsDatabaseReady = false;
        }
    }

    /// <summary>
    /// 모든 Firebase 시스템이 준비될 때까지 대기합니다.
    /// </summary>
    private IEnumerator CheckAllSystemsReady()
    {
        while (!IsFirebaseReady || !IsAuthReady || !IsDatabaseReady)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        OnAllSystemsReady?.Invoke();
    }
    #endregion

    #region 인증 상태 관리
    /// <summary>
    /// Firebase 인증 상태 변경 시 호출되는 콜백
    /// </summary>
    private void OnAuthStateChanged(object sender, EventArgs eventArgs)
    {
        FirebaseAuth authSender = sender as FirebaseAuth;
        if (authSender != null)
        {
            FirebaseUser user = authSender.CurrentUser;
            OnUserDataChanged?.Invoke(user);

            if (user == null)
            {
                OnUserLoggedOut?.Invoke();
            }
        }
    }
    #endregion
    
    #region 데이터베이스 연결 모니터링
    /// <summary>
    /// 데이터베이스 연결 상태를 모니터링합니다.
    /// </summary>
    private IEnumerator MonitorDatabaseConnection()
    {
        if (database == null) yield break;
        
        var connectedRef = database.GetReference(".info/connected");
        
        connectedRef.ValueChanged += (sender, args) =>
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError($"Database 연결 오류: {args.DatabaseError.Message}");
                return;
            }
            
            bool connected = (bool)args.Snapshot.Value;
            // 연결 상태는 로그로만 확인 (UI 업데이트 등은 필요시 추가)
        };
    }
    #endregion
    
    #region 계정 관리 메서드
    /// <summary>
    /// 사용자 프로필을 업데이트합니다.
    /// </summary>
    public static void UpdateUserProfile(string displayName, Action<bool> onComplete = null)
    {
        if (auth?.CurrentUser == null)
        {
            onComplete?.Invoke(false);
            return;
        }
        
        UserProfile profile = new UserProfile
        {
            DisplayName = displayName
        };
        
        auth.CurrentUser.UpdateUserProfileAsync(profile).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                onComplete?.Invoke(true);
            }
            else
            {
                Debug.LogError($"사용자 프로필 업데이트 실패: {task.Exception?.GetBaseException()?.Message}");
                onComplete?.Invoke(false);
            }
        });
    }
    
    /// <summary>
    /// 현재 사용자 계정을 삭제합니다.
    /// </summary>
    public static void DeleteCurrentUserAccount(Action<bool> onComplete = null)
    {
        if (auth?.CurrentUser == null)
        {
            onComplete?.Invoke(false);
            return;
        }
        
        string userID = auth.CurrentUser.UserId;
        
        var userRef = GetUserRef(userID);
        if (userRef != null)
        {
            // 사용자 데이터 먼저 삭제
            userRef.RemoveValueAsync().ContinueWithOnMainThread(dataTask =>
            {
                if (dataTask.IsCompletedSuccessfully)
                {
                    // 계정 삭제
                    auth.CurrentUser.DeleteAsync().ContinueWithOnMainThread(authTask =>
                    {
                        if (authTask.IsCompletedSuccessfully)
                        {
                            PerformPostDeletionCleanup();
                            onComplete?.Invoke(true);
                        }
                        else
                        {
                            HandleAccountDeletionError(authTask.Exception, onComplete);
                        }
                    });
                }
                else
                {
                    onComplete?.Invoke(false);
                }
            });
        }
        else
        {
            onComplete?.Invoke(false);
        }
    }
    
    /// <summary>
    /// 계정 삭제 오류를 처리합니다.
    /// </summary>
    private static void HandleAccountDeletionError(Exception exception, Action<bool> onComplete)
    {
        string errorMessage = exception?.GetBaseException()?.Message ?? "알 수 없는 오류";
        
        if (errorMessage.Contains("requires-recent-login"))
        {
            Debug.LogWarning("계정 삭제를 위해 재인증이 필요합니다.");
        }
        
        onComplete?.Invoke(false);
    }
    
    /// <summary>
    /// 계정 삭제 후 정리 작업을 수행합니다.
    /// </summary>
    private static void PerformPostDeletionCleanup()
    {
        // 필요시 계정 삭제 후 정리 로직 추가
    }
    
    /// <summary>
    /// 비밀번호 재설정 이메일을 발송합니다.
    /// </summary>
    public static void SendPasswordResetEmail(string email, Action<bool> onComplete = null)
    {
        if (auth == null)
        {
            onComplete?.Invoke(false);
            return;
        }
        
        auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                onComplete?.Invoke(true);
            }
            else
            {
                Debug.LogError($"비밀번호 재설정 이메일 발송 실패: {task.Exception?.GetBaseException()?.Message}");
                onComplete?.Invoke(false);
            }
        });
    }
    
    /// <summary>
    /// 사용자를 로그아웃합니다.
    /// </summary>
    public static void SignOut()
    {
        if (auth != null)
        {
            auth.SignOut();
        }
        
        PerformLogoutCleanup();
    }
    
    /// <summary>
    /// 로그아웃과 함께 씬을 전환합니다.
    /// </summary>
    public static void SignOutWithSceneTransition(string targetScene = "LoginScene", Action onComplete = null)
    {
        SignOut();
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ClearUserInfo();
        }
        
        if (Instance != null)
        {
            Instance.StartCoroutine(DelayedSceneTransition(targetScene, onComplete));
        }
        else
        {
            SceneManager.LoadScene(targetScene);
            onComplete?.Invoke();
        }
    }
    
    /// <summary>
    /// 지연된 씬 전환을 수행합니다.
    /// </summary>
    private static IEnumerator DelayedSceneTransition(string targetScene, Action onComplete)
    {
        yield return new WaitForSeconds(0.5f);
        
        SceneManager.LoadScene(targetScene);
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// 로그아웃 시 정리 작업을 수행합니다.
    /// </summary>
    private static void PerformLogoutCleanup()
    {
        // 필요시 로그아웃 정리 로직 추가
    }
    #endregion
    
    #region 데이터베이스 참조 메서드
    /// <summary>
    /// 특정 사용자의 데이터베이스 참조를 가져옵니다.
    /// </summary>
    public static DatabaseReference GetUserRef(string userID)
    {
        if (database == null || string.IsNullOrEmpty(userID))
        {
            return null;
        }
        
        return database.GetReference("users").Child(userID);
    }
    
    /// <summary>
    /// 글로벌 설정의 데이터베이스 참조를 가져옵니다.
    /// </summary>
    public static DatabaseReference GetGlobalSettingsRef()
    {
        if (database == null)
        {
            return null;
        }
        
        return database.GetReference("globalSettings");
    }
    #endregion
    
    #region 상태 확인 메서드
    /// <summary>
    /// 모든 Firebase 시스템이 초기화되었는지 확인합니다.
    /// </summary>
    public static bool IsFullyInitialized()
    {
        return IsFirebaseReady && IsAuthReady && IsDatabaseReady;
    }
    
    /// <summary>
    /// 사용자가 로그인되어 있는지 확인합니다.
    /// </summary>
    public static bool IsUserLoggedIn()
    {
        return auth?.CurrentUser != null;
    }
    
    /// <summary>
    /// 현재 로그인된 사용자를 반환합니다.
    /// </summary>
    public static FirebaseUser GetCurrentUser()
    {
        return auth?.CurrentUser;
    }
    
    /// <summary>
    /// 현재 사용자의 ID를 반환합니다.
    /// </summary>
    public static string GetCurrentUserID()
    {
        return auth?.CurrentUser?.UserId;
    }
    
    /// <summary>
    /// 현재 사용자의 이메일을 반환합니다.
    /// </summary>
    public static string GetCurrentUserEmail()
    {
        return auth?.CurrentUser?.Email;
    }
    
    /// <summary>
    /// 현재 사용자의 표시 이름을 반환합니다.
    /// </summary>
    public static string GetCurrentUserDisplayName()
    {
        return auth?.CurrentUser?.DisplayName ?? "Guest";
    }
    #endregion
    
    #region 유틸리티 메서드
    /// <summary>
    /// 네트워크 연결 상태를 확인합니다.
    /// </summary>
    public static void CheckNetworkConnection(Action<bool> onResult)
    {
        if (database == null)
        {
            onResult?.Invoke(false);
            return;
        }
        
        var connectedRef = database.GetReference(".info/connected");
        connectedRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                bool connected = (bool)task.Result.Value;
                onResult?.Invoke(connected);
            }
            else
            {
                onResult?.Invoke(false);
            }
        });
    }
    #endregion
}