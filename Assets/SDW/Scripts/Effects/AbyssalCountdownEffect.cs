using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 원형 카운트다운 이펙트를 제어하는 컨트롤러
/// </summary>
public class AbyssalCountdownEffect : MonoBehaviourPun, IPunObservable
{
    [Header("Circle UI")]
    //# 카운트다운 게이지의 채워지는 원형 이미지
    [SerializeField] private Image _filledCircleImg;
    //# 카운트다운 게이지의 움직이는 원형 이미지
    [SerializeField] private Image _movingCircleImg;
    //# 카운트다운 게이지의 움직이는 라인 이미지
    [SerializeField] private Image _movingLineImg;
    //# 쉴드 효과 원형 이미지

    [Header("Effects Prefabs")]
    //# 카운트다운 완료 시 활성화할 octagon 이펙트 오브젝트
    [SerializeField] private GameObject _octagonObject;

    //# Abyssal Countdown Skill Data
    private AbyssalSkillDataSO _skillData;
    //# 생성된 VFX 오브젝트의 파티클 시스템 컴포넌트
    private ParticleSystem _particle;
    //# 생성된 VFX 오브젝트의 참조
    private GameObject _vfxObject;
    //# 현재 게이지의 채움 정도 (0~1)
    private float _currentFillAmount = 0f;
    //# 목표로 하는 게이지의 채움 정도 (0~1)
    private float _targetFillAmount = 1f;
    //# 현재 게이지가 증가 모드인지 여부 (true: 증가, false: 감소)
    private bool _canIncrease = true;

    [Header("Shield Effect")]
    [SerializeField] private GameObject _shieldObject;
    [SerializeField] private bool _playerMoved;
    private float _shieldTimeCount;
    private float _moveAcceleratesInvincibilityLossRate = 0.2f;
    private float _shieldActiveTime = 2f;
    private bool _shieldEffectActivated;

    private Coroutine _scalingCoroutine;
    private Transform _playerTransform;
    private IStatusEffectable _status;

    private void OnDisable()
    {
        _skillData.Pools.Destroy(_vfxObject);
    }

    /// <summary>
    /// 카운트다운 이펙트 초기화 메서드
    /// </summary>
    private void Start()
    {
        //# 파티클을 중지하고 기존 파티클들을 제거
        _particle.Stop();
        _particle.Clear();
    }

