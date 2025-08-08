using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class TutoHookGoalController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject goalMsgUI;      // Goal_msg UI
    [SerializeField] private Button OKButton;

    [Header("씬 오브젝트")]
    [SerializeField] private GameObject environment;
    [SerializeField] private GameObject timer;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject checkpoint1;
    [SerializeField] private GameObject checkpoint2;
    [SerializeField] private GameObject goalArea;
    
    public TMP_Text H;
    public TMP_Text M;
    public TMP_Text S;
    public TMP_Text GoalUiTimeText;
    

    private int playerLayer;

    private void Start()
    {
        playerLayer = LayerMask.NameToLayer("Player");

        OKButton.onClick.AddListener(OnClickOK);

        // 시작 시 UI 숨김 (필요 시 활성)
        // goalMsgUI.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == playerLayer)
        {
            ShowGoalMessage();
        }
    }

    public void ShowGoalMessage()
    {
        if (goalMsgUI != null) goalMsgUI.SetActive(true);
        if (environment != null) environment.SetActive(false);
        if (timer != null) timer.SetActive(false);
        if (player != null) player.SetActive(false);
        if (checkpoint1 != null) checkpoint1.SetActive(false);
        if (checkpoint2 != null) checkpoint2.SetActive(false);
        if (goalArea != null) goalArea.SetActive(false);

        // 게임 정지
        Time.timeScale = 0f;

        GoalUiTimeText.text = $"{H.text} : {M.text} : {S.text}";
    }

    private void OnClickOK()
    {
        // 게임 다시 재개
        Time.timeScale = 1f;

        SceneManager.LoadScene("LobbyScene");
    }
}