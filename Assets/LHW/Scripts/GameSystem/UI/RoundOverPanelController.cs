using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 라운드 종료시 두 플레이어의 점수를 표시하고, 애니메이션 효과를 주는 스크립트
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
    [Tooltip("매 라운드마다 나타난 승리 횟수 이미지가 줄어드는 데 걸리는 시간")]
    [SerializeField] private float roundImageShrinkDuration = 0.1f;
    [Space(15f)]
    [Tooltip("한 라운드가 완전히 끝났을 때, 애니메이션 효과로 승리자 이미지(원)가 줄어드는데 걸리는 시간")]
    [SerializeField] private float winImageShrinkDuration = 0.1f;
    [Tooltip("한 라운드가 완전히 끝났을 때, 승리자에 대한 애니메이션 효과가 시작되기 전 딜레이")]
    [SerializeField] private float winImageShrinkDelay = 1.6f;
    [Tooltip("한 라운드가 완전히 끝났을 때, 애니메이션 효과로 승리자 이미지(원)이 이동하는 데 걸리는 시간")]
    [SerializeField] private float winImageMoveDuration = 0.3f;
    [Tooltip("한 라운드가 완전히 끝났을 때, 패배자의 이미지가 화면 중앙 바닥으로 이동하고 커지는 크기")]
    [SerializeField] private float loseImageExpansionScale = 3.5f;

    Color textColor;
    string leftTextColor = "#FF8400";
    string rightTextColor = "#009EFF";

    PhotonView leftBackgroundImageView;
    PhotonView rightBackgroundImageView;

    PhotonView leftImageFillView;
    PhotonView rightImageFillView;

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
    }

    private void Init()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ImageInit();
            string winner = TestIngameManager.Instance.ReadScore(out int left, out int right);
            Debug.Log("엥");
            TextInit(winner, left, right);
            ImageInit(left, right);
        }
    }

    /// <summary>
    /// 이미지 크기 및 위치 초기화
    /// </summary>
    private void ImageInit()
    {
        leftBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageInit), RpcTarget.All);
        rightBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageInit), RpcTarget.All);
    }

    /// <summary>
    /// 승자를 표시함(텍스트)
    /// </summary>
    /// <param name="winner"></param>
    /// <param name="left"></param>
    /// <param name="right"></param>
    private void TextInit(string winner, int left, int right)
    {
        PhotonView textView = winnerText.GetComponent<PhotonView>();
        textView.RPC(nameof(WinnerTextController.RPC_TextInit), RpcTarget.All, winner, left, right);
    }

    /// <summary>
    /// 각 상황에 따른 이미지 효과를 줌
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

        if (left == 2 || right == 2)
        {
            AddScoreAnimation();
            RoundChange();
            return;
        }

        leftBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageScaleChange), RpcTarget.All, 0f, roundImageShrinkDuration, gameUIManager.RoundOverPanelDuration - roundImageShrinkDuration);
        rightBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageScaleChange), RpcTarget.All, 0f, roundImageShrinkDuration, gameUIManager.RoundOverPanelDuration - roundImageShrinkDuration);
    }

    /// <summary>
    /// 한 라운드의 최종 승리자가 결정되었을 경우 애니메이션 효과를 줌
    /// </summary>
    /// <param name="winnerImage"></param>
    private void AddScoreAnimation()
    {
        string currentWinner = TestIngameManager.Instance.ReadRoundScore(out int leftScore, out int rightScore);
        if (currentWinner == "Left")
        {
            leftBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageScaleChange), RpcTarget.AllBuffered, 0.13f, winImageShrinkDuration, winImageShrinkDelay);
            float imageShrinkTime = winImageShrinkDuration + winImageShrinkDelay;
            switch (leftScore)
            {
                case 1:
                    leftBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageMove), RpcTarget.AllBuffered, leftImageWinSpot[0].position, winImageMoveDuration, imageShrinkTime);
                    break;
                case 2:
                    leftBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageMove), RpcTarget.AllBuffered, leftImageWinSpot[1].position, winImageMoveDuration, imageShrinkTime);
                    break;
                case 3:
                    leftBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageMove), RpcTarget.AllBuffered, leftImageWinSpot[2].position, winImageMoveDuration, imageShrinkTime);
                    break;
                default:
                    break;
            }
            float winnerImageMoveTime = imageShrinkTime + winImageMoveDuration;
            rightBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageMove), RpcTarget.AllBuffered, losePosition.position, winImageMoveDuration, winnerImageMoveTime);
            rightBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageScaleChange), RpcTarget.AllBuffered, loseImageExpansionScale, winImageShrinkDuration, winnerImageMoveTime + 0.3f);
        }
        else if (currentWinner == "Right")
        {
            rightBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageScaleChange), RpcTarget.AllBuffered, 0.13f, winImageShrinkDuration, winImageShrinkDelay);
            float imageShrinkTime = winImageShrinkDuration + winImageShrinkDelay;
            switch (rightScore)
            {
                case 1:
                    rightBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageMove), RpcTarget.AllBuffered, rightImageWinSpot[0].position, winImageMoveDuration, imageShrinkTime);
                    break;
                case 2:
                    rightBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageMove), RpcTarget.AllBuffered, rightImageWinSpot[1].position, winImageMoveDuration, imageShrinkTime);
                    break;
                case 3:
                    rightBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageMove), RpcTarget.AllBuffered, rightImageWinSpot[2].position, winImageMoveDuration, imageShrinkTime);
                    break;
                default:
                    break;
            }
            float winnerImageMoveTime = imageShrinkTime + winImageMoveDuration;
            leftBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageMove), RpcTarget.AllBuffered, losePosition.position, winImageMoveDuration, winnerImageMoveTime);
            leftBackgroundImageView.RPC(nameof(WinnerImageUI.WinnerImageScaleChange), RpcTarget.AllBuffered, loseImageExpansionScale, winImageShrinkDuration, winnerImageMoveTime + 0.3f);
        }
    }

    private void RoundChange()
    {
        PhotonView sceneChangeView = sceneChangePanel.GetComponent<PhotonView>();
        sceneChangeView.RPC(nameof(SceneChangePanelController.RoundChange), RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void RoundOverPanelActivate(bool activation)
    {
        gameObject.SetActive(activation);
    }
}