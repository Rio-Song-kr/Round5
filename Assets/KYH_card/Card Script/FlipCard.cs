using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class FlipCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private bool isFlipped = false;
    private bool isSelected = false;
    private bool isHovered = false;

    [Header("앞/뒷면 루트 오브젝트")]
    public GameObject frontRoot;
    public GameObject backRoot;

    [Header("설정")]
    public float flipDuration = 0.25f;
    public float hoverScale = 1.1f;

    private Vector3 originalScale;
    private CardSelectManager manager;

    public void SetManager(CardSelectManager mgr)
    {
        manager = mgr;
    }

    private void Start()
    {
        isFlipped = false;
        isSelected = false;
        isHovered = false;

        originalScale = transform.localScale;

        // 현재 회전을 유지하고 Y만 180도로 변경
        Vector3 rot = transform.localEulerAngles;
        rot.y = 180f;
        transform.localRotation = Quaternion.Euler(rot);

        if (frontRoot != null) frontRoot.SetActive(false);
        if (backRoot != null) backRoot.SetActive(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        transform.DOScale(originalScale * hoverScale, 0.4f).SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        transform.DOScale(originalScale, 0.4f).SetEase(Ease.InBack);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isHovered) return; // 마우스가 올라가 있지 않으면 무시

        OnClickCard(); // 기존 로직 그대로 호출
    }

    public void OnClickCard()
    {
        if (!isFlipped)
        {
            isFlipped = true;

            // 현재 회전값 가져오기
            Vector3 startEuler = transform.localEulerAngles;

            // Z축 보정 (360도 이상 값이면 -360 보정 후 부호 반전)
            float z = startEuler.z;
            if (z > 180f) z -= 360f;
            float flippedZ = -z;

            // Y = 0 으로 회전하면서 Z는 반전해서 부채꼴 각도 유지
            Vector3 targetEuler = new Vector3(0f, 0f, flippedZ);

            transform.DORotate(targetEuler, flipDuration)
                .SetEase(Ease.InOutSine)
                .OnUpdate(() =>
                {
                    float yRot = transform.localEulerAngles.y;
                    if (yRot > 180f) yRot -= 360f;
                    bool showFront = Mathf.Abs(yRot) <= 90f;

                    if (frontRoot != null) frontRoot.SetActive(showFront);
                    if (backRoot != null) backRoot.SetActive(!showFront);
                })
                .OnComplete(() =>
                {
                    if (frontRoot != null) frontRoot.SetActive(true);
                    if (backRoot != null) backRoot.SetActive(false);
                });
        }
        else if (!isSelected)
        {
            isSelected = true;
            manager?.OnCardSelected(gameObject);
        }
    }

}