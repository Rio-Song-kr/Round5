using DG.Tweening;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// ���� �÷��� ���� ���������� �����Ǵ� ���� UI
/// </summary>
public class StaticScoreUI : MonoBehaviour
{
    [SerializeField] private GameObject[] leftWinImages;
    [SerializeField] private GameObject[] rightWinImages;

    [Header("Offset")]
    [Tooltip("���ھ� ȹ�� �ִϸ��̼� ���� �� ���� ���� �̹��� �ݿ� ������, ���ھ� ȹ�� �ִϸ��̼� ��ü ���̺��� �ణ ��� �������ּ���")]
    [SerializeField]
    private float scoreObtainDelay = 2f;

    private void OnEnable()
    {
        Init();
        InGameManager.OnGameStart += Init;
        InGameManager.OnRoundEnd += RoundScoreChange;
        InGameManager.OnMatchEnd += GameScoreChange;
    }

    private void OnDisable()
    {
        InGameManager.OnGameStart -= Init;
        InGameManager.OnRoundEnd -= RoundScoreChange;
        InGameManager.OnMatchEnd -= GameScoreChange;
    }

    private void Init()
    {
        for (int i = 0; i < leftWinImages.Length; i++)
        {
            leftWinImages[i].SetActive(false);
            rightWinImages[i].SetActive(false);
        }
    }

    /// <summary>
    /// �� ���帶�� Ư�� �÷��̾ 1���� �� �ÿ� UI�� ǥ��. ������ ���� �¸��ڰ� ���� �� �й��ڰ� 1���� ���� ��� �ش� UI�� ��Ȱ��ȭ
    /// </summary>
    private void RoundScoreChange()
    {
        string winner = InGameManager.Instance.LastRoundWinner;
        //todo left right 수정해야 함
        // string leftPlayerKey = PhotonNetwork.PlayerList[0].ActorNumber.ToString();
        // string rightPlayerKey= PhotonNetwork.PlayerList[1].ActorNumber.ToString();
        string leftPlayerKey = InGameManager.Instance.LeftRightActorNumber["LeftPlayer"];
        string rightPlayerKey = InGameManager.Instance.LeftRightActorNumber["RightPlayer"];
        string winnerSide = !string.IsNullOrEmpty(winner) && winner == leftPlayerKey ? "Left" : "Right";
        if (winnerSide == "Left" && InGameManager.Instance.GetPlayerRoundScore(leftPlayerKey) == 1)
        {
            for (int i = 0; i < leftWinImages.Length; i++)
            {
                if (leftWinImages[i].activeSelf) continue;
                if (!leftWinImages[i].activeSelf)
                {
                    var leftImgView = leftWinImages[i].GetComponent<PhotonView>();
                    leftImgView.RPC(nameof(WinimgUIController.WinImgUIActivate), RpcTarget.AllBuffered, true);

                    break;
                }
            }
        }
        else if (winnerSide == "Right" && InGameManager.Instance.GetPlayerRoundScore(rightPlayerKey) == 1)
        {
            for (int i = 0; i < leftWinImages.Length; i++)
            {
                if (rightWinImages[i].activeSelf) continue;
                if (!rightWinImages[i].activeSelf)
                {
                    var rightImgView = rightWinImages[i].GetComponent<PhotonView>();
                    rightImgView.RPC(nameof(WinimgUIController.WinImgUIActivate), RpcTarget.AllBuffered, true);

                    break;
                }
            }
        }
    }

    /// <summary>
    /// Ư�� ������ ���� �¸��ڸ� UI�� �ݿ�
    /// </summary>
    private void GameScoreChange()
    {
        string winner = InGameManager.Instance.LastRoundWinner;
        string leftPlayerKey = PhotonNetwork.PlayerList[0].ActorNumber.ToString();
        string rightPlayerKey = PhotonNetwork.PlayerList[1].ActorNumber.ToString();
        string winnerSide = !string.IsNullOrEmpty(winner) && winner == leftPlayerKey ? "Left" : "Right";

        if (winnerSide == "Left")
        {
            int matchWin = InGameManager.Instance.GetPlayerMatchScore(leftPlayerKey);

            if (matchWin > 2) matchWin = 2;

            var leftImgView = leftWinImages[matchWin - 1].GetComponent<PhotonView>();
            leftImgView.RPC(nameof(WinimgUIController.RoundWinImgAnimationActivate), RpcTarget.AllBuffered, scoreObtainDelay);

            if (rightWinImages[InGameManager.Instance.GetPlayerMatchScore(rightPlayerKey)].activeSelf)
            {
                var rightImgView = rightWinImages[InGameManager.Instance.GetPlayerMatchScore(rightPlayerKey)]
                    .GetComponent<PhotonView>();
                rightImgView.RPC(nameof(WinimgUIController.WinImgUIActivate), RpcTarget.AllBuffered, false);
            }
        }
        else if (winnerSide == "Right")
        {
            int matchWin = InGameManager.Instance.GetPlayerMatchScore(rightPlayerKey);

            if (matchWin > 2) matchWin = 2;

            var rightImgView = rightWinImages[matchWin - 1].GetComponent<PhotonView>();
            rightImgView.RPC(nameof(WinimgUIController.RoundWinImgAnimationActivate), RpcTarget.AllBuffered, scoreObtainDelay);

            if (leftWinImages[InGameManager.Instance.GetPlayerMatchScore(leftPlayerKey)].activeSelf)
            {
                var leftImgView = leftWinImages[InGameManager.Instance.GetPlayerMatchScore(leftPlayerKey)]
                    .GetComponent<PhotonView>();
                leftImgView.RPC(nameof(WinimgUIController.WinImgUIActivate), RpcTarget.AllBuffered, false);
            }
        }
    }
}