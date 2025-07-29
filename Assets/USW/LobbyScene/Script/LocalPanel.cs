using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalPanel : MonoBehaviour
{
     
    [SerializeField] private GameObject localPanel;
    [SerializeField] private Button backButton;
    [SerializeField] private Animator localAnimator;
    [SerializeField] private Animator mainMenuAnimator;

    private void Start()
    {
        Init();
    }

    void Init()
    {
        if (backButton)
        {
            backButton.onClick.AddListener(OnBackButtonClick);
        }
    }

    public void OnBackButtonClick()
    {
        if (localAnimator)
        {
            localAnimator.SetTrigger("Lobby_BackTrigger");
        }

        if (mainMenuAnimator)
        {
            mainMenuAnimator.SetTrigger("PlayWelcomeAgain");
        }
    }
    
    void OnBackButtonClicked()
    {
        if (localPanel)
        {
            localPanel.SetActive(false);
        }
    }

   
    
    /// <summary>
    /// 퍼블릭 메서드
    /// </summary>
    public void ShowLocalPanel()
    {
        if (localPanel)
        {
            localPanel.SetActive(true);
        }
    }
}
