using System.Collections;
using Photon.Pun;
using UnityEngine;

public class VfxExplosiveEffect : MonoBehaviourPun
{
    private ParticleSystem _particle;
    private Coroutine _coroutine;
    private bool _isReleased;

    private void Awake() => _particle = GetComponent<ParticleSystem>();

    private void OnDisable()
    {
        if (_coroutine != null && !_isReleased)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
            PhotonNetwork.Destroy(gameObject);
            _isReleased = true;
        }
    }

    public void Play()
    {
        _isReleased = false;
        _particle.Clear();
        _particle.Play();

        _coroutine = StartCoroutine(ReturnToPool());
    }

    private IEnumerator ReturnToPool()
    {
        yield return new WaitForSeconds(_particle.main.duration);
        _particle.Clear();
        _particle.Stop();

        if (!_isReleased)
        {
            _isReleased = true;
            PhotonNetwork.Destroy(gameObject);
            _coroutine = null;
        }
    }
}