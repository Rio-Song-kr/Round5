public enum StatusEffectType
{
    None,
    //# Emp(40%, 2s), Frost Slam(50%, 1s)
    ReduceSpeed,
    //# Abyssal Countdown(5s), Frost Slam(0.5s)
    FreezePlayer,
    //# Frost Slam(0.5s)
    UnableToAttack,
    //# Abyssal Countdown(5s)
    Invincibility,
    //# Abyssal Countdown(20%, Player 이동 -> Effect에서 가속)
    ReduceInvincibilityTime,
    //# Abyssal Countdown(5s)
    PullOtherPlayer,
    IncreaseCooldown,
    //# Emp(30%, 영구)
    IncreaseMaxHp
}