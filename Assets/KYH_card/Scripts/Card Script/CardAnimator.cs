using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Sprite[] clips;               // 프레임 시퀀스
    public Image card;                   // 애니메이션 적용 대상 이미지
    public float wait = 0.1f;            // 프레임 간 시간

    public int numberOfClips;
    private int stage = 0;
    private float timer = 0f;

    private bool isHovered = false;
    private bool isPlaying = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        timer = 0f;

        // 호버 시에도 재생 시작
        isPlaying = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        stage = 0;
        timer = 0f;
        isPlaying = false;

        if (clips != null && clips.Length > 0)
            card.sprite = clips[0]; // 초기 프레임으로 복원
    }

    private void Start()
    {
        if (clips != null && clips.Length > 0)
        {
            numberOfClips = clips.Length;
            card.sprite = clips[0];
        }
    }

    private void Update()
    {
        if ((!isHovered && !isPlaying) || clips.Length == 0) return;

        timer += Time.deltaTime;

        if (timer >= wait)
        {
            timer = 0f;
            stage = (stage + 1) % numberOfClips;
            card.sprite = clips[stage];
        }
    }

    /// <summary>
    /// 외부에서 애니메이션 재시작 (처음부터)
    /// </summary>
    public void RestartAnimation()
    {
        stage = 0;
        timer = 0f;
        isPlaying = true;

        if (card != null && clips.Length > 0)
            card.sprite = clips[0];
    }

    /// <summary>
    /// 외부에서 애니메이션 강제 정지
    /// </summary>
    public void StopAnimation()
    {
        isPlaying = false;
        stage = 0;
        timer = 0f;

        if (card != null && clips.Length > 0)
            card.sprite = clips[0];
    }
}
