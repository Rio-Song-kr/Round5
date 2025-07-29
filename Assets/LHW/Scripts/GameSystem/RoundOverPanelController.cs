using Photon.Pun.Demo.PunBasics;
using System.Collections;
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
    [SerializeField] private Image LeftFillImage;
    [SerializeField] private Image rightImage;
    [SerializeField] private Image RightFillImage;

    Color textColor;
    string leftTextColor = "#FF8400";
    string rightTextColor = "#009EFF";

    private Coroutine shrink;

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
        if(left == 0) LeftFillImage.fillAmount = 0;
        else LeftFillImage.fillAmount = (float)left / 2;
        
        if(right == 0) RightFillImage.fillAmount = 0;
        else RightFillImage.fillAmount = (float)right / 2;
    }

    public void ShrinkImage()
    {
        shrink = StartCoroutine(ShrinkCoroutine());
    }

    IEnumerator ShrinkCoroutine()
    { 
        float postDelay = 0.05f;
        float elapsedTime = 0;
        while(elapsedTime < postDelay)
        {
            elapsedTime += Time.deltaTime;
            float t = 1 - elapsedTime / postDelay;

            leftImage.rectTransform.localScale = new Vector3(t, t, t);
            rightImage.rectTransform.localScale = new Vector3(t, t, t);
            yield return null;
        }
        leftImage.rectTransform.localScale = new Vector3(0, 0, 0);
        rightImage.rectTransform.localScale = new Vector3(0, 0, 0);
        gameUIManager.HideRoundOverPanel();

        shrink = null;
    }
}