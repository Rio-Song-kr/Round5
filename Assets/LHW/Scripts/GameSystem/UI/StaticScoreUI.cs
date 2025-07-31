using DG.Tweening;
using UnityEngine;

/// <summary>
/// 게임 플레이 내내 고정적으로 유지되는 점수 UI
/// </summary>
public class StaticScoreUI : MonoBehaviour
{
    [SerializeField] GameObject[] leftWinImages;
    [SerializeField] GameObject[] rightWinImages;

    [Header("Offset")]
    [Tooltip("스코어 획득 애니메이션 종료 후 실제 점수 이미지 반영 딜레이, 스코어 획득 애니메이션 전체 길이보다 약간 길게 설정해주세요")]
    [SerializeField] float scoreObtainDelay = 2f;

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

    /// <summary>
    /// 매 라운드마다 특정 플레이어가 1승을 할 시에 UI로 표시. 라운드의 최종 승리자가 나올 시 패배자가 1승을 했을 경우 해당 UI를 비활성화
    /// </summary>
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

    /// <summary>
    /// 특정 라운드의 최종 승리자를 UI로 반영
    /// </summary>
    private void GameScoreChange()
    {
        string currentWinner = TestIngameManager.Instance.ReadRoundScore(out int leftWinNum, out int rightWinNum);
        currentWinner = TestIngameManager.Instance.ReadScore(out int leftRoundNum, out int rightRoundNum);

        if (currentWinner == "Left")
        {
            leftWinImages[leftWinNum - 1].transform.DOScale(new Vector3(2.5f, 2.5f, 2.5f), 0.1f).SetDelay(scoreObtainDelay);
            if (rightRoundNum == 1 && rightWinImages[rightRoundNum - 1].activeSelf)
            {
                rightWinImages[rightRoundNum - 1].SetActive(false);
            }
        }
        else if (currentWinner == "Right")
        {
            rightWinImages[rightWinNum - 1].transform.DOScale(new Vector3(2.5f, 2.5f, 2.5f), 0.1f).SetDelay(scoreObtainDelay);
            if (leftRoundNum == 1 && leftWinImages[leftWinNum - 1].activeSelf)
            {
                leftWinImages[leftWinNum - 1].SetActive(false);
            }
        }

        if (leftWinNum != 3 && rightWinNum != 3)
        {
            TestIngameManager.Instance.SceneChange();
        }
    }
}