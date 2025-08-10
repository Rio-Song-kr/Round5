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

        yield return new WaitForSeconds(2f);

        Sequence seq = DOTween.Sequence();
        seq.Append(logoImage.DOFade(1, fadeInTime));
        seq.AppendInterval(stayTime);
        seq.Append(logoImage.DOFade(0, fadeOutTime));

        seq.OnComplete(() =>
        {
            StartCoroutine(NextSequence());
        });
    }

    IEnumerator NextSequence()
    {
        logo.SetActive(false);
        AcademiLogo.color = new Color(1, 1, 1, 0);
        Academi.SetActive(true);

        Sequence seq = DOTween.Sequence();

        
        
        seq.AppendCallback(() => SoundManager.Instance.PlaySFX("Pop"));


        seq.AppendInterval(1.1f);
        // 페이드 인 → 유지 → 페이드 아웃
        seq.Append(AcademiLogo.DOFade(1, 1.3f));
        seq.AppendInterval(stayTime);
        seq.Append(AcademiLogo.DOFade(0, fadeOutTime));

        // 시작 후 0.8초 지점에 KYUNGIL 재생을 "삽입"
        seq.Insert(0.7f, DOVirtual.DelayedCall(0f, () =>
        {
            SoundManager.Instance.PlaySFX("KYUNGIL");
        }));

        seq.OnComplete(() =>
        {
            SceneManager.LoadScene(nextSceneName);
            SoundManager.Instance.PlayBGMLoop("LoginBGM");
        });

        // 필요하면 완료까지 대기
        yield return seq.WaitForCompletion();
    }
}
