using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SettingPanels : MonoBehaviour
{
    [Header("Tab Buttons")] [SerializeField]
    private Button graphicTabButton;

    [SerializeField] private Button soundTabButton;
    [SerializeField] private Button accountTabButton;
    [SerializeField] private Button backButton;

    [Header("Tab Content Panels")] [SerializeField]
    private GameObject graphicPanel;

    [SerializeField] private GameObject soundPanel;
    [SerializeField] private GameObject accountPanel;

    [Header("Tab Visual Feedback")] [SerializeField]
    private Image[] tabHighlights;

    [SerializeField] private Color activeTabColor = Color.white;
    [SerializeField] private Color inactiveTabColor = Color.gray;

    [Header("Graphic Settings")] [SerializeField]
    private Button lowQualityButton;

    [SerializeField] private Button mediumQualityButton;
    [SerializeField] private Button highQualityButton;

    [Header("Quality Button Visual")] [SerializeField]
    private Color selectedQualityColor = Color.green;

    [SerializeField] private Color unselectedQualityColor = Color.white;

    [Header("Sound Settings")] [SerializeField]
    private Slider bgmSlider;

    [SerializeField] private Slider sfxSlider;

    [Header("Account Settings")] [SerializeField]
    private TextMeshProUGUI uidText;

    [SerializeField] private TextMeshProUGUI emailText;
    [SerializeField] private TextMeshProUGUI currentNicknameText;

    [Header("Nickname Change")] [SerializeField]
    private Button editNicknameButton;


    [Header("Password Change")] [SerializeField]
    private Button editPasswordButton;

    [Header("Account Management")] [SerializeField]
    private Button deleteAccountButton;

    [SerializeField] private Button logoutButton;


    /// <summary>
    /// 설정 탭 종류
    /// </summary>
    public enum SettingTab
    {
        Graphic,
        Sound,
        Account
    }

    /// <summary>
    /// 그래픽 품질 레벨
    /// </summary>
    public enum QualityLevel
    {
        Low = 0,
        Medium = 2,
        High = 4
    }

    // 현재 활성화된 탭
    private SettingTab currentTab = SettingTab.Graphic;

    // 현재 그래픽 품질 설정
    private QualityLevel currentQuality = QualityLevel.Medium;

    // 사운드 볼륨 설정
    private float bgmVolume = 1f;
    private float sfxVolume = 1f;

    // 중복 처리 방지 플래그
    private bool isProcessingLogout = false;
    private bool isProcessingAccountDeletion = false;

    /// <summary>
    /// 초기화 및 기본 설정
    /// </summary>
    private void Start()
    {
        InitializeUI();
        SetupEvents();
        LoadSettings();
        SwitchTab(SettingTab.Sound);
    }

    /// <summary>
    /// 패널이 활성화될 때마다 계정 정보 업데이트
    /// </summary>
    private void OnEnable()
    {
        UpdateAccountInfo();
    }

    /// <summary>
    /// UI 초기 상태 설정
    /// </summary>
    private void InitializeUI()
    {
        // 모든 패널 비활성화
        graphicPanel.SetActive(false);
        soundPanel.SetActive(false);
        accountPanel.SetActive(false);
    }

    /// <summary>
    /// 모든 버튼 이벤트 등록
    /// </summary>
    private void SetupEvents()
    {
        // 탭 버튼 이벤트
        graphicTabButton.onClick.AddListener(() => SwitchTab(SettingTab.Graphic));
        soundTabButton.onClick.AddListener(() => SwitchTab(SettingTab.Sound));
        accountTabButton.onClick.AddListener(() => SwitchTab(SettingTab.Account));
        backButton.onClick.AddListener(OnBackClick);

        // 그래픽 품질 버튼 이벤트
        lowQualityButton.onClick.AddListener(() => OnQualityButtonClick(QualityLevel.Low));
        mediumQualityButton.onClick.AddListener(() => OnQualityButtonClick(QualityLevel.Medium));
        highQualityButton.onClick.AddListener(() => OnQualityButtonClick(QualityLevel.High));

        // 사운드 슬라이더 이벤트
        bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        // 닉네임 변경 버튼 이벤트
        editNicknameButton.onClick.AddListener(OnEditNicknameClick);

        // 계정 관리 버튼 이벤트
        editPasswordButton.onClick.AddListener(OnEditPasswordClick);
        deleteAccountButton.onClick.AddListener(OnDeleteAccountClick);
        logoutButton.onClick.AddListener(OnLogoutClick);
    }

    #region 탭 전환 시스템

    /// <summary>
    /// 탭 전환
    /// </summary>
    /// <param name="tab">전환할 탭</param>
    public void SwitchTab(SettingTab tab)
    {
        if (currentTab == tab) return;

        StartCoroutine(SwitchTabWithAnimation(tab));
    }

    /// <summary>
    /// 페이드 애니메이션과 함께 탭 전환
    /// </summary>
    private IEnumerator SwitchTabWithAnimation(SettingTab newTab)
    {
        SoundManager.Instance.PlaySFX("ClickSound");

        GameObject currentPanel = GetCurrentPanel();
        yield return StartCoroutine(FadePanel(currentPanel, 1f, 0f));
        currentPanel.SetActive(false);


        currentTab = newTab;
        GameObject newPanel = GetCurrentPanel();
        newPanel.SetActive(true);
        yield return StartCoroutine(FadePanel(newPanel, 0f, 1f));

        // UI 업데이트
        UpdateTabHighlights();
        OnTabSwitched(newTab);

        
    }

    /// <summary>
    /// 패널 페이드 애니메이션
    /// </summary>
    private IEnumerator FadePanel(GameObject panel, float from, float to)
    {
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }

        // 페이드의
        float duration = 0.3f;
        float startTime = Time.time;

        AnimationCurve easeInOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);


        while (Time.time - startTime < duration)
        {
            float progress = (Time.time - startTime) / duration;
            // EaseInOut 커브 적용
            //float curveValue = Mathf.SmoothStep(0f, 1f, progress);
            float curveValue = easeInOutCurve.Evaluate(progress);
            canvasGroup.alpha = Mathf.Lerp(from, to, curveValue);
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    /// <summary>
    /// 현재 탭에 해당하는 패널 반환
    /// </summary>
    private GameObject GetCurrentPanel()
    {
        switch (currentTab)
        {
            case SettingTab.Graphic: return graphicPanel;
            case SettingTab.Sound: return soundPanel;
            case SettingTab.Account: return accountPanel;
            default: return null;
        }
    }

    /// <summary>
    /// 탭 하이라이트 색상 업데이트
    /// </summary>
    private void UpdateTabHighlights()
    {
        
        for (int i = 0; i < tabHighlights.Length; i++)
        {
            tabHighlights[i].color = (i == (int)currentTab) ? activeTabColor : inactiveTabColor;
        }
    }

    /// <summary>
    /// 탭 전환 시 해당 설정 정보 업데이트
    /// </summary>
    private void OnTabSwitched(SettingTab tab)
    {
        switch (tab)
        {
            case SettingTab.Graphic:
                UpdateGraphicSettings();
                break;
            case SettingTab.Sound:
                UpdateSoundSettings();
                break;
            case SettingTab.Account:
                UpdateAccountInfo();
                break;
        }
    }

    #endregion

    #region 그래픽 설정

    /// <summary>
    /// 그래픽 설정 UI 업데이트
    /// </summary>
    private void UpdateGraphicSettings()
    {
        UpdateQualityButtonVisuals();
    }

    /// <summary>
    /// 그래픽 품질 변경
    /// </summary>
    private void OnQualityButtonClick(QualityLevel quality)
    {
        SoundManager.Instance.PlaySFX("ClickSound");
        currentQuality = quality;
        QualitySettings.SetQualityLevel((int)quality);
        PlayerPrefs.SetInt("QualityLevel", (int)quality);
        PlayerPrefs.Save();
        UpdateQualityButtonVisuals();
    }

    /// <summary>
    /// 품질 버튼들의 시각적 상태 업데이트
    /// </summary>
    private void UpdateQualityButtonVisuals()
    {
        SetQualityButtonColor(lowQualityButton, currentQuality == QualityLevel.Low);
        SetQualityButtonColor(mediumQualityButton, currentQuality == QualityLevel.Medium);
        SetQualityButtonColor(highQualityButton, currentQuality == QualityLevel.High);
    }

    /// <summary>
    /// 품질 버튼 색상 설정
    /// </summary>
    private void SetQualityButtonColor(Button button, bool isSelected)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = isSelected ? selectedQualityColor : unselectedQualityColor;
        colors.highlightedColor = isSelected ? selectedQualityColor : unselectedQualityColor;
        colors.selectedColor = isSelected ? selectedQualityColor : unselectedQualityColor;
        button.colors = colors;
    }

    #endregion

    #region 사운드 설정

    /// <summary>
    /// 사운드 설정 UI 업데이트
    /// </summary>
    private void UpdateSoundSettings()
    {
        bgmSlider.value = bgmVolume;

        sfxSlider.value = sfxVolume;
    }

    /// <summary>
    /// BGM 볼륨 변경 처리
    /// </summary>
    private void OnBGMVolumeChanged(float value)
    {
        bgmVolume = value;
        SoundManager.Instance.SetBGMVolume(bgmVolume);
        SaveAudioSettings();
    }

    /// <summary>
    /// SFX 볼륨 변경 처리
    /// </summary>
    private void OnSFXVolumeChanged(float value)
    {
        sfxVolume = value;
        SoundManager.Instance.SetSFXVolume(sfxVolume);
        SaveAudioSettings();
    }

    /// <summary>
    /// 오디오 설정 저장
    /// </summary>
    private void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }

    #endregion

    #region 계정 설정

    /// <summary>
    /// 계정 정보 UI 업데이트
    /// </summary>
    private void UpdateAccountInfo()
    {
        uidText.text = $"{FirebaseManager.GetCurrentUserID()}";
        emailText.text = $"{FirebaseManager.GetCurrentUserEmail()}";
        currentNicknameText.text = $"{FirebaseManager.GetCurrentUserDisplayName()}";
    }

    #region 닉네임 변경

    /// <summary>
    /// 닉네임 편집 모드 시작
    /// </summary>
    public void OnEditNicknameClick()
    {
        string currentNickname = FirebaseManager.GetCurrentUserDisplayName();

        PopupManager.Instance.ShowNicknameChangePopup(currentNickname, (newNickname) =>
        {
            if (newNickname == currentNickname)
            {
                return;
            }

            // Firebase 닉네임 업데이트 
            PopupManager.Instance.ShowPopup("닉네임 변경 중...");

            FirebaseManager.UpdateUserProfile(newNickname, (success) =>
            {
                if (success)
                {
                    PopupManager.Instance.ShowPopup("닉네임이 변경되었습니다.");
                    UpdateAccountInfo(); // 계정 정보 UI 업데이트

                    // GameManager에도 업데이트 반영
                    GameManager.Instance?.UpdateUserName(newNickname);
                }
                else
                {
                    PopupManager.Instance.ShowPopup("닉네임 변경에 실패했습니다.");
                }
            });
        });
    }

    #endregion

    #region 비밀번호 변경

    /// <summary>
    /// 비밀번호 변경 팝업 표시
    /// </summary>
    public void OnEditPasswordClick()
    {
        PopupManager.Instance.ShowPasswordChangePopup();
    }

    #endregion


    /// <summary>
    /// 로그아웃 요청 처리
    /// </summary>
    public void OnLogoutClick()
    {
        if (isProcessingLogout) return;
    
        PopupManager.Instance.ShowConfirmationPopup(
            "정말 로그아웃 하시겠습니까?",
            onYes: () => {
                isProcessingLogout = true;
                ProcessLogout();
            },
            onNo: () => { /* 취소한다룽 */ }
        );
    }
    

    /// <summary>
    /// 로그아웃 처리
    /// </summary>
    private void ProcessLogout()
    {
        FirebaseManager.SignOut();
        GameManager.Instance?.ClearUserInfo(); 
        CleanupDontDestroyOnLoadObjects();
        SceneManager.LoadScene("LoginScene");
    }

    /// <summary>
    /// 계정 탈퇴 요청 처리
    /// </summary>
    public void OnDeleteAccountClick()
    {
        if (isProcessingAccountDeletion) return;

        PopupManager.Instance.ShowConfirmationPopup(
            "정말로 계정을 탈퇴하시겠습니까?",
            onYes: ConfirmDeleteAccount,
            onNo: () =>
            {/* 뭐 아쉬운거지*/}
        );
    }

    /// <summary>
    /// 계정 탈퇴 확인 후 실행
    /// </summary>
    private void ConfirmDeleteAccount()
    {
        if (isProcessingAccountDeletion) return;

        isProcessingAccountDeletion = true;

        StartCoroutine(ProcessAccountDeletion());
    }

    /// <summary>
    /// 계정 탈퇴 처리 코루틴 (firebase 얘가 느려서 코루틴걸어야함.)
    /// </summary>
    private IEnumerator ProcessAccountDeletion()
    {
        bool deletionSuccess = false;
        bool deletionCompleted = false;

        // Firebase 계정 삭제 요청
        FirebaseManager.DeleteCurrentUserAccount((success) =>
        {
            deletionSuccess = success;
            deletionCompleted = true;
        });

        // 타임아웃 처리 (10초)
        float timeout = 0f;
        while (!deletionCompleted && timeout < 10f)
        {
            yield return new WaitForSeconds(0.1f);
            timeout += 0.1f;
        }


        if (deletionSuccess)
        {
            yield return new WaitForSeconds(2f);

            GameManager.Instance?.ClearUserInfo();
            CleanupDontDestroyOnLoadObjects();

            try
            {
                SceneManager.LoadScene("LoginScene");
            }
            catch (Exception e)
            {
                PopupManager.Instance.ShowPopup("Scene 전환 중 오류가 발생했습니다.");
                isProcessingAccountDeletion = false;
            }
        }
        else
        {
            PopupManager.Instance.ShowPopup("계정 탈퇴 중 오류가 발생했습니다.");
            isProcessingAccountDeletion = false;
        }
    }

    /// <summary>
    /// DontDestroyOnLoad 오브젝트 정리
    /// </summary>
    private void CleanupDontDestroyOnLoadObjects()
    {
        if (GameManager.Instance != null)
        {
            Destroy(GameManager.Instance.gameObject);
        }
    }

    #endregion

    #region 공통 UI 메서드

    /// <summary>
    /// 메시지 표시(PopupManager 사용)
    /// </summary>
    private void ShowMessage(string message)
    {
        PopupManager.Instance.ShowPopup(message);
    }

    /// <summary>
    /// 뒤로 가기 버튼 처리
    /// </summary>
    public void OnBackClick()
    {
        gameObject.SetActive(false);
    }

    #endregion

    #region 설정 로드/저장

    /// <summary>
    /// 저장된 설정 로드
    /// </summary>
    private void LoadSettings()
    {
        // 사운드 설정 로드
        bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // 그래픽 품질 설정 로드
        int qualityLevel = PlayerPrefs.GetInt("QualityLevel", 1);
        currentQuality = (QualityLevel)qualityLevel;
        QualitySettings.SetQualityLevel(qualityLevel);
    }

    #endregion

    #region 공개 메서드

    /// <summary>
    /// 설정 패널 열기
    /// </summary>
    public void OpenPanel()
    {
        gameObject.SetActive(true);
        SwitchTab(SettingTab.Sound);
    }

    /// <summary>
    /// 설정 패널 닫기
    /// </summary>
    public void ClosePanel()
    {
        OnBackClick();
    }

    /// <summary>
    /// 설정 패널 토글
    /// </summary>
    public void TogglePanel()
    {
        if (gameObject.activeInHierarchy)
        {
            ClosePanel();
        }
        else
        {
            OpenPanel();
        }
    }

    /// <summary>
    /// 그래픽 탭으로 패널 열기
    /// </summary>
    public void OpenGraphicTab()
    {
        gameObject.SetActive(true);
        SwitchTab(SettingTab.Graphic);
    }

    /// <summary>
    /// 사운드 탭으로 패널 열기
    /// </summary>
    public void OpenSoundTab()
    {
        gameObject.SetActive(true);
        SwitchTab(SettingTab.Sound);
    }

    /// <summary>
    /// 계정 탭으로 패널 열기
    /// </summary>
    public void OpenAccountTab()
    {
        gameObject.SetActive(true);
        SwitchTab(SettingTab.Account);
    }

    #endregion
}