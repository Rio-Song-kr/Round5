using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace USW.LoginScene.Script
{
    public class TitlePanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] GameObject loginPanel;
        [SerializeField] TMP_Text titleText;
        [SerializeField] private GameObject loginsomethingPanel;
    
        [Header("Glitch Settings")]
        [SerializeField] float glitchSpeed = 1.5f;

        [SerializeField] private float delayBetweenFlitches = 0.5f;

        private void Start()
        {
            // LoginPanel 비활성화
            loginPanel.SetActive(false);
        
            // 글리치 효과 시작
            StartCoroutine(GlitchEffect(titleText));
            StartCoroutine(DelayBetweenSomething(loginsomethingPanel, delayBetweenFlitches));
        
            // 패널 전체를 버튼으로 만들기
            Button panelButton = GetComponent<Button>();
            if (panelButton == null)
            {
                panelButton = gameObject.AddComponent<Button>();
            }
            panelButton.onClick.AddListener(GoToLogin);
        }

        IEnumerator DelayBetweenSomething(GameObject target, float delay)
        {
            yield return new WaitForSeconds(delay);
            StartCoroutine(GlitchEffect(target));
        }

        IEnumerator GlitchEffect(GameObject target)
        {
            Image image = target.GetComponent<Image>();
            
            while (true)
            {
                // 알파값 0 → 1
                for (float t = 0; t <= 1; t += Time.deltaTime * glitchSpeed)
                {
                    SetImageAlpha(image,t);
                    yield return null;
                }
            
                // 알파값 1 → 0
                for (float t = 1; t >= 0; t -= Time.deltaTime * glitchSpeed)
                {
                    SetImageAlpha(image,t);
                    yield return null;
                }
            }
        }

        private IEnumerator GlitchEffect(TMP_Text text)
        {
            while (true)
            {
                // 알파값 0 → 1
                for (float t = 0; t <= 1; t += Time.deltaTime * glitchSpeed)
                {
                    SetTextAlpha(text,t);
                    yield return null;
                }
            
                // 알파값 1 → 0
                for (float t = 1; t >= 0; t -= Time.deltaTime * glitchSpeed)
                {
                    SetTextAlpha(text,t);
                    yield return null;
                }
            }
        }

        private void SetTextAlpha(TMP_Text text, float alpha)
        {
            Color color = text.color;
            color.a = alpha;
            text.color = color;
        }
        
        private void SetImageAlpha(Image image, float alpha)
        {
            Color color = image.material.color;
            color.a = alpha;
            image.material.color = color;
        }

        private void GoToLogin()
        {
            gameObject.SetActive(false);
            loginPanel.SetActive(true);
        }
    }
}