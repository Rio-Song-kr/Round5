using UnityEngine;

public class IngameUIManager : MonoBehaviour
{
    [SerializeField] GameObject roundOverPanel;
    [SerializeField] GameObject gameRestartPanel;

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
        roundOverPanel.SetActive(true);
    }

    public void HideRoundOverPanel()
    {
        roundOverPanel.SetActive(false);
    }

    private void RestartPanelShow()
    {
        gameRestartPanel.SetActive(true);
    }

    public void HideRestartPanel()
    {
        gameRestartPanel.SetActive(false);
    }
}