using Photon.Pun;
using System.Collections;
using UnityEngine;

/// <summary>
/// ���� ���� �гΰ� ���� ����� �г��� ������
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
    [Tooltip("���� ���� �г� ���� �ð�")]
    [SerializeField] private float roundOverPanelDuration = 3.5f;
    public float RoundOverPanelDuration { get { return roundOverPanelDuration; } }
    [Tooltip("���� ���� �� ����� �г� Ȱ��ȭ ������")]
    [SerializeField] private float restartPanelShowDelay = 3.5f;

    Coroutine ROPanelCoroutine;
    Coroutine restartPanelCoroutine;

    private void OnEnable()
    {
        InGameManager.OnRoundEnd += RoundOverPanelShow;
        InGameManager.OnGameEnd += RestartPanelShow;
        InGameManager.OnCardSelectEnd += HideCardSelectPanel;
        InGameManager.OnMatchEnd += OnMatchEndHandler;
        InGameManager.OnCardSelectStart += ShowCardSelectPanel;
        
    }

    private void OnDisable()
    {
        InGameManager.OnRoundEnd -= RoundOverPanelShow;
        InGameManager.OnGameEnd -= RestartPanelShow;
        InGameManager.OnCardSelectEnd -= HideCardSelectPanel;
        InGameManager.OnMatchEnd -= OnMatchEndHandler;
        InGameManager.OnCardSelectStart -= ShowCardSelectPanel;
        
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

    private void ShowCardSelectPanel()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonView cardSelectPanelView = cardSelectPanel.GetComponent<PhotonView>();
            cardSelectPanelView.RPC(nameof(CardSelectUIPanelController.CardSelectUIActivate),RpcTarget.AllBuffered, true);
        }
    }

    private void OnMatchEndHandler()
    {
        if (PhotonNetwork.IsMasterClient && creator != null)
        {
            creator.MapUpdate(InGameManager.Instance.CurrentMatch);
        }
    }

    /// <summary>
    /// ���� ���� �г��� Ȱ��ȭ�ϰ� ���ӽð���ŭ ������ ���� �ٽ� ��Ȱ��ȭ�ϴ� �ڷ�ƾ
    /// </summary>
    /// <returns></returns>
    IEnumerator RoundOverPanelCoroutine()
    {
        WaitForSeconds delay = new WaitForSeconds(roundOverPanelDuration);
        PhotonView roundOverPanelView = roundOverPanel.GetComponent<PhotonView>();
        roundOverPanelView.RPC(nameof(RoundOverPanelController.RoundOverPanelActivate), RpcTarget.All, true);

        yield return delay;
        roundOverPanelView.RPC(nameof(RoundOverPanelController.RoundOverPanelActivate), RpcTarget.All, false);
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