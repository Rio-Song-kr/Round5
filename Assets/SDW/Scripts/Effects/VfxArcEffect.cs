using System.Collections;
using Photon.Pun;
using UnityEngine;

public class VfxArcEffect : MonoBehaviourPun
{
    private Coroutine _coroutine;
    private PoolManager _pools;
    private ParticleSystem _hitParticle;

    /// <summary>
    /// 컴포넌트 연결
    /// </summary>
    private void OnEnable() => _hitParticle = gameObject.GetComponentInChildren<ParticleSystem>();

    /// <summary>
    /// 컴포넌트 비활성화 처리
    /// </summary>
    private void OnDisable()
    {
        if (_coroutine != null)
            StopCoroutine(_coroutine);
    }

    /// <summary>
    /// 초기화 메서드를 통해 필요한 리소스와 설정값을 할당
    /// </summary>
    /// <param name="pools">파티클 효과 풀 관리 객체</param>
    public void Initialize(PoolManager pools) => _pools = pools;

    /// <summary>
    /// 파티클 재생
    /// </summary>
    public void Play()
    {
        Stop();
        _hitParticle.Play();
        _coroutine = StartCoroutine(ReturnPool());
    }

    public void Stop()
    {
        _hitParticle.Stop();
        _hitParticle.Clear();
    }

    /// <summary>
    /// 풀로 객체 반환 및 해제 로직을 실행하는 비동기 메서드
    /// </summary>
    private IEnumerator ReturnPool()
    {
        yield return new WaitForSeconds(_hitParticle.main.duration);
        _coroutine = null;
        _pools.Destroy(gameObject);
    }
}