public interface IStatusEffectable
{
    public void ApplyStatusEffect(
        StatusEffectType type,
        float effectValue,
        float duration,
        bool isPermanent = false,
        DefenceSkills skills = DefenceSkills.None
    );
    void RemoveStatusEffect(StatusEffectType type);
    bool HasStatusEffect(StatusEffectType type);
    StatusEffect GetStatusEffect(StatusEffectType type);
}