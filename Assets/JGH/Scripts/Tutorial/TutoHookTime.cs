using Photon.Pun;
using UnityEngine;
using TMPro;

public class TutoHookTIme : MonoBehaviourPunCallbacks
{
    public TMP_Text H;
    public TMP_Text M;
    public TMP_Text S;
    

    private bool isTimerRunning = false;
    private float elapsedTime = 0f;

    private int playerLayer;

    private void Start()
    {
        playerLayer = LayerMask.NameToLayer("Player");
    }

    private void Update()
    {
        if (!isTimerRunning) return;

        elapsedTime += Time.deltaTime;

        int hours = (int)(elapsedTime / 3600);
        int minutes = (int)(elapsedTime / 60) % 60;
        int seconds = (int)(elapsedTime % 60);

        H.text = hours.ToString("00");
        M.text = minutes.ToString("00");
        S.text = seconds.ToString("00");
    }

    // 2D 충돌 감지
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == playerLayer)
        {
            isTimerRunning = true;
            Debug.Log("타이머 시작됨");
        }
    }
}