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

    public StatusEffect(StatusEffectType type, float value, float duration, bool isPermanent)
    {
        EffectType = type;
        EffectValue = value;
        Duration = duration;
        RemainingTime = duration;
        IsPermanent = isPermanent;
    }
}