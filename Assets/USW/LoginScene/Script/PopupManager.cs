using System;
using System.Collections;
using Firebase.Auth;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 게임내 다양한 팝업을 통한 관리하는 클래스입니다
/// - 일반 메시지 팝업 , 확인/취소 팝업 , 비밀번호 변경 팝업등을 현재 구현하였으며
/// - Firebase Auth 와 연동 할 예정입니다.
/// </summary>
public class PopupManager : MonoBehaviour
{
    [Header("오류 메세지 확인 팝업")] [SerializeField]
    private GameObject mainPopupPanel;

    [SerializeField] private TextMeshProUGUI statusText;

    [SerializeField] private Button closeButton;

    [SerializeField] Canvas canvas;

    [Header("확인 / 취소 팝업")] [SerializeField]
    private GameObject confirmationPanel;

    [SerializeField] TextMeshProUGUI confirmationText;
    [SerializeField] Button yesButton;
    [SerializeField] Button noButton;

    [Header("비밀번호 변경 팝업")] [SerializeField]
    private GameObject passwordChangePanel;

    [SerializeField] TMP_InputField currentPasswordInputField;
    [SerializeField] TMP_InputField newPasswordInputField;
    [SerializeField] Button changePasswordButton;
    [SerializeField] Button cancelPasswordButton;
    
    [Header("닉네임 변경 팝업")]
    [SerializeField] private GameObject nicknameChangePanel;
    [SerializeField] private TMP_InputField nicknameInputField;
    [SerializeField] private Button saveNicknameButton;
    [SerializeField] private Button  cancelNicknameButton;

    [Header("룸 코드 팝업")] [SerializeField] private GameObject roomcodePanel;
    [SerializeField] private TMP_InputField roomcodeInputField;
    [SerializeField] private Button confirmJoinButton;
    [SerializeField] private Button cancelJoinButton;
    

    
    

    /// <summary>
    /// 싱글톤으로 인게임 , 메뉴 , 로그인창에 사용될 예정입니다.
    /// 근데 생각해보자 이게 어쩌피 동적으로 생성되고 팝업패널도 액션으로 인해 매번 다르게 나오는데 ? 그럼 굳이 싱글톤 하겝다시고
    /// 굳이 Awake 에서 계속 생성할 필요가있을까 ?
    /// 어쩌피 팝업이 동적으로 생성되고 (instance 호출할때)
    /// 씬도 미리 배치할 이유도없고
    /// 그러면 그냥 필요할떄만 쓰는게 차라리 메모리적으로도 효율이 좋지않을까 ?
    /// 그러면 create 에서 걍 초기화 해주면 어느정도 논리는 맞는것같은데 ....
    /// 상상으로는 맞는데 getter실행되고 ? - > createpopupmanager 실행되고 ? resourece 에서 로드되고 ? 그럼 굳이 awake 거쳐갈필요강벗잖아 근데 아 ... 꼬일거 생각하니깐 또 킹받네
    /// </summary>
    /// <returns></returns>
    public static PopupManager instance;

    public static PopupManager Instance
    {
        get
        {
            if (instance == null)
            {
                CreatePopupManager();
            }

            return instance;
        }
    }

    // 확인 팝업 / 닉네임 변경 에서 사용할 콜백 함수들입니다. 
    private Action onYesCallback;
    private Action onNoCallback;
    private Action<string> onNicknameSaveCallBack;
    private Action<string> onRoomCodeJoinCallBack;

    // 비밀번호 변경 죽복 처리 방지를 위한 bool 변수입니다.
    private bool isProcessingPasswordChange = false;

