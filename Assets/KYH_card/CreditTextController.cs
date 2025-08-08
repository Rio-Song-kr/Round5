using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class CreditTextController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI creditText;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float stayTime = 1f;
    [SerializeField] private string nextSceneName = "LoginScene";
    [SerializeField] private float delayBeforeStart = 3.5f; // Optional: 몇 초 뒤에 실행할지
    // [SerializeField] GameObject creditTextObject;

    private void Start()
    {
        creditText.alpha = 0;
        Invoke(nameof(PlayCreditSequence), delayBeforeStart);
    }

    private void PlayCreditSequence()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(creditText.DOFade(1f, fadeDuration));
        seq.AppendInterval(stayTime);
        seq.Append(creditText.DOFade(0f, fadeDuration));
        seq.OnComplete(() =>
        {
            SceneManager.LoadScene(nextSceneName);
        });
    }
}
