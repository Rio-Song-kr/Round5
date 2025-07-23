using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
public class FlipCard : MonoBehaviour
{
    private bool isFlipped = false;      // 뒷면 → 앞면으로 뒤집혔는가?
    private bool isSelected = false;     // 선택되었는가?

    [Header("앞/뒷면 루트 오브젝트")]
    public GameObject frontRoot; // frontImage
    public GameObject backRoot;  // BackImage

    [Header("설정")]
    public float flipDuration = 0.25f;

    private CardSelectManager manager;

    public void SetManager(CardSelectManager mgr)
    {
        manager = mgr;
    }

    private void Start()
    {
        isFlipped = false;
        isSelected = false;

        transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

        if (frontRoot != null) frontRoot.SetActive(false);
        if (backRoot != null) backRoot.SetActive(true);
    }

    public void OnClickCard()
    {
        // 아직 뒤집지 않은 상태면 → 회전해서 앞면으로
        if (!isFlipped)
        {
            isFlipped = true;

            transform.DORotate(new Vector3(0, 0, 0), flipDuration)
                .SetEase(Ease.InOutSine)
                .OnUpdate(() =>
                {
                    float yRot = transform.localEulerAngles.y;
                    if (yRot > 180f) yRot -= 360f;
                    bool showFront = Mathf.Abs(yRot) <= 90f;

                    frontRoot.SetActive(showFront);
                    backRoot.SetActive(!showFront);
                })
                .OnComplete(() =>
                {
                    frontRoot.SetActive(true);
                    backRoot.SetActive(false);
                });
        }
        // 이미 앞면이고 아직 선택되지 않았다면 → 선택 처리
        else if (!isSelected)
        {
            isSelected = true;
            manager?.OnCardSelected(gameObject);
        }
    }
}