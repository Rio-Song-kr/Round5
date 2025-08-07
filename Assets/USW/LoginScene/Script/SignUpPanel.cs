using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignUpPanel : MonoBehaviour
{
    [Header("Panel References")] [SerializeField]
    private GameObject loginPanel;

    [Header("Input Fields")] [SerializeField]
    TMP_InputField emailInputField;

    [SerializeField] TMP_InputField passwordInputField;
    [SerializeField] TMP_InputField confirmPasswordInputField;

    [Header("Buttons")] [SerializeField] Button signUpButton;
    [SerializeField] Button cancelButton;
    [SerializeField] Button emailCheckButton;

    private bool isSigningUp = false;
    private bool isEmailChecking = false;
    private bool isEmailVerified = false;
    private string verifiedEmail = "";

    /// <summary>
    /// 컴포넌트 초기화 시 버튼 리스너 설정 및 UI 상태 초기화
    /// Unity의 생명주기에 따라 Start()보다 먼저 실행되어 의존성 설정을 보장
    /// </summary>
    private void Awake()
    {
        SetupButtonListeners();
        ResetUI();
    }

    /// <summary>
    /// 모든 버튼에 이벤트 리스너 등록하고
    /// null 체크 해야하나 근데 ? 
    /// </summary>
    void SetupButtonListeners()
    {
        signUpButton.onClick.AddListener(SignUp);
        cancelButton.onClick.AddListener(Cancel);
        emailCheckButton.onClick.AddListener(CheckEmailDuplication);
        emailInputField.onValueChanged.AddListener(OnEmailInputChanged);
    }

    /// <summary>
    /// UI 상태를 초기값으로 되돌림
    /// </summary>
    private void ResetUI()
    {
        isSigningUp = false;
        isEmailChecking = false;
        isEmailVerified = false;
        verifiedEmail = "";

        signUpButton.interactable = true; 
        emailCheckButton.interactable = true;
    }

    /// <summary>
    /// 이메일 입력 필드 값 변경 시 호출되는 콜백
    /// 이전에 검증된 이메일과 다른 값이 입력되면 검증 상태를 초기화
    /// </summary>
    /// <param name="newEmail">새로 입력된 이메일 문자열</param>
    private void OnEmailInputChanged(string newEmail)
    {
        // 이메일이 변경되면 중복체크 상태 리셋
        if (verifiedEmail != newEmail.Trim())
        {
            isEmailVerified = false;
            verifiedEmail = "";
            UpdateSignUpButtonState();
        }
    }

    /// <summary>
    /// 회원가입 버튼의 활성화 상태를 업데이트
    /// 이메일 검증과 관계없이 회원가입 버튼은 활성화하고, SignUp() 메서드에서 검증 처리
    /// </summary>
    private void UpdateSignUpButtonState()
    {
        if (signUpButton != null)
        {
            signUpButton.interactable = !isSigningUp;
        }
    }

    #region 이메일 중복체크

    void CheckEmailDuplication()
    {
        string email = emailInputField.text.Trim();

        if (!IsValidEmailFormat(email))
        {
            ShowPopup("이메일 형식을 지켜주세요");
            return;
        }

        if (isEmailChecking)
        {
            // 이미 체크 중이면 중복 실행방지
            return;
        }

        isEmailChecking = true;

        if (emailCheckButton != null)
        {
            emailCheckButton.interactable = false;
        }

        FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(email, "DummyPassword")
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    ShowPopup("이메일 확인이 취소되었습니다");
                    ResetEmailCheckUI(); 
                    return;
                }

                if (task.IsFaulted)
                {
                    // 오류 발생 - 이메일이 이미 존재하는 경우
                    string errorMessage = task.Exception?.GetBaseException()?.Message ?? "";

                    if (errorMessage.ToLower().Contains("email-already-in-use") ||
                        errorMessage.ToLower().Contains("email already in use") ||
                        errorMessage.ToLower().Contains("already in use by another account"))
                    {
                        ShowPopup("이미 존재하는 이메일입니다");
                    }
                    else
                    {
                        ShowPopup(GetUserFriendlyErrorFromMessage(errorMessage));
                    }

                    ResetEmailCheckUI(); 
                    return;
                }

                // 성공 - 사용 가능한 이메일이므로 즉시 삭제
                FirebaseUser tempUser = task.Result.User;

                // 임시 계정 삭제하여 실제 데이터베이스에 데이터 남기지 않음
                tempUser.DeleteAsync().ContinueWithOnMainThread(deleteTask =>
                {
                    if (deleteTask.IsCompletedSuccessfully)
                    {
                        // 이메일 사용 가능
                        isEmailVerified = true;
                        verifiedEmail = email;
                        UpdateSignUpButtonState();

                        ShowPopup("사용 가능한 이메일입니다!");

                        if (emailCheckButton != null)
                        {
                            var checkText = emailCheckButton.GetComponentInChildren<TMP_Text>();
                            if (checkText != null)
                                checkText.text = "확인완료";
                        }
                    }
                    else
                    {
                        ShowPopup("이메일 확인 과정에서 오류가 발생했습니다. 다시 시도해주세요.");
                    }
                    
                    ResetEmailCheckUI(); 
                });
            });
    }

    /// <summary>
    /// 이메일 중복 체크 UI를 초기 상태로 복원
    /// 체크 완료 후 버튼을 다시 사용할 수 있도록 활성화
    /// </summary>
    private void ResetEmailCheckUI()
    {
        isEmailChecking = false;
        if (emailCheckButton != null)
            emailCheckButton.interactable = true;
    }

    #endregion

    #region 이메일 형식 검증

    /// <summary>
    /// 이메일 주소 형식이 올바른지 검증합니다.
    /// </summary>
    /// <param name="email">검증할 이메일 주소 </param>
    /// <returns>올바른 형식이면 true , 아니면 false 반환</returns>
    bool IsValidEmailFormat(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region 회원가입

    void SignUp()
    {
        if (!isEmailVerified || verifiedEmail != emailInputField.text.Trim())
        {
            ShowPopup("이메일 중복체크를 먼저 해주세요");
            return;
        }

        if (!ValidateInputs())
        {
            return;
        }

        // 회원가입 진행중 상태로 변경
        isSigningUp = true;

        // Firebase 회원가입 시도
        string email = emailInputField.text.Trim();
        string password = passwordInputField != null ? passwordInputField.text : "";

        FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    HandleSignUpResult(false, "회원가입이 취소되었습니다");
                    return;
                }

                if (task.IsFaulted)
                {
                    string errorMessage = HandleFirebaseException(task.Exception);
                    HandleSignUpResult(false, errorMessage);
                    return;
                }

                // 회원가입 성공
                ShowCelebrationPopup();

                FirebaseAuth.DefaultInstance.SignOut();

                StartCoroutine(WaitForSignOutComplete());
            });
    }

    #endregion

    #region Firebase 오류 처리

    /// <summary>
    /// Firebase에서 발생한 예외처리 한국말 번역
    /// </summary>
    /// <param name="exception">Firebase에서 발생한 예외</param>
    /// <returns>플레이어에게 보여줄 메세지</returns>
    private string HandleFirebaseException(AggregateException exception)
    {
        if (exception != null && exception.InnerExceptions.Count > 0)
        {
            string errorMessage = exception.InnerExceptions[0].Message;
            return GetUserFriendlyErrorFromMessage(errorMessage);
        }

        return "알 수 없는 오류가 발생했습니다";
    }

    /// <summary>
    /// Firebase 오류 메시지를 한국어로 변환
    /// </summary>
    /// <param name="errorMessage">원본 오류 메시지</param>
    private string GetUserFriendlyErrorFromMessage(string errorMessage)
    {
        if (string.IsNullOrEmpty(errorMessage))
            return "알 수 없는 오류가 발생했습니다";

        errorMessage = errorMessage.ToLower();

        if (errorMessage.Contains("email-already-in-use") ||
            errorMessage.Contains("email already in use") ||
            errorMessage.Contains("already in use by another account"))
        {
            return "이미 존재하는 이메일입니다";
        }
        else if (errorMessage.Contains("invalid-email"))
        {
            return "올바른 이메일 주소를 입력해 주세요";
        }
        else if (errorMessage.Contains("weak-password"))
        {
            return "비밀번호는 6자리 이상이어야 합니다";
        }
        else if (errorMessage.Contains("network") || errorMessage.Contains("connection"))
        {
            return "네트워크 연결을 확인해주세요";
        }
        else if (errorMessage.Contains("too-many-requests"))
        {
            return "너무 많은 요청이 발생했습니다. 잠시 후 다시 시도해주세요";
        }
        else
        {
            return "오류가 발생했습니다. 다시 시도해주세요";
        }
    }

    /// <summary>
    /// 회원가입 결과를 처리하고 UI를 업데이트
    /// </summary>
    /// <param name="success">회원가입 성공 여부</param>
    /// <param name="message">사용자에게 표시할 메시지</param>
    private void HandleSignUpResult(bool success, string message)
    {
        ResetSignUpUI();
        ShowPopup(message);
    }

    #endregion

    #region 입력 검증

    bool ValidateInputs()
    {
        {
            string email = emailInputField.text.Trim();

            if (!IsValidEmailFormat(email))
            {
                ShowPopup("올바른 이메일 형식을 입력해주세요");
                return false;
            }

            if (string.IsNullOrEmpty(passwordInputField.text))
            {
                ShowPopup("비밀번호를 입력해주세요");
                return false;
            }

            if (passwordInputField.text.Length < 6)
            {
                ShowPopup("비밀번호는 6자리 이상이어야 합니다");
                return false;
            }

            if (passwordInputField.text != confirmPasswordInputField.text)
            {
                ShowPopup("비밀번호가 일치하지 않습니다");
                return false;
            }

            return true;
        }
    }

    #endregion

    #region UI 관리

    /// <summary>
    /// 회원가입 UI를 초기 상태로 복원
    /// 가입 프로세스 완료 후 다시 사용할 수 있도록 버튼 활성화
    /// </summary>
    private void ResetSignUpUI()
    {
        isSigningUp = false;

        if (signUpButton != null)
        {
            signUpButton.interactable = true; 
        }
    }

    /// <summary>
    /// 취소 버튼 클릭 시 호출되는 메서드
    /// 모든 입력을 초기화하고 로그인 패널로 돌아감
    /// </summary>
    private void Cancel()
    {
        ResetInputs();

        if (loginPanel != null)
        {
            loginPanel.SetActive(true);
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 모든 입력 필드와 UI 상태를 초기화
    /// 패널을 다시 열 때 이전 입력 내용이 남아있지 않도록 보장
    /// </summary>
    private void ResetInputs()
    {
        if (emailInputField != null) emailInputField.text = "";
        if (passwordInputField != null) passwordInputField.text = "";
        if (confirmPasswordInputField != null) confirmPasswordInputField.text = "";
        ResetUI();
    }

    /// <summary>
    /// 로그아웃 완료까지 대기하는 코루틴
    /// Firebase의 비동기 로그아웃이 완료될 때까지 기다린 후 UI 전환
    /// 타임아웃 설정으로 무한 대기 방지
    /// </summary>
    private IEnumerator WaitForSignOutComplete()
    {

        float waitTime = 0f;
        const float maxWaitTime = 3f;

        while (FirebaseAuth.DefaultInstance.CurrentUser != null && waitTime < maxWaitTime)
        {
            yield return new WaitForSeconds(0.1f);
            waitTime += 0.1f;
        }
        
        yield return new WaitForSeconds(0.5f);

        // UI 상태 리셋
        ResetSignUpUI();

        // 로그인 화면으로 이동
        if (loginPanel != null)
        {
            gameObject.SetActive(false);
            loginPanel.SetActive(true);
        }
    }

    #endregion

    #region 팝업 관련 메서드

    /// <param name="message">표시할 메세지</param>
    void ShowPopup(string message)
    {
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.ShowPopup(message);
        }
    }

    void ShowCelebrationPopup()
    {
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.ShowPopup("회원 가입 성공");
        }
    }

    #endregion
}