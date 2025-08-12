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

        int targetWidth = 1920;
        int targetHeight = 1080;

        Screen.SetResolution(targetWidth, targetHeight, true);
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
        yield return new WaitForSeconds(1f); // 1�� ���

        // ù �ΰ� ����
        SoundManager.Instance.PlaySFX("TeamLogoIntro");
        logoImage.color = new Color(1, 1, 1, 0);
        logo.SetActive(true);

        yield return new WaitForSeconds(2f);

        var seq = DOTween.Sequence();
        seq.Append(logoImage.DOFade(1, fadeInTime));
        seq.AppendInterval(stayTime);
        seq.Append(logoImage.DOFade(0, fadeOutTime));

        seq.OnComplete(() => { StartCoroutine(NextSequence()); });
    }

    private IEnumerator NextSequence()
    {
        logo.SetActive(false);
        AcademiLogo.color = new Color(1, 1, 1, 0);
        Academi.SetActive(true);

        var seq = DOTween.Sequence();


        seq.AppendCallback(() => SoundManager.Instance.PlaySFX("Pop"));


        seq.AppendInterval(1.1f);
        // ���̵� �� �� ���� �� ���̵� �ƿ�
        seq.Append(AcademiLogo.DOFade(1, 1.3f));
        seq.AppendInterval(stayTime);
        seq.Append(AcademiLogo.DOFade(0, fadeOutTime));

        // ���� �� 0.8�� ������ KYUNGIL ����� "����"
        seq.Insert(0.7f, DOVirtual.DelayedCall(0f, () => { SoundManager.Instance.PlaySFX("KYUNGIL"); }));

        seq.OnComplete(() =>
        {
            SceneManager.LoadScene(nextSceneName);
            SoundManager.Instance.PlayBGMLoop("LoginBGM");
        });

        // �ʿ��ϸ� �Ϸ���� ���
        yield return seq.WaitForCompletion();
    }
}