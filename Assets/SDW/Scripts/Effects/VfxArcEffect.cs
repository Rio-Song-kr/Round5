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

        Stop();
    }

    /// <summary>
    /// 초기화 프로세싱을 수행
    /// </summary>
    public void Initialize()
    {
        Stop();
        if (!photonView.IsMine) return;
        photonView.RPC(nameof(InitializeArcEffect), RpcTarget.All);
    }

    /// <summary>
    /// 클라이언트 측에서 아크 효과 초기화 로직을 원격 호출하여 실행
    /// </summary>
    [PunRPC]
    private void InitializeArcEffect()
    {
        _pools = FindFirstObjectByType<PoolManager>();
    }

    /// <summary>
    /// 파티클 재생
    /// </summary>
    public void Play()
    {
        if (!photonView.IsMine) return;

        photonView.RPC(nameof(PlayVfxArc), RpcTarget.All);
    }

    /// <summary>
    /// 클라이언트 측에서 시각 효과 아크를 재생하는 함수를 원격 호출
    /// 이 메서드는 Photon 네트워크를 통해 모든 클라이언트에게 신호를 보내 충돌 효과를 재생
    /// </summary>
    [PunRPC]
    private void PlayVfxArc()
    {
        Stop();
        _hitParticle.Play();
        _coroutine = StartCoroutine(ReturnPool());
    }

    /// <summary>
    /// 중지 작업 수행
    /// </summary>
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

        if (photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }
}