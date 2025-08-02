using System.Collections;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// 원형 카운트다운 이펙트를 제어하는 컨트롤러
/// </summary>
public class AbyssalCountdownEffect : MonoBehaviourPun, IPunObservable
// public class AbyssalCountdownEffect : MonoBehaviourPun
{
    [Header("Effects Prefabs")]
    //# 카운트다운 완료 시 활성화할 octagon 이펙트 오브젝트
    [SerializeField] private GameObject _octagonObject;
    [SerializeField] private CircleFillController _circleFill;

    //# Abyssal Countdown Skill Data
    public AbyssalSkillDataSO SkillData;
    //# 생성된 VFX 오브젝트의 파티클 시스템 컴포넌트
    private ParticleSystem _particle;
    //# 생성된 VFX 오브젝트의 참조
    private GameObject _vfxObject;
    //# 현재 게이지가 증가 모드인지 여부 (true: 증가, false: 감소)
    // private bool _canIncrease = true;

    [Header("Shield Effect")]
    //# 쉴드 효과 원형 이미지
    public GameObject ShieldObject;
    [SerializeField] private bool _playerMoved;
    private float _shieldTimeCount;
    private float _moveAcceleratesInvincibilityLossRate = 0.2f;
    private float _shieldActiveTime = 2f;
    private bool _shieldEffectActivated;

    private Coroutine _scalingCoroutine;
    private Transform _playerTransform;
    private IStatusEffectable _status;

    private Vector3 _networkPosition;
    private float _networkFillAmount;

    private bool _isStarted;
    private int _vfxViewId;

    private void OnDisable()
    {
        if (_vfxObject != null)
            PhotonNetwork.Destroy(_vfxObject);
    }

    /// <summary>
    /// 카운트다운 이펙트 초기화 메서드
    /// </summary>
    private void Start()
    {
        if (_particle == null) return;
        //# 파티클을 중지하고 기존 파티클들을 제거
        _particle.Stop();
        _particle.Clear();
    }

    /// <summary>
    /// 카운트다운 게이지를 업데이트하고 이펙트 상태를 관리
    /// </summary>
    private void Update()
    {
        if (!photonView.IsMine || !_isStarted) return;

        //# 증가 모드 -> 감소 모드로 변경되었을 때 처리
        if (_circleFill.StartEffect && !_circleFill.CanIncrese)
        {
            photonView.RPC(nameof(StartEffect), RpcTarget.All);
            _circleFill.StartEffect = false;
        }

        //# 감소 모드 -> 증가 모드로 변경되었을 때 처리
        else if (_circleFill.StartEffect && _circleFill.CanIncrese)
        {
            photonView.RPC(nameof(EndEffect), RpcTarget.All);
            _circleFill.StartEffect = false;
        }

        if (_shieldEffectActivated)
        {
            //# 플레이어가 움직일 경우, 사용 시간 감소
            if (_playerMoved) _shieldTimeCount += Time.deltaTime * (1 + _moveAcceleratesInvincibilityLossRate);
            else _shieldTimeCount += Time.deltaTime;

            if (_shieldTimeCount >= _shieldActiveTime)
                photonView.RPC(nameof(DisableShieldEffect), RpcTarget.All);
        }
    }

    /// <summary>
    /// effect의 위치 동기화
    /// </summary>
    private void LateUpdate()
    {
        if (photonView.IsMine)
        {
            transform.position = _playerTransform.position;
            _vfxObject.transform.position = _playerTransform.position;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, _networkPosition, Time.deltaTime * 5f);
            _vfxObject.transform.position = Vector3.Lerp(_vfxObject.transform.position, _networkPosition, Time.deltaTime * 5f);
        }
        // NetworkSync();
    }

    /// <summary>
    /// VFX 오브젝트를 생성하고 초기 상태를 설정
    /// </summary>
    public void Initialize(AbyssalSkillDataSO skillData, int playerViewId)
    {
        SkillData = skillData;

        if (!photonView.IsMine) return;

        //# VFX 프리팹을 현재 위치에 생성하여 자식으로 설정
        _vfxObject = PhotonNetwork.Instantiate("VFX_CorePoolEffect", transform.position, transform.rotation);

        //# 생성된 VFX 오브젝트에서 파티클 시스템 컴포넌트를 가져옴
        _particle = _vfxObject.GetComponentInChildren<ParticleSystem>();

        var vfxView = _vfxObject.GetComponent<PhotonView>();
        _vfxViewId = vfxView.ViewID;

        //# 파티클을 중지하고 기존 파티클들을 제거
        photonView.RPC(nameof(InitializeComponent), RpcTarget.All, playerViewId, _vfxViewId);

        _octagonObject.SetActive(true);
    }

    [PunRPC]
    private void InitializeComponent(int playerViewId, int vfxViewId)
    {
        var playerTransform = PhotonView.Find(playerViewId).transform;
        _playerTransform = playerTransform;

        var effect = playerTransform.GetComponent<AbyssalCountdownEffect>();

        if (SkillData == null)
            SkillData = effect.SkillData;

        if (ShieldObject == null)
            ShieldObject = effect.ShieldObject;

        ShieldObject.GetComponent<ShieldEffect>().Init(SkillData.ShieldScaleMultiplier, SkillData.ShieldScaleDuration);
        ShieldObject.SetActive(false);
        _shieldTimeCount = 0f;

        if (_status == null)
            _status = playerTransform.GetComponent<PlayerStatus>();

        _vfxViewId = vfxViewId;
        var vfxView = PhotonView.Find(vfxViewId);

        _vfxObject = vfxView.gameObject;
        _particle = vfxView.GetComponentInChildren<ParticleSystem>();

        photonView.RPC(nameof(EndEffect), RpcTarget.All);

        _isStarted = true;

        //# 제대로 초기화가 안되는 문제가 있어서 RPC에서 다시 초기화
        _circleFill.Initialize(SkillData.ActivateTime, SkillData.ChargeTime);
        _circleFill.gameObject.SetActive(true);
    }

    private void StartVfx()
    {
        _particle.Play();
    }

    private void StopVfx()
    {
        _particle.Stop();
        _particle.Clear();
    }

    [PunRPC]
    private void DisableShieldEffect()
    {
        _status.RemoveStatusEffect(StatusEffectType.Invincibility);
        ShieldObject.SetActive(false);
        _shieldTimeCount = 0f;
        _shieldEffectActivated = false;
    }

    /// <summary>
    /// 카운트다운 효과의 시작 시 호출되는 메서드
    /// 파티클 효과를 시작하고 목표 스케일로 확대하며, 특정 이펙트 객체를 활성화
    /// </summary>
    [PunRPC]
    public void StartEffect()
    {
        //# 파티클 효과 시작 및 octagon 이펙트 활성화
        StartVfx();

        if (_scalingCoroutine != null) StopCoroutine(_scalingCoroutine);
        _scalingCoroutine =
            StartCoroutine(ScaleOverTime(Vector3.one * SkillData.TargetScaleMultiplier, SkillData.ScalingDuration));

        UseShieldEffect();
        _octagonObject.SetActive(true);
        _shieldTimeCount = 0f;
    }

    /// <summary>
    /// 카운트다운 효과의 끝에 호출되는 메서드
    /// </summary>
    [PunRPC]
    public void EndEffect()
    {
        //# 파티클 효과 중지 및 octagon 이펙트 비활성화
        // photonView.RPC(nameof(StopVfx), RpcTarget.All);
        StopVfx();

        if (_scalingCoroutine != null) StopCoroutine(_scalingCoroutine);
        _scalingCoroutine = StartCoroutine(ScaleOverTime(Vector3.one, SkillData.ScalingDuration));

        _octagonObject.SetActive(false);
    }

    //todo Coroutine 대신에 TimeDeltaTime으로 처리해야할 듯
    //# 플레이어의 이동이 있을 때는 사용 시간이 깍히는 시간이 20% 증가해야 함
    //# 이동 시 -> Time.deltaTime * 1.2, 정지 시 -> Time.deltaTime
    private void UseShieldEffect()
    {
        ShieldObject.SetActive(true);

        foreach (var status in SkillData.Status)
        {
            if (status.EffectType == StatusEffectType.MoveAcceleratesInvincibilityLoss)
            {
                _moveAcceleratesInvincibilityLossRate = status.EffectValue;
                continue;
            }

            if (status.EffectType != StatusEffectType.Invincibility) continue;

            _status.ApplyStatusEffect(status.EffectType, status.EffectValue, status.Duration);
            _shieldActiveTime = status.Duration;

            _shieldEffectActivated = true;
        }
    }
    /// <summary>
    /// 카운트 다운 감소 시 확장/카운트 다운의 끝에 다시 축소를 제어하는 Coroutine
    /// </summary>
    /// <param name="targetScale">목표 Scale</param>
    /// <param name="duration">목표 Scale 도달까지의 시간</param>
    private IEnumerator ScaleOverTime(Vector3 targetScale, float duration)
    {
        var startScale = transform.localScale;
        float timer = 0f;

        while (timer < duration)
        {
            transform.localScale = Vector3.Lerp(startScale, targetScale, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
        _scalingCoroutine = null;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(_playerMoved);
        }
        else
        {
            _networkPosition = (Vector3)stream.ReceiveNext();
            _playerMoved = (bool)stream.ReceiveNext();
        }
    }
}