    /// <summary>
    /// 모든 버튼의 클릭 이벤트 리스너를 설정하는 메서드입니다.
    /// </summary>
    void SetupButtons()
    {
        if (closeButton)
        {
            closeButton.onClick.AddListener(ClosePopup);
        }

        // 확인 팝업의 " Yes " 버튼 - 콜백 실행 후 팝업 닫기
        if (yesButton)
        {
            yesButton.onClick.AddListener(() =>
            {
                onYesCallback?.Invoke();
                ClosePopup();
            });
        }

        // 확인 팝업의 " No " 버튼 - 콜백 실행 후 팝업 닫기
        if (noButton)
        {
            noButton.onClick.AddListener(() =>
            {
                onNoCallback?.Invoke();
                ClosePopup();
            });
        }

        // 비밀번호 변경 버튼
        if (changePasswordButton)
        {
            changePasswordButton.onClick.AddListener(OnChangePasswordClick);
        }

        // 비밀번호 변경 취소 버튼
        if (cancelPasswordButton)
        {
            cancelPasswordButton.onClick.AddListener(OnCancelPasswordClick);
        }
        
        // 닉네임 변경 버튼
        if (saveNicknameButton)
        {
            saveNicknameButton.onClick.AddListener(OnSaveNicknameClick);
        }

        if (cancelNicknameButton)
        {
            cancelNicknameButton.onClick.AddListener(OnCancleNicknameClick);
        }
        
        // 방 코드 입력 버튼
        if (confirmJoinButton)
        {
            confirmJoinButton.onClick.AddListener(OnConfirmJoinClick);
        }
        if (cancelJoinButton)
        {
            cancelJoinButton.onClick.AddListener(OnCancelJoinClick);
        }

        // 모든 패널을 초기에는 비활성화 함 
        if (confirmationPanel)
        {
            confirmationPanel.SetActive(false);
        }

        if (passwordChangePanel)
        {
            passwordChangePanel.SetActive(false);
        }

        if (nicknameChangePanel)
        {
            nicknameChangePanel.SetActive(false);
        }

        if (roomcodePanel)
        {
            roomcodePanel.SetActive(false);
        }

        // Enter 키로 비밀번호 변경 실행
        if (newPasswordInputField)
        {
            newPasswordInputField.onEndEdit.AddListener(delegate
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    OnChangePasswordClick();
                }
            });
        }
        
        //Enter 키로 닉네임 저장 실행
        if (nicknameInputField)
        {
            nicknameInputField.onEndEdit.AddListener(delegate
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    OnSaveNicknameClick();
                }
            });
        }
        
        // Enter 키로 방 참가 실행
        if (roomcodeInputField)
        {
            roomcodeInputField.onEndEdit.AddListener(delegate
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    OnConfirmJoinClick();
                }
            });
        }
    }

    /// <summary>
    /// Resources 폴더에서 PopupManager 프리팹 로드해서 인스턴스 생성
    /// </summary>
    static void CreatePopupManager()
    { 
        GameObject popupPrefab = Resources.Load<GameObject>("PopupManager");

        GameObject popupInstance = Instantiate(popupPrefab);
        
       PopupManager popup = popupInstance.GetComponent<PopupManager>();
       instance = popup;
       
       DontDestroyOnLoad(popupPrefab);
       popup.canvas.sortingOrder = 777;
       
       //초기화
       popup.SetupButtons();
       popupPrefab.SetActive(false);
    }


    #region 공용 메서드

    /// <summary>
    /// 기본 메세지 팝업 표시
    /// </summary>
    /// <param name="message"> 표시할 메세지입니다. </param>
    public void ShowPopup(string message)
    {
        // 다른 패널 숨기기 기능
        HideAllPanels();

        if (statusText != null)
        {
            statusText.text = message;
        }

        ShowMainPanel();
        gameObject.SetActive(true);
    }


    /// <summary>
    /// 확인 / 취소 팝업 표시
    /// </summary>
    /// <param name="message">확인 메세지</param>
    /// <param name="onYes">"yes" 선택 시 실행할 콜백입니다.</param>
    /// <param name="onNo">"No" 선택 시 실행할 콜백입니다.</param>
    public void ShowConfirmationPopup(string message, Action onYes, Action onNo = null)
    {
        // 콜백 함수 저장 
        onYesCallback = onYes;
        onNoCallback = onNo;

        if (confirmationText != null)
        {
            confirmationText.text = message;
        }

        HideAllPanels();
        ShowConfirmationPanel();
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 비밀번호 변경 팝업 표시입니다.
    /// </summary>
    public void ShowPasswordChangePopup()
    {
        HideAllPanels();
        ShowPasswordChangePanel();
        gameObject.SetActive(true);

        if (currentPasswordInputField != null)
        {
            currentPasswordInputField.text = "";
            currentPasswordInputField.Select();
        }

        if (newPasswordInputField != null)
        {
            newPasswordInputField.text = "";
        }

        isProcessingPasswordChange = false;
    }

    #endregion


    #region Panel 표시 관련

    /// <summary>
    /// 기본 오류 메세지 패널 표시
    /// </summary>
    void ShowMainPanel()
    {
        if (mainPopupPanel)
        {
            mainPopupPanel.SetActive(true);
        }
    }


    /// <summary>
    /// 확인/취소 패널 표시
    /// </summary>
    void ShowConfirmationPanel()
    {
        if (confirmationPanel)
        {
            confirmationPanel.SetActive(true);
        }
    }

    /// <summary>
    /// 비밀번호 변경 패널 표시
    /// </summary>
    void ShowPasswordChangePanel()
    {
        if (passwordChangePanel)
        {
            passwordChangePanel.SetActive(true);
        }
    }

    void ShowNicknameChangePanel()
    {
        if (nicknameChangePanel)
        {
            nicknameChangePanel.SetActive(true);
        }
    }


    /// <summary>
    /// 모든 팝업 패널 숨기기
    /// </summary>
    void HideAllPanels()
    {
        if (mainPopupPanel)
        {
            mainPopupPanel.SetActive(false);
        }
        
        if (confirmationPanel)
        {
            confirmationPanel.SetActive(false);
        }

        if (passwordChangePanel)
        {
            passwordChangePanel.SetActive(false);
        }

        if (nicknameChangePanel)
        {
            nicknameChangePanel.SetActive(false);
        }
        
        if(roomcodePanel)
        {roomcodePanel.SetActive(false);}
    }

    #endregion


    #region 비밀번호 변경 로직

    void OnChangePasswordClick()
    {
        // 중복 처리 방지
        if (isProcessingPasswordChange)
        {
            Debug.Log("이미 비밀번호 변경 처리 중입니다.");
            return;
        }

        string currentPassword = currentPasswordInputField.text.Trim();
        string newPassword = newPasswordInputField.text.Trim();

        // 입력값 유효성 검사
        if (string.IsNullOrEmpty(currentPassword))
        {
            ShowPopup("현재 비밀번호를 입력해주세요.");
            return;
        }

        if (string.IsNullOrEmpty(newPassword))
        {
            ShowPopup("새 비밀번호를 입력해주세요.");
            return;
        }

        if (newPassword.Length < 6)
        {
            ShowPopup("새 비밀번호는 6자 이상이어야 합니다.");
            return;
        }

        if (currentPassword == newPassword)
        {
            ShowPopup("새 비밀번호는 현재 비밀번호와 달라야 합니다.");
            return;
        }

        // 비밀번호 변경 프로세스 시작
        isProcessingPasswordChange = true;
    }


    /// <summary>
    /// Firebase Auth를 사용한 비밀번호로 변경 프로세스
    /// 1. 현재 비밀번호로 인증 후
    /// 2. 새 비밀번호로 업데이트 함.
    /// </summary>
    /// <param name="currentPassword">현재 비밀번호</param>
    /// <param name="newPassword">새로 등록할 비밀번호 </param>
    /// <returns></returns>
    private IEnumerator ProcessPasswordChange(string currentPassword, string newPassword)
    {
        // 현재 로그인된 사용자 확인 
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            ShowPopup("로그인 되지 않은 유저입니다.");
            isProcessingPasswordChange = false;
            yield break;
        }


        // 재인증 한번 하고 
        bool reAuthSuccess = false;
        bool reAuthCompleted = false;
        string reAuthErrorMessage = "";


        // 이메일 / 비밀번호 자격증명 생성
        Credential credential = EmailAuthProvider.GetCredential(user.Email, currentPassword);

        // 비동기 재인증 실행  
        user.ReauthenticateAsync(credential).ContinueWithOnMainThread(reAuthTask =>
        {
            reAuthCompleted = true;

            if (reAuthTask.IsCanceled)
            {
                reAuthErrorMessage = "취소 되었습니다.";
            }
            else if (reAuthTask.IsFaulted)
            {
                reAuthErrorMessage = "현재 비밀번호가 아닙니다";
            }
            else
            {
                reAuthSuccess = true;
            }
        });


        // 재인증 완료까지 대기 ( 포톤 대기용 타임 아웃 적용 )

        float timeout = 0f;
        while (!reAuthCompleted && timeout < 10f)
        {
            yield return new WaitForSeconds(0.1f);
            timeout += 0.1f;
        }

        // 재인증 실패시 종료 
        if (!reAuthSuccess)
        {
            ShowPopup(string.IsNullOrEmpty(reAuthErrorMessage) ? "재인증에 실패했습니다." : reAuthErrorMessage);
            isProcessingPasswordChange = false;
            yield break;
        }

        // 비밀번호 변경 프로세스 ( 불변수 판단 ) 
        bool changeSuccess = false;
        bool changeCompleted = false;
        string changeErrorMessage = "";

        // 비동기 비밀번호 업데이트 실행 
        user.UpdatePasswordAsync(newPassword).ContinueWithOnMainThread(changeTask =>
        {
            changeCompleted = true;

            if (changeTask.IsCanceled)
            {
                changeErrorMessage = "비밀번호 변경이 취소되었습니다";
            }
            else if (changeTask.IsFaulted)
            {
                changeErrorMessage = "비밀번호 변경에 실패하였습니다.";
            }
            else
            {
                changeSuccess = true;
            }
        });

        // 비밀번호 변경 완료까지 또 대기 ( 타임아웃 적용 ) 
        timeout = 0f;
        while (!changeCompleted && timeout < 10f)
        {
            yield return new WaitForSeconds(0.1f);
            timeout += 0.1f;
        }

        if (changeSuccess)
        {
            ShowPopup("비밀번호가 변경되었습니다.");
        }
        else
        {
            ShowPopup(string.IsNullOrEmpty(changeErrorMessage) ? "비밀번호 변경에 실패하였습니다" : changeErrorMessage);
        }

        isProcessingPasswordChange = false;
        // 피드백 표시
    }


    /// <summary>
    /// 비밀번호 변경 취소 처리입니다.
    /// </summary>
    void OnCancelPasswordClick()
    {
        ClosePopup();
    }

    #endregion


    #region 닉네임 변경 로직

    /// <summary>
    /// 닉네임 변경 팝업 표시
    /// </summary>
    /// <param name="currentNickname">현재 닉네임</param>
    /// <param name="onSave">저장시 실행할 콜백(새 닉네임 전달)</param>
    public void ShowNicknameChangePopup(string currentNickname, Action<string> onSave)
    {
        onNicknameSaveCallBack = onSave;
        
        HideAllPanels();
        ShowNicknameChangePanel();
        gameObject.SetActive(true);

        if (nicknameInputField)
        {
            nicknameInputField.text = currentNickname;
            nicknameInputField.Select();
        }
    }

    void OnSaveNicknameClick()
    {
        string newNickname = nicknameInputField.text.Trim();

        if (string.IsNullOrEmpty(newNickname))
        {
            ShowPopup("닉네임을 입력해주세요");
            return;
        }

        if (newNickname.Length < 2 || newNickname.Length > 12)
        {
            ShowPopup("닉네임은 2~12자로 입력해주세요");
            return;
        }
        
        // 콜백 호출(백앤드는 SettingsPanel 에서 처리되고있습니다.
        // OnEditNickname 쪽 봐주세요
        onNicknameSaveCallBack?.Invoke(newNickname);
        ClosePopup();
    }

    /// <summary>
    /// 닉네임 변경 취소
    /// </summary>
    void OnCancleNicknameClick()
    {
        ClosePopup();
    }

    #endregion


    #region 방 참가 관련 메서드들

    /// <summary>
    /// 방 코드 입력 팝업 표시
    /// </summary>
    /// <param name="onJoin">방 참가 시 실행할 콜백</param>
    public void ShowRoomCodeInputPopup(Action<string> onJoin)
    {
        onRoomCodeJoinCallBack = onJoin;

        HideAllPanels();
        ShowRoomCodeInputPanel();
        gameObject.SetActive(true);

        if (roomcodeInputField)
        {
            roomcodeInputField.text = "";
            roomcodeInputField.Select();
        }
    }
    
    /// <summary>
    /// 방 코드 입력 패널 표시
    /// </summary>
    void ShowRoomCodeInputPanel()
    {
        if (roomcodePanel)
        {
            roomcodePanel.SetActive(true);
        }
    }
    
    void OnConfirmJoinClick()
    {
        string roomCode = roomcodeInputField.text.Trim();

        if (string.IsNullOrEmpty(roomCode))
        {
            ShowPopup("방 코드를 입력해주세요");
            return;
        }

        if (roomCode.Length != 4)
        {
            ShowPopup("4자리의 방 코드를 입력하세요");
            return;
        }

        if (!int.TryParse(roomCode, out int code))
        {
            ShowPopup("숫자만 입력 가능합니다");
            return;
        }

        // 콜백 호출
        onRoomCodeJoinCallBack?.Invoke(roomCode);
        ClosePopup();
    }
    
    /// <summary>
    /// 방 코드 입력 취소
    /// </summary>
    void OnCancelJoinClick()
    {
        ClosePopup();
    }
    
    

    #endregion
    
    #region 팝업 컨트롤 메서드

    /// <summary>
    /// 팝업 닫기 및 상태 초기화
    /// </summary>
    public void ClosePopup()
    {
        HideAllPanels();
        
        // 콜백 함수 참조 정리 (메모리 누수 방지)
        onNoCallback = null;
        onYesCallback = null;
        onNicknameSaveCallBack = null;
        isProcessingPasswordChange = false;
        onRoomCodeJoinCallBack = null;

        gameObject.SetActive(false);
    }

    #endregion
}