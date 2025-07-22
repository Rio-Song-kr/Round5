using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
public class FlipCard : MonoBehaviour
{
    private bool CardFlip = false; // false = 뒷면, true = 앞면

    [Header("Canvas to control")]
    public GameObject frontCanvas; // 앞면 Canvas
    public GameObject backCanvas;  // 뒷면 Canvas

    [Header("설정")]
    public float flipDuration = 0.25f;

    private void Start()
    {
        // 카드 뒷면 상태로 시작
        transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        CardFlip = false;

        if (frontCanvas != null) frontCanvas.SetActive(false);
        if (backCanvas != null) backCanvas.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Flip();
        }
    }

    public void Flip()
    {
        CardFlip = !CardFlip;
        float targetY = CardFlip ? 0f : 180f;

        // 미리 앞면 켜고 뒷면 끔 (회전 중 상태에 따라 다시 바뀔 수 있음)
        if (frontCanvas != null) frontCanvas.SetActive(true);
        if (backCanvas != null) backCanvas.SetActive(true);

        transform.DORotate(new Vector3(0, targetY, 0), flipDuration)
            .SetEase(Ease.InOutSine)
            .OnUpdate(() =>
            {
                float yRot = transform.localEulerAngles.y;
                if (yRot > 180f) yRot -= 360f;

                // 앞면 보이는 각도: -90 ~ 90
                bool showFront = Mathf.Abs(yRot) <= 90f;

                if (frontCanvas != null) frontCanvas.SetActive(showFront);
                if (backCanvas != null) backCanvas.SetActive(!showFront);
            })
            .OnComplete(() =>
            {
                // 최종 상태 정리
                if (frontCanvas != null) frontCanvas.SetActive(CardFlip);
                if (backCanvas != null) backCanvas.SetActive(!CardFlip);
            });
    }
}