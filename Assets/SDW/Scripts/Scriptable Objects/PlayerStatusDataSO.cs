using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatus", menuName = "Player/PlayerStatus")]
public class PlayerStatusDataSO : ScriptableObject
{
    [Header("Default Player Status Settings")]
    public float DefaultHp = 100f;
    public float DefaultGroundSpeed = 5f;
    public float DefaultAirSpeed = 0.2f;

    [Header("Default Attack Settings")]
    public float DefaultAmmo = 3f;
    public float DefaultAttackSpeed = 5f;
    public float DefaultDamage = 10f;
    public float DefaultReloadSpeed = 2f;
    // 250803 추가
    public float DefaultBulletSpeed = 10f;
    public float DefaultAttackDelay = 10f;

    [Header("Default Defence Settings")]
    public float DefaultInvincibilityCoolTime = 2f;
}