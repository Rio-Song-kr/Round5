using System.Collections;
using UnityEngine;

public class IngameUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject roundOverPanel;
    [SerializeField] GameObject gameRestartPanel;

    [Header("Offset")]
    [Tooltip("라운드 종료 패널 지속 시간")]
    [SerializeField] private float roundOverPanelDuration = 3.5f;
    public float RoundOverPanelDuration { get { return roundOverPanelDuration; } }
    [Tooltip("게임 종료 후 재시작 패널 활성화 딜레이")]
    [SerializeField] private float restartPanelShowDelay = 3.5f;

    Coroutine ROPanelCoroutine;
    Coroutine restartPanelCoroutine;

    private void OnEnable()
    {
        TestIngameManager.OnRoundOver += RoundOverPanelShow;
        TestIngameManager.OnGameOver += RestartPanelShow;
    }

    private void OnDisable()
    {
        TestIngameManager.OnRoundOver -= RoundOverPanelShow;
        TestIngameManager.OnGameOver -= RestartPanelShow;
    }

    private void RoundOverPanelShow()
    {
        ROPanelCoroutine = StartCoroutine(RoundOverPanelCoroutine());
    }

    public void HideRoundOverPanel()
    {
        roundOverPanel.SetActive(false);
    }

    public void RestartPanelShow()
    {
        restartPanelCoroutine = StartCoroutine(RestartPanelCoroutine());
    }

    public void HideRestartPanel()
    {
        gameRestartPanel.SetActive(false);
    }

    IEnumerator RoundOverPanelCoroutine()
    {
        WaitForSeconds delay = new WaitForSeconds(roundOverPanelDuration);
        roundOverPanel.SetActive(true);

        yield return delay;
        HideRoundOverPanel();
        TestIngameManager.Instance.RoundStart();
        if (TestIngameManager.Instance.IsGameSetOver)
        {
            TestIngameManager.Instance.GameSetStart();
        }

        ROPanelCoroutine = null;
    }

    IEnumerator RestartPanelCoroutine()
    {
        yield return new WaitForSeconds(restartPanelShowDelay);

        gameRestartPanel.SetActive(true);
        restartPanelCoroutine = null;
    }
}