using System.Collections;
using Photon.Pun;
using UnityEngine;

public class VfxSmokeEffect : MonoBehaviourPun
{
    private Coroutine _coroutine;
    private ParticleSystem[] _frostSlamParticles;
    private PoolManager _pools;
    private WaitForSeconds _wait = new WaitForSeconds(1f);

    /// <summary>
    /// 컴포넌트 연결
    /// </summary>
    private void OnEnable() => _frostSlamParticles = gameObject.GetComponentsInChildren<ParticleSystem>();

    /// <summary>
    /// Disable 시 Coroutine 종료
    /// </summary>
    private void OnDisable()
    {
        if (_coroutine != null)
            StopCoroutine(_coroutine);
    }

    /// <summary>
    /// 컴포넌트 초기화
    /// </summary>
    /// <param name="pools">효과 재사용을 위한 풀 매니저 인스턴스</param>
    public void Initialize(PoolManager pools) => _pools = pools;

    /// <summary>
    /// 효과 재생 시작
    /// </summary>
    public void Play()
    {
        Stop();

        foreach (var particle in _frostSlamParticles)
        {
            particle.Play();
        }

        _coroutine = StartCoroutine(ReturnPool());
    }

    /// <summary>
    /// 현재 효과 재생 중지
    /// </summary>
    public void Stop()
    {
        foreach (var particle in _frostSlamParticles)
        {
            particle.Stop();
            particle.Clear();
        }
    }

    /// <summary>
    /// 효과 풀 반환 루틴 실행
    /// </summary>
    private IEnumerator ReturnPool()
    {
        yield return _wait;
        _coroutine = null;

        if (photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }
}