    /// <summary>
    /// 카운트다운 게이지를 업데이트하고 이펙트 상태를 관리
    /// </summary>
    private void Update()
    {
        if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha4)) _playerMoved = !_playerMoved;
        //# 현재 채움량을 목표값으로 부드럽게 이동
        if (_currentFillAmount > _targetFillAmount) _currentFillAmount -= Time.deltaTime / _skillData.ActivateTime;
        else if (_currentFillAmount < _targetFillAmount) _currentFillAmount += Time.deltaTime / _skillData.ChargeTime;

        //# 증가 모드에서 게이지가 거의 완전히 채워졌을 때의 처리
        if (_canIncrease && Mathf.Abs(_currentFillAmount - 1f) < 0.01f)
        {
            //# 게이지를 완전히 채우고 감소 모드로 전환
            _currentFillAmount = 1f;
            _targetFillAmount = 0f;
            _canIncrease = false;

            StartEffect();
        }
        //# 감소 모드에서 게이지가 거의 완전히 비워졌을 때의 처리
        else if (!_canIncrease && Mathf.Abs(_currentFillAmount) < 0.01f)
        {
            //# 게이지를 완전히 비우고 증가 모드로 전환
            _currentFillAmount = 0f;
            _targetFillAmount = 1f;
            _canIncrease = true;

            EndEffect();
        }

        if (_shieldEffectActivated)
        {
            //# 플레이어가 움직일 경우, 사용 시간 감소
            if (_playerMoved) _shieldTimeCount += Time.deltaTime * (1 + _moveAcceleratesInvincibilityLossRate);
            else _shieldTimeCount += Time.deltaTime;

            if (_shieldTimeCount >= _shieldActiveTime)
            {
                _status.RemoveStatusEffect(StatusEffectType.Invincibility);
                _shieldObject.SetActive(false);
                _shieldTimeCount = 0f;
                _shieldEffectActivated = false;
            }
        }

        //# 계산된 채움량을 UI 요소들에 적용
        SetFillAmount(_currentFillAmount);
    }

    /// <summary>
    /// effect의 위치 동기화
    /// </summary>
    private void LateUpdate()
    {
        if (transform == null || _playerTransform == null) return;
        transform.position = _playerTransform.position;
        _vfxObject.transform.position = _playerTransform.position;
    }

    /// <summary>
    /// VFX 오브젝트를 생성하고 초기 상태를 설정
    /// </summary>
    public void Initialize(AbyssalSkillDataSO skillData, Transform playerTransform)
    {
        _skillData = skillData;
        _playerTransform = playerTransform;
        _status = playerTransform.GetComponent<PlayerStatus>();

        _shieldTimeCount = 0f;

        //# VFX 프리팹을 현재 위치에 생성하여 자식으로 설정
        _vfxObject = _skillData.Pools.Instantiate("VFX_CorePoolEffect", transform.position, transform.rotation);
        _vfxObject.transform.parent = transform;

        //# 생성된 VFX 오브젝트에서 파티클 시스템 컴포넌트를 가져옴
        _particle = _vfxObject.GetComponentInChildren<ParticleSystem>();

        _shieldObject.GetComponent<ShieldEffect>().Init(_skillData.ShieldScaleMultiplier, _skillData.ShieldScaleDuration);
        _shieldObject.SetActive(false);

        //# 파티클을 중지하고 기존 파티클들을 제거
        _particle.Stop();
        _particle.Clear();

        //# octagon 이펙트는 초기에 비활성화
        _octagonObject.SetActive(false);
    }

    /// <summary>
    /// 계산된 채움량을 각 UI 이미지 요소에 적용하여 카운트다운 효과를 시각화
    /// </summary>
    /// <param name="fillAmount">적용할 채움량 (0~1 범위)</param>
    public void SetFillAmount(float fillAmount)
    {
        //# 채워지는 원형 이미지와 움직이는 원형 이미지의 fillAmount 설정
        _filledCircleImg.fillAmount = fillAmount;
        _movingCircleImg.fillAmount = fillAmount;

        //# 움직이는 라인 이미지를 채움량에 따라 회전 (360도 기준)
        _movingLineImg.transform.rotation = Quaternion.Euler(0f, 0f, -fillAmount * 360f);
    }

    /// <summary>
    /// 카운트다운 효과의 시작 시 호출되는 메서드
    /// 파티클 효과를 시작하고 목표 스케일로 확대하며, 특정 이펙트 객체를 활성화
    /// </summary>
    public void StartEffect()
    {
        //# 파티클 효과 시작 및 octagon 이펙트 활성화
        _particle.Clear();
        _particle.Play();

        if (_scalingCoroutine != null) StopCoroutine(_scalingCoroutine);
        _scalingCoroutine =
            StartCoroutine(ScaleOverTime(Vector3.one * _skillData.TargetScaleMultiplier, _skillData.ScalingDuration));

        UseShieldEffect();
        _octagonObject.SetActive(true);
        _shieldTimeCount = 0f;
    }

    /// <summary>
    /// 카운트다운 효과의 끝에 호출되는 메서드
    /// </summary>
    public void EndEffect()
    {
        //# 파티클 효과 중지 및 octagon 이펙트 비활성화
        _particle.Stop();

        if (_scalingCoroutine != null) StopCoroutine(_scalingCoroutine);
        _scalingCoroutine = StartCoroutine(ScaleOverTime(Vector3.one, _skillData.ScalingDuration));

        _octagonObject.SetActive(false);
    }

    //todo Coroutine 대신에 TimeDeltaTime으로 처리해야할 듯
    //# 플레이어의 이동이 있을 때는 사용 시간이 깍히는 시간이 20% 증가해야 함
    //# 이동 시 -> Time.deltaTime * 1.2, 정지 시 -> Time.deltaTime
    private void UseShieldEffect()
    {
        _shieldObject.SetActive(true);

        foreach (var status in _skillData.Status)
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
    /// <returns></returns>
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
            stream.SendNext(_playerMoved);
        else
            _playerMoved = (bool)stream.ReceiveNext();
    }
}