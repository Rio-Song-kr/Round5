public interface IStatusEffectable
{
    void ApplyStatusEffect(StatusEffectType type, float effectValue, float duration, bool isPermanent = false);
    void RemoveStatusEffect(StatusEffectType type);
    bool HasStatusEffect(StatusEffectType type);
    StatusEffect GetStatusEffect(StatusEffectType type);
}