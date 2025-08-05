using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerStatus : MonoBehaviour, IStatusEffectable
{
    [SerializeField] private PlayerStatusDataSO _playerData;
    [SerializeField] private List<StatusEffect> _activeEffect = new List<StatusEffect>();
    // private Dictionary<DefenceSkills, StatusEffect> _statusEffects = new Dictionary<DefenceSkills, StatusEffect>();
    private List<DefenceSkills> _defenceSkillList = new List<DefenceSkills>();
    private List<StatusEffect> _statusEffectList = new List<StatusEffect>();

    private float _currentHp;
    private float _currentMaxHp;
    private float _currentGroundSpeed;
    private float _currentAirSpeed;
    private float _currentInvincibilityCooldown;

    private float _calculatedMaxHp;
    private float _calculatedGroundSpeed;
    private float _calculatedAirSpeed;
    private float _calculatedInvincibilityCooldown;
    public float InvincibilityCooldown => _calculatedInvincibilityCooldown;

    private bool _isInvincibility;
    private bool _canAttack;
    private bool _freezePlayer;

    public Action<float, float> OnPlayerHpValueChanged;
    public Action<float, float> OnPlayerSpeedValueChanged;
    public Action<bool> OnPlayerFreezeValueChanged;
    public Action<bool> OnPlayerCanAttackValueChanged;
    public Action<bool> OnInvincibilityValueChanged;

    /// <summary>
    /// 초기화
    /// </summary>
    private void Awake()
    {
        _currentHp = _playerData.DefaultHp;
        _currentMaxHp = _playerData.DefaultHp;
        _currentGroundSpeed = _playerData.DefaultGroundSpeed;
        _currentAirSpeed = _playerData.DefaultAirSpeed;
        _currentInvincibilityCooldown = _playerData.DefaultInvincibilityCoolTime;

        //# 초기에는 기본값과 동일
        _calculatedMaxHp = _playerData.DefaultHp;
        _calculatedGroundSpeed = _playerData.DefaultGroundSpeed;
        _calculatedAirSpeed = _playerData.DefaultAirSpeed;
        _calculatedInvincibilityCooldown = _playerData.DefaultInvincibilityCoolTime;

        _canAttack = true;
    }

    /// <summary>
    /// 게임 오브젝트의 상태를 업데이트하고 활성화된 상태 효과를 관리
    /// </summary>
    private void Update() => UpdateStatusEffects();

    /// <summary>
    /// Stage가 바뀔 때 호출하여 Effect가 적용된 값으로 적용
    /// </summary>
    public void InitializeStatus()
    {
        _currentHp = _calculatedMaxHp;
        _currentMaxHp = _calculatedMaxHp;
        _currentGroundSpeed = _playerData.DefaultGroundSpeed;
        _currentAirSpeed = _playerData.DefaultAirSpeed;
        _isInvincibility = false;
        _canAttack = true;
        _freezePlayer = false;
        _currentInvincibilityCooldown = _calculatedInvincibilityCooldown;

        //# 초기화 후 Action을 통해 전달
        OnPlayerHpValueChanged?.Invoke(_currentHp, _currentMaxHp);
        OnPlayerSpeedValueChanged?.Invoke(_currentGroundSpeed, _currentAirSpeed);
        OnPlayerFreezeValueChanged?.Invoke(_freezePlayer);
        OnPlayerCanAttackValueChanged?.Invoke(_canAttack);
        OnInvincibilityValueChanged?.Invoke(_isInvincibility);
    }

    /// <summary>
    /// 활성화된 상태 효과들을 업데이트
    /// </summary>
    private void UpdateStatusEffects()
    {
        for (int i = 0; i < _activeEffect.Count; i++)
        {
            //# 영구 적용 효과인 경우에는 return;
            if (_activeEffect[i].IsPermanent) continue;

            _activeEffect[i].RemainingTime -= Time.deltaTime;
            if (_activeEffect[i].RemainingTime <= 0) _activeEffect.RemoveAt(i);
        }

        CalculateStatus();
    }

    /// <summary>
    /// 상태 효과를 계산하고 플레이어의 상태 변수를 업데이트
    /// </summary>
    private void CalculateStatus()
    {
        float prevSpeed = _calculatedGroundSpeed;
        bool prevInvincibility = _isInvincibility;
        bool prevCanAttack = _canAttack;
        bool prevPlayerFreeze = _freezePlayer;

        _calculatedGroundSpeed = _playerData.DefaultGroundSpeed;
        _isInvincibility = false;
        _canAttack = true;
        _freezePlayer = false;

        foreach (var effect in _activeEffect)
        {
            switch (effect.EffectType)
            {
                case StatusEffectType.ReduceSpeed:
                    _calculatedGroundSpeed *= 1f + effect.EffectValue;
                    // _calculatedAirSpeed *= 1f + effect.EffectValue;
                    break;
                case StatusEffectType.FreezePlayer:
                    _freezePlayer = true;
                    break;
                case StatusEffectType.UnableToAttack:
                    _canAttack = false;
                    break;
                //# 무적인 상태에서 총알이 맞으면, 시간이 연장되어야 함
                case StatusEffectType.Invincibility:
                    _isInvincibility = true;
                    break;
            }
        }

        //# 값이 변경된 경우에만 Invoke
        if (Mathf.Abs(prevSpeed - _calculatedGroundSpeed) > 0.05f)
        {
            OnPlayerSpeedValueChanged?.Invoke(_calculatedGroundSpeed, _calculatedAirSpeed);
            Debug.Log($"Move Speed Changed : {_calculatedGroundSpeed}");
        }

        if (prevCanAttack != _canAttack)
        {
            OnPlayerCanAttackValueChanged?.Invoke(_canAttack);
            Debug.Log($"Can Attack Changed : {_canAttack}");
        }

        if (prevInvincibility != _isInvincibility)
        {
            OnInvincibilityValueChanged?.Invoke(_isInvincibility);
            Debug.Log($"Is Invincibility Changed : {_isInvincibility}");
        }

        if (prevPlayerFreeze != _freezePlayer)
        {
            OnPlayerFreezeValueChanged?.Invoke(_freezePlayer);
            Debug.Log($"Freeze Player Changed : {_freezePlayer}");
        }
    }

    /// <summary>
    /// 영구적인 상태 효과를 계산하고 적용
    /// </summary>
    /// <param name="effect">적용할 상태 효과</param>
    private void CalculatePermanentEffects(StatusEffect effect)
    {
        float prevMaxHp = _calculatedMaxHp;
        float prevInvincibilityCoolTime = _calculatedInvincibilityCooldown;

        _calculatedMaxHp = _playerData.DefaultHp;
        _calculatedInvincibilityCooldown = _playerData.DefaultInvincibilityCoolTime;

        //# 이펙트 내에서 더하기 쿨다운을 모두 계산한 후 아래에서 곱하기
        float multiplyCooldown = 1;
        float additionalCooldown = 0;
        float multiplyMaxHp = 0;

        for (int i = 0; i < _defenceSkillList.Count; i++)
        {
            switch (_defenceSkillList[i])
            {
                case DefenceSkills.Emp:
                    if (_statusEffectList[i].EffectType == StatusEffectType.IncreaseCooldown)
                        additionalCooldown += _statusEffectList[i].EffectValue;
                    else if (_statusEffectList[i].EffectType == StatusEffectType.IncreaseMaxHp)
                        multiplyMaxHp += _statusEffectList[i].EffectValue;
                    break;
                case DefenceSkills.ShieldUp:
                    if (_statusEffectList[i].EffectType == StatusEffectType.IncreaseCooldown)
                        additionalCooldown += _statusEffectList[i].EffectValue;
                    //todo 재장전
                    break;
                case DefenceSkills.Defender:
                    if (_statusEffectList[i].EffectType == StatusEffectType.DecreaseCooldown)
                        multiplyCooldown *= _statusEffectList[i].EffectValue;
                    else if (_statusEffectList[i].EffectType == StatusEffectType.IncreaseMaxHp)
                        multiplyMaxHp += _statusEffectList[i].EffectValue;
                    break;
                case DefenceSkills.Huge:
                    if (_statusEffectList[i].EffectType == StatusEffectType.IncreaseMaxHp)
                        multiplyMaxHp += _statusEffectList[i].EffectValue;
                    break;
            }
        }
        Debug.Log($"Multiply : {multiplyMaxHp}");

        if (multiplyCooldown == 1f) multiplyCooldown = 0f;

        //todo 체력에 대한 것도 추가 해야 함

        switch (effect.EffectType)
        {
            case StatusEffectType.IncreaseMaxHp:
                _calculatedMaxHp *= 1 + multiplyMaxHp;
                Debug.Log($"Max Hp Changed : {_calculatedMaxHp}");
                break;
            case StatusEffectType.IncreaseCooldown:
                _calculatedInvincibilityCooldown =
                    (_calculatedInvincibilityCooldown + additionalCooldown) * (1 + multiplyCooldown);
                Debug.Log($"Shield CoolTime Changed : {_calculatedInvincibilityCooldown}");
                break;
        }

        //# 영구 적용 효과는 바로 적용
        if (Mathf.Abs(prevMaxHp - _calculatedMaxHp) > 0.05f ||
            Mathf.Abs(prevInvincibilityCoolTime - _calculatedInvincibilityCooldown) > 0.05f)
            InitializeStatus();
    }

    /// <summary>
    /// 플레이어에게 상태 효과를 적용
    /// </summary>
    /// <param name="type">적용할 상태 효과의 유형</param>
    /// <param name="effectValue">상태 효과의 강도 값</param>
    /// <param name="duration">상태 효과의 지속 시간 (초)</param>
    /// <param name="isPermanent">상태 효과가 영구적인지 여부(기본값: false)</param>
    /// <param name="skill">스킬 이름 정보(영구 적용 효과에서만 사용</param>
    public void ApplyStatusEffect(
        StatusEffectType type,
        float effectValue,
        float duration,
        bool isPermanent = false,
        DefenceSkills skillName = DefenceSkills.None
    )
    {
        var existingEffect = GetStatusEffect(type);

        //# 동일한 효과가 없다면 추가
        if (existingEffect == null || isPermanent)
        {
            var effect = new StatusEffect(type, effectValue, duration, isPermanent);
            _activeEffect.Add(effect);

            if (isPermanent)
            {
                _defenceSkillList.Add(skillName);
                _statusEffectList.Add(effect);
                CalculatePermanentEffects(effect);
            }
        }
        //# 동일한 효과가 있을 경우, 시간 갱신
        else
        {
            if (Mathf.Abs(effectValue) > Mathf.Abs(existingEffect.EffectValue))
                existingEffect.EffectValue = effectValue;
            existingEffect.RemainingTime = duration;
        }
    }

    /// <summary>
    /// 지정된 상태 효과를 제거
    /// </summary>
    /// <param name="type">제거할 상태 효과의 종류</param>
    public void RemoveStatusEffect(StatusEffectType type)
    {
        _activeEffect.RemoveAll(e => e.EffectType == type);
    }

    /// <summary>
    /// 특정 상태 효과의 존재 여부를 확인
    /// </summary>
    /// <param name="type">검사할 상태 효과의 종류</param>
    /// <returns>지정된 상태 효과가 존재하면 true, 그렇지 않으면 false</returns>
    public bool HasStatusEffect(StatusEffectType type) => _activeEffect.Exists(e => e.EffectType == type);

    /// <summary>
    /// 지정된 상태 효과 타입에 해당하는 상태 효과를 가져옴
    /// </summary>
    /// <param name="type">찾으려는 상태 효과의 종류</param>
    /// <returns>해당 상태 효과 타입의 <see cref="StatusEffect"/> 인스턴스를 반환합니다. 존재하지 않으면 null을 반환</returns>
    public StatusEffect GetStatusEffect(StatusEffectType type) => _activeEffect.Find(e => e.EffectType == type);
}