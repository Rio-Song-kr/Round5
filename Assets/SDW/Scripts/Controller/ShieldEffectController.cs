using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class ShieldEffectController : MonoBehaviourPun
{
    [SerializeField] private Image _shieldImg;
    private float _shieldScaleMultiplier = 1.1f;
    private float _shieldScaleDuration = 0.2f;
    private Vector3 _originalScale;
    private Coroutine _coroutine;
    private bool _isInitialized;

    /// <summary>
    /// Enable 시 Shield 크기 조절(반복) Coroutine 시작
    /// </summary>
    private void OnEnable()
    {
        if (!_isInitialized) return;

        _coroutine = StartCoroutine(RoundTripScaleOverTime(Vector3.one * _shieldScaleMultiplier, _shieldScaleDuration));
    }

    /// <summary>
    /// Disable 시 Shield 효과 초기화 및 코루틴 중지
    /// </summary>
    private void OnDisable()
    {
        _shieldImg.transform.localScale = _originalScale;

        if (_coroutine != null)
            StopCoroutine(_coroutine);
    }

    /// <summary>
    /// 초기화 메서드로, 쉴드 효과의 초기 설정을 수행
    /// </summary>
    /// <param name="targetScale">목표 스케일 값(기본값: 1.1f)</param>
    /// <param name="duration">스케일 변경 지속 시간(기본값: 0.22f)</param>
    public void Init(float targetScale = 1.1f, float duration = 0.22f)
    {
        _shieldScaleMultiplier = targetScale;
        _shieldScaleDuration = duration;
        _originalScale = _shieldImg.transform.localScale;
        _isInitialized = true;
    }

    /// <summary>
    /// 반복적으로 쉴드의 크기를 목표 스케일 사이에서 왔다 갔다 조절하는 코루틴 실행 메서드
    /// </summary>
    /// <param name="targetScale">목표 스케일 값</param>
    /// <param name="duration">스케일 변화 지속 시간</param>
    private IEnumerator RoundTripScaleOverTime(Vector3 targetScale, float duration)
    {
        var startScale = _originalScale;
        float timer = 0f;
        bool isReversed = false;

        while (true)
        {
            if (!isReversed)
                _shieldImg.transform.localScale = Vector3.Lerp(startScale, targetScale, timer / duration);
            else
                _shieldImg.transform.localScale = Vector3.Lerp(startScale, targetScale, timer / duration);

            timer += Time.deltaTime;

            if (Mathf.Abs((targetScale - _shieldImg.transform.localScale).magnitude) < 0.01f)
            {
                isReversed = !isReversed;

                //# 튜플을 이용한 Swap
                (startScale, targetScale) = (targetScale, startScale);
                timer = 0;
            }

            yield return null;
        }
    }
}