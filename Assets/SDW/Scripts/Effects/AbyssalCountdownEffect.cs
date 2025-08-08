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
    private PlayerController _myPlayer;
    private IStatusEffectable _status;

    private Vector3 _networkPosition;

    private bool _isStarted;
    private int _vfxViewId;

    private GameObject _targetPlayer;
    private int _targetViewId;

    /// <summary>
    /// 스크립트 시작 시 초기 설정 및 Photon 네트워크 설정 조정을 수행
    /// </summary>
    private void Awake()
    {
        PhotonNetwork.SendRate = 40;
        PhotonNetwork.SerializationRate = 20;
    }

    /// <summary>
    /// 게임 오브젝트 비활성화 시 진행 중인 이펙트 오브젝트 해제 및 리소스 해제 작업 수행
    /// </summary>
    private void OnDisable()
    {
        if (!photonView.IsMine) return;
        if (_vfxObject != null && _vfxObject.activeSelf)
            PhotonNetwork.Destroy(_vfxObject);
    }

    /// <summary>
    /// 카운트다운 게이지를 업데이트하고 이펙트 상태를 관리
    /// </summary>
    private void Update()
    {
        if (InGameManager.Instance.IsGameOver) return;

        if (!photonView.IsMine || !_isStarted) return;

        // 끌어당김 로직
        if (_targetPlayer != null)
        {
            if (Vector3.Distance(_playerTransform.position, _targetPlayer.transform.position) > 1.2f)
            {
                Vector2 direction = (_playerTransform.position - _targetPlayer.transform.position).normalized;
                var moveAmount = _pullRate * Time.deltaTime * direction;
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
                photonView.RPC(nameof(DisableShieldObject), RpcTarget.All);
        }
    }

    /// <summary>
    /// effect의 위치 동기화
    /// </summary>
    private void LateUpdate()
    {
        if (!_isStarted) return;

        transform.position = _playerTransform.position;
        _vfxObject.transform.position = _playerTransform.position;
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

    /// <summary>
    /// 초기 설정 및 구성 요소 활성화를 위한 콜백 메서드 호출
    /// </summary>
    /// <param name="playerViewId">플레이어의 Photon 뷰 ID</param>
    /// <param name="vfxViewId">VFX 효과의 Photon 뷰 ID</param>
    [PunRPC]
    private void InitializeComponent(int playerViewId, int vfxViewId)
    {
        var playerTransform = PhotonView.Find(playerViewId).transform;
        _playerTransform = playerTransform;
        _myPlayer = _playerTransform.GetComponent<PlayerController>();

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

        ShieldObject.GetComponent<ShieldEffectController>().Init(SkillData.ShieldScaleMultiplier, SkillData.ShieldScaleDuration);
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

    /// <summary>
    /// 카운트다운 효과의 시작 시 파티클 효과를 활성화하고 스케일 조정을 시작하며 쉴드 이펙트를 활성화하는 메서드
    /// </summary>
    private void StartVfx() => _particle.Play();

    /// <summary>
    /// 카운트다운 효과의 비활성화를 위한 메서드
    /// 생성된 시각 효과와 파티클 시스템을 중지시키고 리소스를 정리한다
    /// </summary>
    private void StopVfx()
    {
        _particle.Stop();
        _particle.Clear();
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

        UseShieldObject();

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

    /// <summary>
    /// 쉴드 오브젝트를 활성화하고 해당 스킬 데이터에 정의된 상태 효과를 적용
    /// </summary>
    private void UseShieldObject()
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
    /// 플레이어의 방패 효과를 비활성화하는 원격 프로시저 콜 함수
    /// </summary>
    [PunRPC]
    private void DisableShieldObject()
    {
        _status.RemoveStatusEffect(StatusEffectType.Invincibility);
        ShieldObject.SetActive(false);
        _shieldTimeCount = 0f;
        _shieldEffectActivated = false;
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

    /// <summary>
    /// 2D 충돌 감지 시 특정 조건 하의 플레이어에 대한 효과 처리를 수행
    /// </summary>
    /// <param name="other">충돌한 Collider2D 컴포넌트</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!photonView.IsMine || !other.gameObject.CompareTag("Player")) return;

        var targetView = other.GetComponent<PhotonView>();

        if (targetView.IsMine) return;

        _targetPlayer = other.gameObject;

        //# 네트워크 지연 문제로, Player가 충돌이 감지됨에도 이동하는 문제가 있음
        //# 충돌된 플레이어가 AbyssalCountdown을 가지고 있다면 로컬에서도 바로 멈추기 위해 작성
        // var targetDefenceSkill = _targetPlayer.GetComponent<DefenceSkillManager>();
        //
        // if (targetDefenceSkill.SkillNames.Contains(DefenceSkills.AbyssalCountdown))
        // {
        //     if (!_status.HasStatusEffect(StatusEffectType.ReduceSpeed))
        //         photonView.RPC(nameof(SyncTargetPosition), RpcTarget.Others, photonView.ViewID, _playerTransform.position);
        // }

        _targetViewId = other.GetComponent<PhotonView>().ViewID;

        float duration = SkillData.ActivateTime * _circleFill.CurrentFillAmount;

        //# 다른 클라이언트에 동기화
        photonView.RPC(nameof(ApplyReduceSpeed), RpcTarget.Others, _targetViewId, duration);
    }

    /// <summary>
    /// 2D 충돌 감지에서 다른 객체와의 상호작용이 종료될 때 호출되는 메서드
    /// </summary>
    /// <param name="other">충돌 감지를 벗어난 Collider2D 객체</param>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (InGameManager.Instance.IsGameOver) return;
        if (photonView.IsMine && other.CompareTag("Player"))
        {
            _targetViewId = other.GetComponent<PhotonView>().ViewID;
            photonView.RPC(nameof(RemoveReduceSpeed), RpcTarget.Others, _targetViewId);

            _targetPlayer = null;
            _targetViewId = 0;

            photonView.RPC(nameof(SyncTargetPosition), RpcTarget.Others, photonView.ViewID, _playerTransform.position);
        }
    }

    /// <summary>
    /// 지정된 플레이어의 이동 속도를 지정된 시간 동안 감소시킴
    /// </summary>
    /// <param name="viewId">감소시키려는 플레이어의 Photon View ID</param>
    /// <param name="duration">속도 감소 효과가 지속되는 시간</param>
    [PunRPC]
    private void ApplyReduceSpeed(int viewId, float duration)
    {
        var targetView = PhotonView.Find(viewId);
        var otherStatus = targetView.GetComponent<IStatusEffectable>();

        var targetRB = targetView.gameObject.GetComponent<Rigidbody2D>();
        targetRB.gravityScale = 0f;
        targetRB.velocity = Vector2.zero;

        var targetPlayer = targetView.gameObject.GetComponent<PlayerController>();
        targetPlayer.SetZeroToRemoteVelocity();


        //# Abyssal Countdown의 끌려가는 효과에 의해 이속이 2배가 되어야 이동이 가능해짐
        otherStatus.ApplyStatusEffect(
            StatusEffectType.ReduceSpeed,
            1f,
            duration
        );
    }

    /// <summary>
    /// 지정된 플레이어의 속도 감소 효과를 제거
    /// </summary>
    /// <param name="viewId">속도 감소 효과를 제거할 플레이어의 Photon View ID</param>
    [PunRPC]
    private void RemoveReduceSpeed(int viewId)
    {
        var targetView = PhotonView.Find(viewId);
        var otherStatus = targetView.gameObject.GetComponent<IStatusEffectable>();

        var targetRB = targetView.gameObject.GetComponent<Rigidbody2D>();
        targetRB.gravityScale = 1f;

        if (otherStatus != null)
            otherStatus.RemoveStatusEffect(StatusEffectType.ReduceSpeed);
    }

    /// <summary>
    /// 타겟 위치를 동기화하는 원격 프로시저 호출 메서드
    /// </summary>
    /// <param name="viewId">동기화할 Photon 뷰의 ID</param>
    /// <param name="position">동기화할 위치 벡터</param>
    [PunRPC]
    private void SyncTargetPosition(int viewId, Vector3 position)
    {
        var targetView = PhotonView.Find(viewId);

        if (targetView != null) targetView.transform.position = position;
    }

    /// <summary>
    /// Photon 네트워크 뷰 동기화를 위해 뷰 데이터를 직렬화 및 역직렬화하는 메서드
    /// </summary>
    /// <param name="stream">데이터를 직렬화하거나 역직렬화할 PhotonStream 객체</param>
    /// <param name="info">데이터 전송에 대한 정보를 담고 있는 PhotonMessageInfo 객체</param>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
        }
        else
        {
            _networkPosition = (Vector3)stream.ReceiveNext();
        }
    }
}