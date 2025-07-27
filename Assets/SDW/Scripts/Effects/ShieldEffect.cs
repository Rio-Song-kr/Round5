using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShieldEffect : MonoBehaviour
{
    [SerializeField] private Image _shieldImg;
    private float _shieldScaleMultiplier = 1.1f;
    private float _shieldScaleDuration = 0.2f;
    private Vector3 _originalScale;
    private Coroutine _coroutine;
    
    private void OnEnable()
    {
        _coroutine = StartCoroutine(RoundTripScaleOverTime(Vector3.one * _shieldScaleMultiplier, _shieldScaleDuration));
    }


    private void OnDisable()
    {
        _shieldImg.transform.localScale = _originalScale;
        StopCoroutine(_coroutine);
    }

    public void Init(float targetScale = 1.1f, float duration = 0.22f)
    {
        _shieldScaleMultiplier = targetScale;
        _shieldScaleDuration = duration;
        _originalScale = _shieldImg.transform.localScale;
    }

    private IEnumerator RoundTripScaleOverTime(Vector3 targetScale, float duration)
    {
        var originScale = _shieldImg.transform.localScale;
        var startScale = originScale;
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
                (startScale, targetScale) = (targetScale, startScale);
                timer = 0;
            }
            yield return null;
        }
    }
}
