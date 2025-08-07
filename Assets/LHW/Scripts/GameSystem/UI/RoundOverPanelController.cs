using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ���� ����� �� �÷��̾��� ������ ǥ���ϰ�, �ִϸ��̼� ȿ���� �ִ� ��ũ��Ʈ
/// </summary>
public class RoundOverPanelController : MonoBehaviourPun
{
    [Header("Reference")]
    [SerializeField] private IngameUIManager gameUIManager;

    [Header("UI")]
    [SerializeField] private TMP_Text winnerText;
    [Space(15f)]
    [SerializeField] private Image leftImage;
    [SerializeField] private Image leftFillImage;
    [SerializeField] private Transform leftInitTransform;
    [Space(15f)]
    [SerializeField] private Image rightImage;
    [SerializeField] private Image rightFillImage;
    [SerializeField] private Transform rightInitTransform;
    [Space(15f)]
    [SerializeField] private Image sceneChangePanel;
    [Space(15f)]
    [SerializeField] private Transform[] leftImageWinSpot;
    [SerializeField] private Transform[] rightImageWinSpot;
    [SerializeField] private Transform losePosition;

    [Header("Offset")]
    [Tooltip("�� ���帶�� ��Ÿ�� �¸� Ƚ�� �̹����� �پ��� �� �ɸ��� �ð�")]
    [SerializeField] private float roundImageShrinkDuration = 0.1f;
    [Space(15f)]
    [Tooltip("�� ���尡 ������ ������ ��, �ִϸ��̼� ȿ���� �¸��� �̹���(��)�� �پ��µ� �ɸ��� �ð�")]
    [SerializeField] private float winImageShrinkDuration = 0.1f;
    [Tooltip("�� ���尡 ������ ������ ��, �¸��ڿ� ���� �ִϸ��̼� ȿ���� ���۵Ǳ� �� ������")]
    [SerializeField] private float winImageShrinkDelay = 1.6f;
    [Tooltip("�� ���尡 ������ ������ ��, �ִϸ��̼� ȿ���� �¸��� �̹���(��)�� �̵��ϴ� �� �ɸ��� �ð�")]
    [SerializeField] private float winImageMoveDuration = 0.3f;
    [Tooltip("�� ���尡 ������ ������ ��, �й����� �̹����� ȭ�� �߾� �ٴ����� �̵��ϰ� Ŀ���� ũ��")]
    [SerializeField] private float loseImageExpansionScale = 3.5f;

    private Color textColor;
    private string leftTextColor = "#FF8400";
    private string rightTextColor = "#009EFF";

    private PhotonView leftBackgroundImageView;
    private PhotonView rightBackgroundImageView;

    private PhotonView leftImageFillView;
    private PhotonView rightImageFillView;

    private void Awake()
    {
        leftBackgroundImageView = leftImage.GetComponent<PhotonView>();
        rightBackgroundImageView = rightImage.GetComponent<PhotonView>();

        leftImageFillView = leftFillImage.GetComponent<PhotonView>();
        rightImageFillView = rightFillImage.GetComponent<PhotonView>();

        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        Init();
        InGameManager.OnMatchEnd += AddScoreAnimation;
        InGameManager.OnMatchEnd += RoundChange;
    }
    private void OnDisable()
    {
        InGameManager.OnMatchEnd -= AddScoreAnimation;
        InGameManager.OnMatchEnd -= RoundChange;
    }

