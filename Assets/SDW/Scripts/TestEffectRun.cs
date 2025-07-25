using System.Collections;
using UnityEngine;

public class TestEffectRun : MonoBehaviour
{
    [SerializeField] private GameObject _frostSlamEffectObject;
    [SerializeField] private GameObject _empEffectObject;
    [SerializeField] private GameObject _abyssalCountdownObject;
    [SerializeField] private float _delay = 3f;

    private void Start() => StartCoroutine(RunEffect());

    private IEnumerator RunEffect()
    {
        yield return new WaitForSeconds(_delay);
        _frostSlamEffectObject.SetActive(true);
        _empEffectObject.SetActive(true);
        _abyssalCountdownObject.SetActive(true);
    }
}