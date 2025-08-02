using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 테스트용으로 만들어 둔 인게임 시스템 매니저
/// </summary>
public class TestIngameManager : MonoBehaviour
{
    #region Singleton

    public static TestIngameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Init();
    }

    #endregion

    public static event Action onCardSelectEnd;
    public static event Action OnRoundOver;
    public static event Action OnGameSetOver;
    public static event Action OnGameOver;
    public static event Action OnSkillObtained;

    private bool isCardSelectTime = false;
    public bool IsCardSelectTime { get {  return isCardSelectTime; } } 

    private bool isRoundOver = false;
    private bool isGameSetOver = false;
    public bool IsGameSetOver { get { return isGameSetOver; } }
    private bool isGameOver = false;
    public bool IsGameOver {  get { return isGameOver; } }

    private Dictionary<string, int> playerRoundScore = new Dictionary<string, int>();
    private Dictionary<string, int> playerGameScore = new Dictionary<string, int>();
    private string currentWinner;

    private int roundMaxWin = 2;
    private int GameMaxWin = 2;
    private int currentGameRound = 0;
    public int CurrentGameRound { get { return currentGameRound; } }

    // 테스트용
    private List<string> leftPlayerSkill = new List<string>();
    private List<string> rightPlayerSkill = new List<string>();

    private void Init()
    {
        playerRoundScore.Add("Left", 0);
        playerRoundScore.Add("Right", 0);
        playerGameScore.Add("Left", 0);
        playerGameScore.Add("Right", 0);
        GameStart();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            CardSelectEnd();
            RoundStart();
        }

        // 테스트용 코드
        if(Input.GetKeyDown(KeyCode.R))
        {
            RoundOver("Right");
        }
        if(Input.GetKeyDown(KeyCode.E))
        {
            RoundOver("Left");
        }
        if(Input.GetKeyDown(KeyCode.T))
        {
            ObtainSkill("Left", "1");
        }
        if(Input.GetKeyDown(KeyCode.Y))
        {
            ObtainSkill("Right", "2");
        }
    }

    public string ReadScore(out int leftScore, out int rightScore)
    {
        leftScore = playerRoundScore["Left"];
        rightScore = playerRoundScore["Right"];
        return currentWinner;
    }

    public string ReadRoundScore(out int leftScore, out int rightScore)
    {
        leftScore = playerGameScore["Left"];
        rightScore = playerGameScore["Right"];
        return currentWinner;
    }

    public void GameStart()
    {
        isCardSelectTime = true;
        isRoundOver = false;
        isGameSetOver = false;
        isGameOver = false;
        playerRoundScore["Left"] = 0;
        playerRoundScore["Right"] = 0;
        playerGameScore["Left"] = 0;
        playerGameScore["Right"] = 0;
        currentGameRound = 0;
    }

    public void CardSelectEnd()
    {
        isCardSelectTime = false;
        onCardSelectEnd?.Invoke();
    }

    public void RoundStart()
    {
        isRoundOver = false;
    }

    private void RoundOver(string winner)
    {
        isRoundOver = true;
        playerRoundScore[winner] += 1;
        currentWinner = winner;

        if(playerRoundScore["Right"] >= roundMaxWin)
        {
            GameSetOver("Right");
            currentGameRound++;
        }
        else if (playerRoundScore["Left"] >= roundMaxWin)
        {
            GameSetOver("Left");
            currentGameRound++;
        }
        Debug.Log(winner);
        OnRoundOver?.Invoke();
    }

    public void GameSetStart()
    {
        isGameSetOver = false;

        if (playerRoundScore["Right"] >= roundMaxWin || playerRoundScore["Left"] >= roundMaxWin)
        {
            playerRoundScore["Left"] = 0;
            playerRoundScore["Right"] = 0;
        }        
    }

    private void GameSetOver(string winner)
    {
        isGameSetOver = true;
        isCardSelectTime = true;
        playerGameScore[winner] += 1;
        currentWinner = winner;
        OnGameSetOver?.Invoke();
        if (playerGameScore["Left"] >= 2 || playerGameScore["Right"] >= 2)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        isGameOver = true;
        OnGameOver?.Invoke();        
    }

    #region TestCode - Card

    public string[] GetSkillInfo(string player)
    {
        if(player == "Left")
        {
            string[] skillInfo = new string[leftPlayerSkill.Count];
            for (int i = 0; i < leftPlayerSkill.Count; i++)
            {
                skillInfo[i] = leftPlayerSkill[i];
            }
            return skillInfo;
        }
        else if(player == "Right")
        {
            string[] skillInfo = new string[rightPlayerSkill.Count];
            for (int i = 0; i < rightPlayerSkill.Count; i++)
            {
                skillInfo[i] = rightPlayerSkill[i];
            }
            return skillInfo;
        }
        return null;
    }

    public void ObtainSkill(string player, string skill)
    {
        if(player == "Left")
        {
            leftPlayerSkill.Add(skill);
            Debug.Log(skill);
        }
        else if(player == "Right")
        {
            rightPlayerSkill.Add(skill);
            Debug.Log(skill);
        }
        OnSkillObtained?.Invoke();
    }

    #endregion
}