    private void Init()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ImageInit();
            ReadScore(out int left, out int right);
            // Debug.Log("��");
            string winner = InGameManager.Instance.LastRoundWinner;
            string leftPlayerKey = PhotonNetwork.PlayerList[0].ActorNumber.ToString();
            string winnerSide = !string.IsNullOrEmpty(winner) && winner == leftPlayerKey ? "Left" : "Right";
            TextInit(winnerSide, left, right);
            ImageInit(left, right);
        }
    }
    /// <summary>
    /// 라운드 점수 가져오는 메서드
    /// </summary>
    private void ReadScore(out int leftScore, out int rightScore)
    {
        string leftPlayerKey = PhotonNetwork.PlayerList[0].ActorNumber.ToString();
        string rightPlayerKey = PhotonNetwork.PlayerList[1].ActorNumber.ToString();

        leftScore = InGameManager.Instance.GetPlayerRoundScore(leftPlayerKey);
        rightScore = InGameManager.Instance.GetPlayerRoundScore(rightPlayerKey);
    }

    /// <summary>
    /// 매치 점수를 읽어오는 메서드 
    /// </summary>
    private void ReadRoundScore(out int leftScore, out int rightScore)
    {
        string leftPlayerKey = PhotonNetwork.PlayerList[0].ActorNumber.ToString();
        string rightPlayerKey = PhotonNetwork.PlayerList[1].ActorNumber.ToString();

        leftScore = InGameManager.Instance.GetPlayerMatchScore(leftPlayerKey);
        rightScore = InGameManager.Instance.GetPlayerMatchScore(rightPlayerKey);
    }

    /// <summary>
    /// �̹��� ũ�� �� ��ġ �ʱ�ȭ
    /// </summary>
    private void ImageInit()
    {
        leftBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageInit), RpcTarget.All);
        rightBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageInit), RpcTarget.All);
    }

    /// <summary>
    /// ���ڸ� ǥ����(�ؽ�Ʈ)
    /// </summary>
    /// <param name="winner"></param>
    /// <param name="left"></param>
    /// <param name="right"></param>
    private void TextInit(string winner, int left, int right)
    {
        var textView = winnerText.GetComponent<PhotonView>();
        textView.RPC(nameof(WinnerTextController.RPC_TextInit), RpcTarget.All, winner, left, right);
    }

    /// <summary>
    /// �� ��Ȳ�� ���� �̹��� ȿ���� ��
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    private void ImageInit(int left, int right)
    {
        if (left == 0)
        {
            leftImageFillView.RPC(nameof(WinnerImageFillingUI.FillAmountInit), RpcTarget.All, 0f);
        }
        else
        {
            leftImageFillView.RPC(nameof(WinnerImageFillingUI.FillAmountInit), RpcTarget.All, (float)left / 2);
        }

        if (right == 0)
        {
            rightImageFillView.RPC(nameof(WinnerImageFillingUI.FillAmountInit), RpcTarget.All, 0f);
        }
        else
        {
            rightImageFillView.RPC(nameof(WinnerImageFillingUI.FillAmountInit), RpcTarget.All, (float)right / 2);
        }

        leftBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageScaleChange), RpcTarget.All, 0f, roundImageShrinkDuration,
            gameUIManager.RoundOverPanelDuration - roundImageShrinkDuration);
        rightBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageScaleChange), RpcTarget.All, 0f, roundImageShrinkDuration,
            gameUIManager.RoundOverPanelDuration - roundImageShrinkDuration);
    }

    /// <summary>
    /// �� ������ ���� �¸��ڰ� �����Ǿ��� ��� �ִϸ��̼� ȿ���� ��
    /// </summary>
    /// <param name="winnerImage"></param>
    private void AddScoreAnimation()
    {
        ReadRoundScore(out int leftScore, out int rightScore);

        string matchWinner = InGameManager.Instance.LastRoundWinner;
        string leftPlayerKey = PhotonNetwork.PlayerList[0].ActorNumber.ToString();
        string currentWinner = matchWinner == leftPlayerKey ? "Left" : "Right";

        // Debug.Log(currentWinner);
        if (currentWinner == "Left")
        {
            leftBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageScaleChange), RpcTarget.AllBuffered, 0.13f,
                winImageShrinkDuration, winImageShrinkDelay);
            float imageShrinkTime = winImageShrinkDuration + winImageShrinkDelay;
            switch (leftScore)
            {
                case 1:
                    leftBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageMove), RpcTarget.AllBuffered,
                        leftImageWinSpot[0].position, winImageMoveDuration, imageShrinkTime);
                    break;
                case 2:
                    leftBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageMove), RpcTarget.AllBuffered,
                        leftImageWinSpot[1].position, winImageMoveDuration, imageShrinkTime);
                    break;
                case 3:
                    leftBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageMove), RpcTarget.AllBuffered,
                        leftImageWinSpot[2].position, winImageMoveDuration, imageShrinkTime);
                    break;
                default:
                    break;
            }
            float winnerImageMoveTime = imageShrinkTime + winImageMoveDuration;
            rightBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageMove), RpcTarget.AllBuffered, losePosition.position,
                winImageMoveDuration, winnerImageMoveTime);
            rightBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageScaleChange), RpcTarget.AllBuffered,
                loseImageExpansionScale, winImageShrinkDuration, winnerImageMoveTime + 0.3f);
        }
        else if (currentWinner == "Right")
        {
            rightBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageScaleChange), RpcTarget.AllBuffered, 0.13f,
                winImageShrinkDuration, winImageShrinkDelay);
            float imageShrinkTime = winImageShrinkDuration + winImageShrinkDelay;
            switch (rightScore)
            {
                case 1:
                    rightBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageMove), RpcTarget.AllBuffered,
                        rightImageWinSpot[0].position, winImageMoveDuration, imageShrinkTime);
                    break;
                case 2:
                    rightBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageMove), RpcTarget.AllBuffered,
                        rightImageWinSpot[1].position, winImageMoveDuration, imageShrinkTime);
                    break;
                case 3:
                    rightBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageMove), RpcTarget.AllBuffered,
                        rightImageWinSpot[2].position, winImageMoveDuration, imageShrinkTime);
                    break;
                default:
                    break;
            }
            float winnerImageMoveTime = imageShrinkTime + winImageMoveDuration;
            leftBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageMove), RpcTarget.AllBuffered, losePosition.position,
                winImageMoveDuration, winnerImageMoveTime);
            leftBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageScaleChange), RpcTarget.AllBuffered,
                loseImageExpansionScale, winImageShrinkDuration, winnerImageMoveTime + 0.3f);
        }
    }

    private void RoundChange()
    {
        var sceneChangeView = sceneChangePanel.GetComponent<PhotonView>();
        sceneChangeView.RPC(nameof(SceneChangePanelController.RoundChange), RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void RoundOverPanelActivate(bool activation)
    {
        gameObject.SetActive(activation);
    }
}