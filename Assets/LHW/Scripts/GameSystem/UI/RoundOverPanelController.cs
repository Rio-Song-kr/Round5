using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 라운드 종료시 두 플레이어의 점수를 표시하고, 애니메이션 효과를 주는 스크립트
/// </summary>
public class RoundOverPanelController : MonoBehaviour
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

    private void OnEnable()
    {
        Init();
    }

    private void Init()
    {
        ImageInit();
        string winner = TestIngameManager.Instance.ReadScore(out int left, out int right);
        TextInit(winner, left, right);
        ImageInit(left, right);
    }

    /// <summary>
    /// 이미지 크기 및 위치 초기화
    /// </summary>
    private void ImageInit()
    {
        leftImage.rectTransform.localScale = Vector3.one;
        leftImage.rectTransform.position = leftInitTransform.position;
        rightImage.rectTransform.localScale = Vector3.one;
        rightImage.rectTransform.position = rightInitTransform.position;
    }

    /// <summary>
    /// 승자를 표시함(텍스트
    /// </summary>
    /// <param name="winner"></param>
    /// <param name="left"></param>
    /// <param name="right"></param>
    private void TextInit(string winner, int left, int right)
    {
        if(winner == "Left")
        {
            if (ColorUtility.TryParseHtmlString(leftTextColor, out textColor))
            {
                winnerText.color = textColor;
            }

            if(left == 1)
            {
                winnerText.text = "Half Orange";
            }
            else if (left == 2)
            {
                winnerText.text = "Round Orange";
            }
        }
        else if (winner == "Right")
        {
            if (ColorUtility.TryParseHtmlString(rightTextColor, out textColor))
            {
                winnerText.color = textColor;
            }

            if (right == 1)
            {
                winnerText.text = "Half Blue";
            }
            else if (right == 2)
            {
                winnerText.text = "Round Blue";
            }
        }
    }

    /// <summary>
    /// 각 상황에 따른 이미지 효과를 줌
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    private void ImageInit(int left, int right)
    {
        if(left == 0) leftFillImage.fillAmount = 0;
        else leftFillImage.fillAmount = (float)left / 2;
        
        if(right == 0) rightFillImage.fillAmount = 0;
        else rightFillImage.fillAmount = (float)right / 2;
    
        if(left == 2)
        {
            sceneChangePanel.transform.DOMove(transform.position, 1f).SetDelay(1f);
            AddScoreAnimation(leftImage);
            return;
        }
        else if(right == 2)
        {
            sceneChangePanel.transform.DOMove(transform.position, 1f).SetDelay(1f);
            AddScoreAnimation(rightImage);
            return;
        }

        leftImage.rectTransform.DOScale(new Vector3(0, 0, 0), roundImageShrinkDuration).SetDelay(gameUIManager.RoundOverPanelDuration - roundImageShrinkDuration);
        rightImage.rectTransform.DOScale(new Vector3(0, 0, 0), roundImageShrinkDuration).SetDelay(gameUIManager.RoundOverPanelDuration - roundImageShrinkDuration);
    }

    /// <summary>
    /// 한 라운드의 최종 승리자가 결정되었을 경우 애니메이션 효과를 줌
    /// </summary>
    /// <param name="winnerImage"></param>
    private void AddScoreAnimation(Image winnerImage)
    {
        string currentWinner = TestIngameManager.Instance.ReadRoundScore(out int leftScore, out int rightScore);
        Debug.Log(leftScore);
        if (currentWinner == "Left")
        {
            leftImage.transform.DOScale(new Vector3(0.13f, 0.13f, 0.13f), winImageShrinkDuration).SetDelay(winImageShrinkDelay);
            float imageShrinkTime = winImageShrinkDuration + winImageShrinkDelay;
            switch (leftScore)
            {
                case 1:
                    leftImage.rectTransform.DOMove(leftImageWinSpot[0].position, winImageMoveDuration).SetDelay(imageShrinkTime);
                    break;
                case 2:
                    leftImage.rectTransform.DOMove(leftImageWinSpot[1].position, winImageMoveDuration).SetDelay(imageShrinkTime);
                    break;
                case 3:
                    leftImage.rectTransform.DOMove(leftImageWinSpot[2].position, winImageMoveDuration).SetDelay(imageShrinkTime);
                    break;
                default:
                    break;
            }
            float winnerImageMoveTime = imageShrinkTime + winImageMoveDuration;
            rightImage.transform.DOMove(losePosition.position, winImageMoveDuration).SetDelay(winnerImageMoveTime);
            rightImage.transform.DOScale(new Vector3(loseImageExpansionScale, loseImageExpansionScale, loseImageExpansionScale), winImageShrinkDuration).SetDelay(winnerImageMoveTime + 0.3f);
        }
        else if (currentWinner == "Right")
        {
            rightImage.transform.DOScale(new Vector3(0.13f, 0.13f, 0.13f), winImageShrinkDuration).SetDelay(winImageShrinkDelay);
            float imageShrinkTime = winImageShrinkDuration + winImageShrinkDelay;
            switch (rightScore)
            {
                case 1:
                    rightImage.rectTransform.DOMove(rightImageWinSpot[0].position, winImageMoveDuration).SetDelay(imageShrinkTime);
                    break;
                case 2:
                    rightImage.rectTransform.DOMove(rightImageWinSpot[1].position, winImageMoveDuration).SetDelay(imageShrinkTime);
                    break;
                case 3:
                    rightImage.rectTransform.DOMove(rightImageWinSpot[2].position, winImageMoveDuration).SetDelay(imageShrinkTime);
                    break;
                default:
                    break;
            }
            float winnerImageMoveTime = imageShrinkTime + winImageMoveDuration;
            leftImage.transform.DOMove(losePosition.position, winImageShrinkDuration).SetDelay(winnerImageMoveTime);
            leftImage.transform.DOScale(new Vector3(loseImageExpansionScale, loseImageExpansionScale, loseImageExpansionScale), winImageShrinkDuration).SetDelay(winnerImageMoveTime + 0.3f);
        }
    }
}