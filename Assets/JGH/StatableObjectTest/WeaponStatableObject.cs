using UnityEngine;

[CreateAssetMenu(menuName = "Statable/Barrage")]
public class BarrageStable : ScriptableObject
{
    public string description = "여러발의 총알을 동시에 발사";
    public int additionalProjectiles = 4;
    public int additionalAmmo = 5;
    public float damageMultiplier = 0.3f; // -70%
    public float reloadTimeModifier = 0.25f;
    public float blockCooldownModifier = 0f;
    public float hpModifier = 0f;
    public float bulletSpeedMultiplier = 1.0f;
    public string specialNote = "";
}

[CreateAssetMenu(menuName = "Statable/Buckshot")]
public class BuckshotStable : ScriptableObject
{
    public string description = "산탄총처럼 여러 발 산포";
    public int additionalProjectiles = 4;
    public int additionalAmmo = 5;
    public float damageMultiplier = 0.4f; // -60%
    public float reloadTimeModifier = 0.25f;
    public float blockCooldownModifier = 0f;
    public float hpModifier = 0f;
    public float bulletSpeedMultiplier = 1.0f;
    public string specialNote = "";
}

[CreateAssetMenu(menuName = "Statable/Razer")]
public class RazerStable : ScriptableObject
{
    public string description = "마우스 방향으로 2초간 레이저 발사 (0.5초당 지속 데미지), 6발 소모";
    public int additionalProjectiles = 0;
    public int additionalAmmo = 0;
    public float damageMultiplier = 1.0f;
    public float reloadTimeModifier = 0f;
    public float blockCooldownModifier = 0f;
    public float hpModifier = 0f;
    public float bulletSpeedMultiplier = 1.0f;
    public string specialNote = "2초간 지속 피해, 공격보조카드 적용 제외";
}
