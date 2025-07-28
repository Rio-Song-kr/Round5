using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// UI 정보를 참조해서 불러와 실행하는 스크립트
/// </summary>
public class LoadingUIManager : MonoBehaviour
{
    public static LoadingUIManager Instance;

    [Header("UI 참조")]
    [SerializeField] private GameObject loadingPanel;           // 전체 패널
    [SerializeField] private Slider progressBar;                // 진행률 슬라이더  
    [SerializeField] private TextMeshProUGUI progressText;      // 퍼센트 표시 텍스트

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); //  씬 이동시 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Show()
    {
        if (loadingPanel != null)
        {
            Debug.Log("로딩 패널 활성화 되어 출력됨");
            loadingPanel.SetActive(true);
        }

        if (progressBar != null)
        {
            progressBar.value = 0f; // 초기화
        }
        if (progressText != null)
        {
            progressText.text = "Loading...0%"; // 텍스트도 초기화
        }
        // 배경 애니메이션 초기화
        CardAnimator animator = GetComponentInChildren<CardAnimator>();
        if (animator != null)
        {
            animator.RestartAnimation(); 
        }
    }

    public void Hide()
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
    }

    public void UpdateProgress(float progress)
    {
        if (progressBar != null)
        {
            progressBar.DOValue(progress, 0.3f).SetEase(Ease.OutQuad);
        }

        

        if (progressText != null)
        {
            progressText.text = $"Loading...{Mathf.RoundToInt(progress * 100)}%";
        }

        // 프로그레스가 100%에 도달했을 경우 자동으로 패널 숨김
        if (progress >= 1f && loadingPanel != null && loadingPanel.activeSelf)
        {
            Hide();
        }
    }

    
}
