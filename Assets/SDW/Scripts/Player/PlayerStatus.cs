using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : MonoBehaviour, IStatusEffectable
{
    [SerializeField] private PlayerStatusDataSO _playerData;
    [SerializeField] private List<StatusEffect> _activeEffect = new List<StatusEffect>();

    private float _currentHp;
    private float _currentMaxHp;
    private float _currentGroundSpeed;
    private float _currentAirSpeed;
    // private float _currentAttackSpeed;

    private float _calculatedMaxHp;
    private float _calculatedGroundSpeed;
    private float _calculatedAirSpeed;
    // private float _calculatedAttackSpeed;

    private bool _isInvincibility;
    private bool _canAttack;

    public Action<float, float> OnPlayerSpeedValueChanged;
    public Action<bool> OnInvincibilityValueChanged;
    public Action<bool> OnPlayerCanAttackValueChanged;
    // public Action<float> OnAttackSpeedValueChanged;

    /// <summary>
    /// 초기화
    /// </summary>
    private void Awake()
    {
        _currentHp = _playerData.DefaultHp;
        _currentMaxHp = _playerData.DefaultHp;
        _currentGroundSpeed = _playerData.DefaultGroundSpeed;
        _currentAirSpeed = _playerData.DefaultAirSpeed;
        // _currentAttackSpeed = _playerData.DefaultAttackSpeed;

        //# 초기에는 기본값과 동일
        _calculatedMaxHp = _playerData.DefaultHp;
        _calculatedGroundSpeed = _playerData.DefaultGroundSpeed;
        // _calculatedAttackSpeed = _playerData.DefaultAttackSpeed;

        _canAttack = true;
    }

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
        // _currentAttackSpeed = _playerData.DefaultAttackSpeed;

        //# 초기화 후 Action을 통해 전달
        OnPlayerSpeedValueChanged?.Invoke(_calculatedGroundSpeed, _calculatedAirSpeed);
        OnInvincibilityValueChanged?.Invoke(_isInvincibility);
        OnPlayerCanAttackValueChanged?.Invoke(_canAttack);
        // OnAttackSpeedValueChanged?.Invoke(_calculatedAttackSpeed);
    }

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

    private void CalculateStatus()
    {
        float prevSpeed = _calculatedGroundSpeed;
        bool prevInvincibility = _isInvincibility;
        bool prevCanAttack = _canAttack;
        // float prevAttackSpeed = _calculatedAttackSpeed;

        _calculatedGroundSpeed = _playerData.DefaultGroundSpeed;
        _isInvincibility = false;
        _canAttack = true;
        // _calculatedAttackSpeed = _playerData.DefaultAttackSpeed;

        foreach (var effect in _activeEffect)
        {
            switch (effect.EffectType)
            {
                case StatusEffectType.ReduceSpeed:
                case StatusEffectType.FreezePlayer:
                    _calculatedGroundSpeed = _calculatedGroundSpeed * (1f + effect.EffectValue);
                    _calculatedAirSpeed = _calculatedAirSpeed * (1f + effect.EffectValue);
                    Debug.Log($"Move Speed Changed : {_calculatedGroundSpeed}");
                    break;
                case StatusEffectType.UnableToAttack:
                    _canAttack = false;
                    Debug.Log($"Can Attack Changed : {_canAttack}");
                    // _calculatedAttackSpeed = _calculatedAttackSpeed * (1f + effect.EffectValue);
                    break;
                //# 무적인 상태에서 총알이 맞으면, 시간이 연장되어야 함
                case StatusEffectType.Invincibility:
                    _isInvincibility = true;
                    Debug.Log($"Is Invincibility Changed : {_isInvincibility}");
                    break;
            }
        }

        //# 값이 변경된 경우에만 Invoke
        if (Mathf.Abs(prevSpeed - _calculatedGroundSpeed) > 0.1f)
            OnPlayerSpeedValueChanged?.Invoke(_calculatedGroundSpeed, _calculatedAirSpeed);

        if (prevInvincibility != _isInvincibility)
            OnInvincibilityValueChanged?.Invoke(_isInvincibility);

        if (prevCanAttack != _canAttack)
            OnPlayerCanAttackValueChanged?.Invoke(_canAttack);

        // if (Mathf.Abs(prevAttackSpeed - _calculatedAttackSpeed) > 0.1f)
        //     OnAttackSpeedValueChanged?.Invoke(_calculatedAttackSpeed);
    }

    private void CalculatePermanentEffects()
    {
        foreach (var effect in _activeEffect)
        {
            switch (effect.EffectType)
            {
                case StatusEffectType.IncreaseMaxHp:
                    _calculatedMaxHp = _calculatedMaxHp * (1 + effect.EffectValue);
                    Debug.Log($"Max Hp Changed : {_calculatedMaxHp}");
                    break;
            }
        }

        //# 영구 적용 효과는 바로 적용
        InitializeStatus();
    }

    /// <summary>
    /// 상태 효과를 적용
    /// </summary>
    /// <param name="type">적용할 상태 효과의 종류</param>
    /// <param name="effectValue">효과의 강도 또는 값 (예: 감소 속도의 경우 감소 비율)</param>
    /// <param name="duration">효과 지속 시간</param>
    public void ApplyStatusEffect(StatusEffectType type, float effectValue, float duration, bool isPermanent = false)
    {
        var existingEffect = GetStatusEffect(type);

        //# 동일한 효과가 없다면 추가
        if (existingEffect == null)
        {
            _activeEffect.Add(new StatusEffect(type, effectValue, duration, isPermanent));

            if (isPermanent) CalculatePermanentEffects();
        }
        //# 동일한 효과가 있을 경우, 시간 갱신
        else existingEffect.RemainingTime = duration;
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