public interface IStatusEffectable
{
    void ApplyStatusEffect(StatusEffectType type, float value, float duration);
    void RemoveStatusEffect(StatusEffectType type);
    bool HasStatusEffect(StatusEffectType type);
}