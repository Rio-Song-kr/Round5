using DG.Tweening;
using UnityEngine;

public class StaticScoreUI : MonoBehaviour
{
    [SerializeField] GameObject[] leftWinImages;
    [SerializeField] GameObject[] rightWinImages;

    [Header("Offset")]
    [SerializeField] float scoreObtainDelay = 1.6f;

    private void OnEnable()
    {
        Init();
        TestIngameManager.OnRoundOver += RoundScoreChange;
        TestIngameManager.OnGameSetOver += GameScoreChange;
    }

    private void OnDisable()
    {
        TestIngameManager.OnRoundOver -= RoundScoreChange;
        TestIngameManager.OnGameSetOver -= GameScoreChange;
    }

    private void Init()
    {
        for (int i = 0; i < leftWinImages.Length; i++)
        {
            leftWinImages[i].SetActive(false);
            rightWinImages[i].SetActive(false);
        }
    }

    private void RoundScoreChange()
    {
        string currentWinner = TestIngameManager.Instance.ReadScore(out int left, out int right);
        if (currentWinner == "Left" && left == 1)
        {
            for (int i = 0; i < leftWinImages.Length; i++)
            {
                if (leftWinImages[i].activeSelf) continue;
                if (!leftWinImages[i].activeSelf)
                {
                    leftWinImages[i].SetActive(true);
                    
                    break;
                }
            }
        }
        else if (currentWinner == "Right" && right == 1)
        {
            for (int i = 0; i < leftWinImages.Length; i++)
            {
                if (rightWinImages[i].activeSelf) continue;
                if (!rightWinImages[i].activeSelf)
                {
                    rightWinImages[i].SetActive(true);
                    
                    break;
                }
            }
        }
    }

    private void GameScoreChange()
    {
        string currentWinner = TestIngameManager.Instance.ReadRoundScore(out int leftWinNum, out int rightWinNum);
        currentWinner = TestIngameManager.Instance.ReadScore(out int leftRoundNum, out int rightRoundNum);

        if (currentWinner == "Left")
        {
            leftWinImages[leftWinNum - 1].transform.DOScale(new Vector3(2.5f, 2.5f, 2.5f), 0.1f).SetDelay(scoreObtainDelay);
            if (rightRoundNum == 1)
            {
                for (int i = rightWinImages.Length - 1; i >= 0; i--)
                {
                    if (rightWinImages[i].activeSelf)
                    {
                        rightWinImages[i].SetActive(false);
                        break;
                    }
                }
            }
        }
        else if (currentWinner == "Right")
        {
            rightWinImages[rightWinNum - 1].transform.DOScale(new Vector3(2.5f, 2.5f, 2.5f), 0.1f).SetDelay(scoreObtainDelay);
            if (leftRoundNum == 1)
            {
                for (int i = leftWinImages.Length - 1; i >= 0; i--)
                {
                    if (leftWinImages[i].activeSelf)
                    {
                        leftWinImages[i].SetActive(false);
                        break;
                    }
                }
            }
        }
        TestIngameManager.Instance.SceneChange();
    }
}