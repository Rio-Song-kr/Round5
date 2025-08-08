using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class SplashManagerver2 : MonoBehaviour
{
    [SerializeField] private Image logoImage;
    [SerializeField] private GameObject logo;
    [SerializeField] private Image AcademiLogo;
    [SerializeField] private GameObject Academi;

    [SerializeField] private float fadeInTime = 1f;
    [SerializeField] private float stayTime = 1.5f;
    [SerializeField] private float fadeOutTime = 1f;
    [SerializeField] private string nextSceneName = "LoginScene";

    private void Start()
    {
        StartCoroutine(PlaySplashSequence());

        
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(nextSceneName);
            SoundManager.Instance.PlayBGMLoop("LoginBGM");
        }
    }

    private IEnumerator PlaySplashSequence()
    {
        yield return new WaitForSeconds(1f); // 1초 대기

        // 첫 로고 시작
        SoundManager.Instance.PlaySFX("TeamLogoIntro");
        logoImage.color = new Color(1, 1, 1, 0);
        logo.SetActive(true);

        Sequence seq = DOTween.Sequence();
        seq.Append(logoImage.DOFade(1, fadeInTime));
        seq.AppendInterval(stayTime);
        seq.Append(logoImage.DOFade(0, fadeOutTime));

        // 첫 시퀀스 끝난 뒤 두 번째 시작
        seq.OnComplete(() =>
        {
            logo.SetActive(false); // 첫 로고 비활성화
            AcademiLogo.color = new Color(1, 1, 1, 0);
            Academi.SetActive(true);

            SoundManager.Instance.PlaySFX("Drum");

            SoundManager.Instance.PlaySFX("KYUNGIL");
            Sequence seq2 = DOTween.Sequence();
            seq2.Append(AcademiLogo.DOFade(1, fadeInTime));
            seq2.AppendInterval(stayTime);
            seq2.Append(AcademiLogo.DOFade(0, fadeOutTime));
            seq2.OnComplete(() =>
            {
                SceneManager.LoadScene(nextSceneName);
                SoundManager.Instance.PlayBGMLoop("LoginBGM");
            });
        });
    }
}
