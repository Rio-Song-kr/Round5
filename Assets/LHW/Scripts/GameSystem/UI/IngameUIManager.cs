using System.Collections;
using UnityEngine;

/// <summary>
/// 라운드 종료 패널과 게임 재시작 패널을 관리함
/// </summary>
public class IngameUIManager : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] RandomMapPresetCreator creator;

    [Header("Panels")]
    [SerializeField] GameObject cardSelectPanel;
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

    /// <summary>
    /// 라운드 종료 패널을 활성화하고 지속시간만큼 유지한 다음 다시 비활성화하는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator RoundOverPanelCoroutine()
    {
        WaitForSeconds delay = new WaitForSeconds(roundOverPanelDuration);
        roundOverPanel.SetActive(true);

        yield return delay;
        HideRoundOverPanel();
        TestIngameManager.Instance.RoundStart();
        if (TestIngameManager.Instance.IsGameSetOver)
        {
            Debug.Log("새 세트 시작");
            creator.MapUpdate(TestIngameManager.Instance.CurrentGameRound);
            TestIngameManager.Instance.GameSetStart();
        }

        ROPanelCoroutine = null;
    }

    /// <summary>
    /// 게임 재시작 패널을 딜레이 시간 이후에 활성화하는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator RestartPanelCoroutine()
    {
        yield return new WaitForSeconds(restartPanelShowDelay);

        gameRestartPanel.SetActive(true);
        restartPanelCoroutine = null;
    }
}