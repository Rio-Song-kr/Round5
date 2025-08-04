using System;

[Serializable]
public class StatusEffect
{
    public StatusEffectType EffectType;
    //# 효과 비율
    public float EffectValue;
    //# 지속 시간
    public float Duration;
    //# 남은 시간
    public float RemainingTime;
    //# 영구적인 효과인지
    public bool IsPermanent;
    public bool CanAddPlayer;

    /// <summary>
    /// 플레이어의 상태 효과를 관리하고 적용하는 클래스
    /// </summary>
    public StatusEffect(StatusEffectType type, float value, float duration, bool isPermanent = false, bool canAddPlayer = true)
    {
        // 초기화
        EffectType = type;
        EffectValue = value;
        Duration = duration;
        RemainingTime = duration;
        IsPermanent = isPermanent;
        CanAddPlayer = canAddPlayer;
    }
}