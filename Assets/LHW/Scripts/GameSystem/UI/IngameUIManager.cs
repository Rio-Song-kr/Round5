using Photon.Pun;
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
        TestIngameManager.onCardSelectEnd += HideCardSelectPanel;
    }

    private void OnDisable()
    {
        TestIngameManager.OnRoundOver -= RoundOverPanelShow;
        TestIngameManager.OnGameOver -= RestartPanelShow;
        TestIngameManager.onCardSelectEnd -= HideCardSelectPanel;
    }

    private void RoundOverPanelShow()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ROPanelCoroutine = StartCoroutine(RoundOverPanelCoroutine());
        }
    }

    public void HideRoundOverPanel()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            roundOverPanel.SetActive(false);
        }
    }

    public void RestartPanelShow()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            restartPanelCoroutine = StartCoroutine(RestartPanelCoroutine());
        }
    }

    public void HideRestartPanel()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            gameRestartPanel.SetActive(false);
        }
    }

    private void HideCardSelectPanel()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonView cardSelectPanelView = cardSelectPanel.GetComponent<PhotonView>();
            cardSelectPanelView.RPC(nameof(CardSelectUIPanelController.CardSelectUIActivate), RpcTarget.AllBuffered, false);
        }
    }

    /// <summary>
    /// 라운드 종료 패널을 활성화하고 지속시간만큼 유지한 다음 다시 비활성화하는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator RoundOverPanelCoroutine()
    {
        WaitForSeconds delay = new WaitForSeconds(roundOverPanelDuration);
        PhotonView roundOverPanelView = roundOverPanel.GetComponent<PhotonView>();
        roundOverPanelView.RPC(nameof(RoundOverPanelController.RoundOverPanelActivate), RpcTarget.All, true);

        yield return delay;
        roundOverPanelView.RPC(nameof(RoundOverPanelController.RoundOverPanelActivate), RpcTarget.All, false);

        TestIngameManager.Instance.RoundStart();
        if (TestIngameManager.Instance.IsGameSetOver)
        {
            Debug.Log("새 세트 시작");
            creator.MapUpdate(TestIngameManager.Instance.CurrentGameRound);
            TestIngameManager.Instance.GameSetStart();
            if (!TestIngameManager.Instance.IsGameOver)
            {
                PhotonView cardSelectPanelView = cardSelectPanel.GetComponent<PhotonView>();
                cardSelectPanelView.RPC(nameof(CardSelectUIPanelController.CardSelectUIActivate), RpcTarget.AllBuffered, true);
            }
        }

        ROPanelCoroutine = null;
    }

    IEnumerator RestartPanelCoroutine()
    {
        yield return new WaitForSeconds(restartPanelShowDelay);

        PhotonView restartPanelView = gameRestartPanel.GetComponent<PhotonView>();
        restartPanelView.RPC(nameof(GameRestartPanelController.GameRestartPanelActivate), RpcTarget.AllBuffered, true);

        restartPanelCoroutine = null;
    }
}