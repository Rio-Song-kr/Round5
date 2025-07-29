using UnityEngine;
using UnityEngine.UI;

public class StaticScoreUI : MonoBehaviour
{
    [SerializeField] GameObject[] leftWinImages;
    [SerializeField] GameObject[] rightWinImages;

    private void OnEnable()
    {
        Init();
        TestIngameManager.OnGameSetOver += ScoreChange;
    }

    private void OnDisable()
    {
        TestIngameManager.OnGameSetOver -= ScoreChange;
    }

    private void Init()
    {
        for(int i = 0; i < leftWinImages.Length; i++)
        {
            leftWinImages[i].SetActive(false);
            rightWinImages[i].SetActive(false);
        }
    }

    private void ScoreChange()
    {
        int leftWinNum = TestIngameManager.Instance.ReadRoundScore(out int rightWinNum);

        for(int i = 0; i < leftWinNum; i++)
        {
            leftWinImages[i].SetActive(true);
        }
        
        for(int i = 0; i < rightWinNum; i++)
        {
            rightWinImages[i].SetActive(true);
        }
    }
}