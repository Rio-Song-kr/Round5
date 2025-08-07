using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SinglePanel : MonoBehaviour
{
     
    [SerializeField] private GameObject singlePanel;
    [SerializeField] private Button backButton;
    [SerializeField] private Button weaponSceneButton;
    [SerializeField] private Button ropeSceneButton;
    [SerializeField] private Animator singleAnimator;
    [SerializeField] private Animator mainMenuAnimator;
    [SerializeField] private float animationLength = 1f;
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

        if (ropeSceneButton)
        {
            ropeSceneButton.onClick.AddListener(OnRopeButtonClick);
        }

        if (weaponSceneButton)
        {
            weaponSceneButton.onClick.AddListener(OnWeaponButtonClick);
        }
    }

    public void OnBackButtonClick()
    {
        if (singleAnimator)
        {
            singleAnimator.SetTrigger("SingleButton_BackTrigger");
        }

        if (mainMenuAnimator)
        {
            mainMenuAnimator.SetTrigger("PlayWelcomeAgain");
        }
    }
    
    void OnBackButtonClicked()
    {
        if (singlePanel)
        {
            singlePanel.SetActive(false);
        }
    }

    public void OnRopeButtonClick()
    {
        Debug.Log("찍힘");
        StartCoroutine(PlayAnimationAndLoadScene("USW_SandBoxScene"));
    }

    public void OnWeaponButtonClick()
    {
        Debug.Log("이것도찍힘");
        StartCoroutine(PlayAnimationAndLoadScene("USW_SandBox"));
    }

    IEnumerator PlayAnimationAndLoadScene(string sceneName)
    {
        if (singleAnimator)
        {
            singleAnimator.SetTrigger("SingleButton_SceneTrigger");
        }
        
        yield return new WaitForSeconds(animationLength);
        
        SceneManager.LoadScene(sceneName);
    }
    

   
    
    /// <summary>
    /// 퍼블릭 메서드
    /// </summary>
    public void ShowSinglePanel()
    {
        if (singlePanel)
        {
            singlePanel.SetActive(true);
        }
    }
}
