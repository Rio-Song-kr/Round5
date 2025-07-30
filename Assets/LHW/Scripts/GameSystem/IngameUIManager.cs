using System.Collections;
using UnityEngine;

public class IngameUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject roundOverPanel;
    [SerializeField] GameObject gameRestartPanel;

    [Header("Offset")]
    [SerializeField] private float roundOverPanelDuration = 3.5f;
    public float RoundOverPanelDuration { get { return roundOverPanelDuration; } }

    Coroutine ROPanelCoroutine;

    private void OnEnable()
    {
        TestIngameManager.OnRoundOver += RoundOverPanelShow;
    }

    private void OnDisable()
    {
        TestIngameManager.OnRoundOver -= RoundOverPanelShow;
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
        gameRestartPanel.SetActive(true);
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
        if(TestIngameManager.Instance.IsGameSetOver)
        {
            TestIngameManager.Instance.GameSetStart();
        }

        ROPanelCoroutine = null;
    }
}