using System.Collections;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// 원형 카운트다운 이펙트를 제어하는 컨트롤러
/// </summary>
public class AbyssalCountdownEffect : MonoBehaviourPun, IPunObservable
{
    [Header("Effects Prefabs")]
    //# 카운트다운 완료 시 활성화할 octagon 이펙트 오브젝트
    [SerializeField] private GameObject _octagonObject;
    [SerializeField] private CircleFillController _circleFill;
    [SerializeField] private CircleCollider2D _collider;

    //# Abyssal Countdown Skill Data
    public AbyssalSkillDataSO SkillData;
    //# 생성된 VFX 오브젝트의 파티클 시스템 컴포넌트
    private ParticleSystem _particle;
    //# 생성된 VFX 오브젝트의 참조
    private GameObject _vfxObject;

    [Header("Shield Effect")]
    //# 쉴드 효과 원형 이미지
    public GameObject ShieldObject;
    private float _shieldTimeCount;
    private float _moveAcceleratesInvincibilityLossRate = 0.2f;
    private float _shieldActiveTime = 2f;
    private bool _shieldEffectActivated;

    private float _pullRate = 0.2f;

    private Coroutine _scalingCoroutine;

    private Transform _playerTransform;
    private TestPlayerMove _myPlayer;
    private IStatusEffectable _status;

    private Vector3 _networkPosition;

    private bool _isStarted;
    private int _vfxViewId;

    private GameObject _targetPlayer;
    private int _targetViewId;

    private void Awake()
    {
        PhotonNetwork.SendRate = 40;
        PhotonNetwork.SerializationRate = 20;
    }

    private void OnDisable()
    {
        if (_vfxObject != null)
            PhotonNetwork.Destroy(_vfxObject);
    }

    /// <summary>
    /// 카운트다운 게이지를 업데이트하고 이펙트 상태를 관리
    /// </summary>
    private void Update()
    {
        if (!photonView.IsMine || !_isStarted) return;

        // 끌어당김 로직
        if (_targetPlayer != null)
        {
            if (Vector3.Distance(_playerTransform.position, _targetPlayer.transform.position) > 1.2f)
            {
                Vector2 direction = (_playerTransform.position - _targetPlayer.transform.position).normalized;
                var moveAmount = direction * _pullRate * Time.deltaTime;
                _targetPlayer.transform.Translate(moveAmount, Space.World);
                photonView.RPC(nameof(SyncTargetPosition), RpcTarget.Others, _targetViewId, _targetPlayer.transform.position);
            }
        }

        //# 증가 모드 -> 감소 모드로 변경되었을 때 처리
        if (_circleFill.StartEffect && !_circleFill.CanIncrese)
        {
            photonView.RPC(nameof(StartEffect), RpcTarget.All);
            _circleFill.StartEffect = false;
            _collider.enabled = true;
        }
        //# 감소 모드 -> 증가 모드로 변경되었을 때 처리
        else if (_circleFill.StartEffect && _circleFill.CanIncrese)
        {
            photonView.RPC(nameof(EndEffect), RpcTarget.All);
            _circleFill.StartEffect = false;
            _collider.enabled = false;
        }

        if (_shieldEffectActivated)
        {
            //# 플레이어가 움직일 경우, 사용 시간 감소
            if (_circleFill.PlayerMoved) _shieldTimeCount += Time.deltaTime * (1 + _moveAcceleratesInvincibilityLossRate);
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
        if (!_isStarted) return;

        if (photonView.IsMine)
        {
            transform.position = _playerTransform.position;
            _vfxObject.transform.position = _playerTransform.position;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, _networkPosition, Time.deltaTime * _myPlayer.MoveSpeed);
            _vfxObject.transform.position =
                Vector3.Lerp(_vfxObject.transform.position, _networkPosition, Time.deltaTime * _myPlayer.MoveSpeed);
        }
    }

    /// <summary>
    /// VFX 오브젝트를 생성하고 초기 상태를 설정
    /// </summary>
    public void Initialize(AbyssalSkillDataSO skillData, int playerViewId)
    {
        SkillData = skillData;

        if (!photonView.IsMine) return;

        //# 본인만 가지고 있음
        _collider = GetComponent<CircleCollider2D>();
        _collider.enabled = false;

        //# VFX 프리팹을 현재 위치에 생성하여 자식으로 설정
        _vfxObject = PhotonNetwork.Instantiate("VFX_CorePoolEffect", transform.position, transform.rotation);

        //# 생성된 VFX 오브젝트에서 파티클 시스템 컴포넌트를 가져옴
        _particle = _vfxObject.GetComponentInChildren<ParticleSystem>();

        var vfxView = _vfxObject.GetComponent<PhotonView>();
        _vfxViewId = vfxView.ViewID;

        //# 파티클을 중지하고 기존 파티클들을 제거
        photonView.RPC(nameof(InitializeComponent), RpcTarget.All, playerViewId, _vfxViewId);

        _octagonObject.SetActive(false);
    }

