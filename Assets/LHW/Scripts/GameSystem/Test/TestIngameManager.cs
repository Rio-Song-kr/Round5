using System;
using System.Collections.Generic;
using UnityEngine;

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

    public static event Action OnRoundOver;
    public static event Action OnGameSetOver;

    private bool isRoundOver = false;
    private bool isGameSetOver = false;
    private bool isGameOver = false;
    public bool IsGameOver {  get { return isGameOver; } }

    private Dictionary<string, int> playerRoundScore = new Dictionary<string, int>();
    private Dictionary<string, int> playerGameScore = new Dictionary<string, int>();
    private string currentWinner;

    private void Init()
    {
        playerRoundScore.Add("Left", 0);
        playerRoundScore.Add("Right", 0);
        playerGameScore.Add("Left", 0);
        playerGameScore.Add("Right", 0);
    }

    void Update()
    {
        // 테스트용 코드
        if(Input.GetKeyDown(KeyCode.R))
        {
            RoundOver("Right");
        }
        if(Input.GetKeyDown(KeyCode.E))
        {
            RoundOver("Left");
        }
    }

    public string ReadScore(out int leftScore, out int rightScore)
    {
        leftScore = playerRoundScore["Left"];
        rightScore = playerRoundScore["Right"];
        return currentWinner;
    }

    public int ReadRoundScore(out int rightScore)
    {
        rightScore = playerGameScore["Right"];
        return playerGameScore["Left"];
    }

    public void GameStart()
    {
        isRoundOver = false;
        isGameSetOver = false;
        isGameOver = false;
        playerRoundScore["Left"] = 0;
        playerRoundScore["Right"] = 0;
        playerGameScore["Left"] = 0;
        playerGameScore["Right"] = 0;
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

        if(playerRoundScore["Right"] >=2)
        {
            GameSetOver("Right");
        }
        else if (playerRoundScore["Left"] >= 2)
        {
            GameSetOver("Left");
        }
        Debug.Log(winner);
        OnRoundOver?.Invoke();
    }

    public void GameSetStart()
    {
        isGameSetOver = false;

        if (playerRoundScore["Right"] >= 2 || playerRoundScore["Left"] >= 2)
        {
            playerRoundScore["Left"] = 0;
            playerRoundScore["Right"] = 0;
        }
    }

    private void GameSetOver(string winner)
    {
        isGameSetOver = true;
        playerGameScore[winner] += 1;        
        currentWinner = winner;

        OnGameSetOver?.Invoke();
        if (playerGameScore["Left"] >= 3 || playerGameScore["Right"] >= 3)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        isGameOver = true;
    }
}