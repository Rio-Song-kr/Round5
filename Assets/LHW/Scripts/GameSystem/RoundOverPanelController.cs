using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoundOverPanelController : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private IngameUIManager gameUIManager;

    [Header("UI")]
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private Image leftImage;
    [SerializeField] private Image leftFillImage;
    [SerializeField] private Image rightImage;
    [SerializeField] private Image rightFillImage;
    [SerializeField] private Image sceneChangePanel;

    [Header("Offset")]
    [SerializeField] private float imageShrinkDelay = 0.1f;

    Color textColor;
    string leftTextColor = "#FF8400";
    string rightTextColor = "#009EFF";

    private void OnEnable()
    {
        Init();
    }

    private void Init()
    {
        leftImage.rectTransform.localScale = Vector3.one;
        rightImage.rectTransform.localScale = Vector3.one;
        string winner = TestIngameManager.Instance.ReadScore(out int left, out int right);
        TextInit(winner, left, right);
        ImageInit(left, right);
    }

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

    private void ImageInit(int left, int right)
    {
        if(left == 0) leftFillImage.fillAmount = 0;
        else leftFillImage.fillAmount = (float)left / 2;
        
        if(right == 0) rightFillImage.fillAmount = 0;
        else rightFillImage.fillAmount = (float)right / 2;

        leftImage.rectTransform.DOScale(new Vector3(0, 0, 0), imageShrinkDelay).SetDelay(gameUIManager.RoundOverPanelDuration - imageShrinkDelay);
        rightImage.rectTransform.DOScale(new Vector3(0, 0, 0), imageShrinkDelay).SetDelay(gameUIManager.RoundOverPanelDuration - imageShrinkDelay);
    
        if(left == 2)
        {
            sceneChangePanel.transform.DOMove(transform.position, 1f).SetDelay(1f);
        }
        else if(right == 2)
        {
            sceneChangePanel.transform.DOMove(transform.position, 1f).SetDelay(1f);
        }
    }
}