    [PunRPC]
    private void InitializeComponent(int playerViewId, int vfxViewId)
    {
        var playerTransform = PhotonView.Find(playerViewId).transform;
        _playerTransform = playerTransform;
        _myPlayer = _playerTransform.GetComponent<TestPlayerMove>();

        var effect = GetComponent<AbyssalCountdownEffect>();

        if (SkillData == null)
            SkillData = effect.SkillData;

        if (ShieldObject == null)
            ShieldObject = effect.ShieldObject;

        foreach (var effectStatus in SkillData.Status)
        {
            if (effectStatus.EffectType == StatusEffectType.PullOtherPlayer)
                _pullRate = effectStatus.EffectValue;
            else if (effectStatus.EffectType == StatusEffectType.MoveAcceleratesInvincibilityLoss)
                _moveAcceleratesInvincibilityLossRate = effectStatus.EffectValue;
        }

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
        _myPlayer.OnPlayerMoveStateChanged += _circleFill.OnPlayerMoveChanged;
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
        StopVfx();

        if (_scalingCoroutine != null) return;
        _scalingCoroutine = StartCoroutine(ScaleOverTime(Vector3.one, SkillData.ScalingDuration));

        _octagonObject.SetActive(false);
    }

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
            stream.SendNext(transform.position);
        else
            _networkPosition = (Vector3)stream.ReceiveNext();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!photonView.IsMine || !other.gameObject.CompareTag("Player")) return;

        var targetView = other.GetComponent<PhotonView>();

        if (targetView.IsMine) return;

        _targetPlayer = other.gameObject;

        //# 네트워크 지연 문제로, Player가 충돌이 감지됨에도 이동하는 문제가 있음
        //# 충돌된 플레이어가 AbyssalCountdown을 가지고 있다면 로컬에서도 바로 멈추기 위해 작성
        var targetDefenceSkill = _targetPlayer.GetComponent<DefenceSkillManager>();

        if (targetDefenceSkill.SkillNames.Contains(DefenceSkills.AbyssalCountdown))
        {
            if (!_status.HasStatusEffect(StatusEffectType.ReduceSpeed))
                photonView.RPC(nameof(SyncTargetPosition), RpcTarget.Others, photonView.ViewID, _playerTransform.position);
        }

        _targetViewId = other.GetComponent<PhotonView>().ViewID;

        float duration = SkillData.ActivateTime * _circleFill.CurrentFillAmount;

        //# 다른 클라이언트에 동기화
        photonView.RPC(nameof(ApplyReduceSpeed), RpcTarget.Others, _targetViewId, duration);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (photonView.IsMine && other.CompareTag("Player"))
        {
            _targetViewId = other.GetComponent<PhotonView>().ViewID;
            photonView.RPC(nameof(RemoveReduceSpeed), RpcTarget.Others, _targetViewId);

            _targetPlayer = null;
            _targetViewId = 0;

            photonView.RPC(nameof(SyncTargetPosition), RpcTarget.Others, photonView.ViewID, _playerTransform.position);
        }
    }

    [PunRPC]
    private void ApplyReduceSpeed(int viewId, float duration)
    {
        var targetView = PhotonView.Find(viewId);
        var otherStatus = targetView.GetComponent<IStatusEffectable>();

        //# Abyssal Countdown의 끌려가는 효과에 의해 이속이 2배가 되어야 이동이 가능해짐
        otherStatus.ApplyStatusEffect(
            StatusEffectType.ReduceSpeed,
            1f,
            duration
        );
    }

    [PunRPC]
    private void RemoveReduceSpeed(int viewId)
    {
        var targetView = PhotonView.Find(viewId);
        var otherStatus = targetView.gameObject.GetComponent<IStatusEffectable>();

        if (otherStatus != null)
            otherStatus.RemoveStatusEffect(StatusEffectType.ReduceSpeed);
    }

    [PunRPC]
    private void SyncTargetPosition(int viewId, Vector3 position)
    {
        var targetView = PhotonView.Find(viewId);

        if (targetView != null) targetView.transform.position = position;
    }
}