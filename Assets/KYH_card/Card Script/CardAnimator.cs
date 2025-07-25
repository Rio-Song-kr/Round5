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

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        timer = 0f; // 타이머도 초기화하면 즉시 첫 프레임 재생 가능
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        stage = 0;                         // 애니메이션 첫 프레임으로 리셋
        timer = 0f;
        card.sprite = clips[0];           // 이미지도 초기화
    }

    private void Start()
    {
        if (clips != null && clips.Length > 0)
        {
            numberOfClips = clips.Length;
            card.sprite = clips[0];       // 처음 프레임으로 시작
        }
    }

    private void Update()
    {
        if (!isHovered || clips.Length == 0) return;

        timer += Time.deltaTime;

        if (timer >= wait)
        {
            timer = 0f;
            stage = (stage + 1) % numberOfClips;
            card.sprite = clips[stage];
        }
    }
}
