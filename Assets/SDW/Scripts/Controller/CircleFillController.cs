using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircleFillController : MonoBehaviour
{
    [Header("Circle UI")]
    //# 카운트다운 게이지의 채워지는 원형 이미지
    [SerializeField] private Image _filledCircleImg;
    //# 카운트다운 게이지의 움직이는 원형 이미지
    [SerializeField] private Image _movingCircleImg;
    //# 카운트다운 게이지의 움직이는 라인 이미지
    [SerializeField] private Image _movingLineImg;

    //# 현재 게이지의 채움 정도 (0~1)
    public float CurrentFillAmount = 0f;
    //# 목표로 하는 게이지의 채움 정도 (0~1)
    private float _targetFillAmount = 1f;

    //# 현재 게이지가 증가 모드인지 여부 (true: 증가, false: 감소)
    public bool CanIncrese = true;
    public bool StartEffect = false;

    private float _activateTime;
    private float _chargeTime;

    public void Initialize(float activateTime, float chargeTime)
    {
        _activateTime = activateTime;
        _chargeTime = chargeTime;
    }

    private void Update()
    {
        //# 현재 채움량을 목표값으로 부드럽게 이동
        if (CurrentFillAmount > _targetFillAmount) CurrentFillAmount -= Time.deltaTime / _activateTime;
        else if (CurrentFillAmount < _targetFillAmount) CurrentFillAmount += Time.deltaTime / _chargeTime;

        //# 증가 모드에서 게이지가 거의 완전히 채워졌을 때의 처리
        if (CanIncrese && Mathf.Abs(CurrentFillAmount - 1f) < 0.01f)
        {
            //# 게이지를 완전히 채우고 감소 모드로 전환
            CurrentFillAmount = 1f;
            _targetFillAmount = 0f;
            CanIncrese = false;
            StartEffect = true;
        }
        //# 감소 모드에서 게이지가 거의 완전히 비워졌을 때의 처리
        else if (!CanIncrese && Mathf.Abs(CurrentFillAmount) < 0.01f)
        {
            //# 게이지를 완전히 비우고 증가 모드로 전환
            CurrentFillAmount = 0f;
            _targetFillAmount = 1f;
            CanIncrese = true;
            StartEffect = true;
        }

        //# 계산된 채움량을 UI 요소들에 적용
        SetFillAmount(CurrentFillAmount);
    }

    /// <summary>
    /// 계산된 채움량을 각 UI 이미지 요소에 적용하여 카운트다운 효과를 시각화
    /// </summary>
    /// <param name="fillAmount">적용할 채움량 (0~1 범위)</param>
    public void SetFillAmount(float fillAmount)
    {
        //# 채워지는 원형 이미지와 움직이는 원형 이미지의 fillAmount 설정
        _filledCircleImg.fillAmount = fillAmount;
        _movingCircleImg.fillAmount = fillAmount;

        //# 움직이는 라인 이미지를 채움량에 따라 회전 (360도 기준)
        _movingLineImg.transform.rotation = Quaternion.Euler(0f, 0f, -fillAmount * 360f);
    }
}