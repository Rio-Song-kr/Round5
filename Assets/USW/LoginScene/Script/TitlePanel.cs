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
    
        [Header("Glitch Settings")]
        [SerializeField] float glitchSpeed = 2f;

        private void Start()
        {
            // LoginPanel 비활성화
            loginPanel.SetActive(false);
        
            // 글리치 효과 시작
            StartCoroutine(GlitchEffect());
        
            // 패널 전체를 버튼으로 만들기
            Button panelButton = GetComponent<Button>();
            if (panelButton == null)
            {
                panelButton = gameObject.AddComponent<Button>();
            }
            panelButton.onClick.AddListener(GoToLogin);
        }

        private IEnumerator GlitchEffect()
        {
            while (true)
            {
                // 알파값 0 → 1
                for (float t = 0; t <= 1; t += Time.deltaTime * glitchSpeed)
                {
                    SetAlpha(t);
                    yield return null;
                }
            
                // 알파값 1 → 0
                for (float t = 1; t >= 0; t -= Time.deltaTime * glitchSpeed)
                {
                    SetAlpha(t);
                    yield return null;
                }
            }
        }

        private void SetAlpha(float alpha)
        {
            Color color = titleText.color;
            color.a = alpha;
            titleText.color = color;
        }

        private void GoToLogin()
        {
            gameObject.SetActive(false);
            loginPanel.SetActive(true);
        }
    }
}