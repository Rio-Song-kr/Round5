using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatus", menuName = "Player/PlayerStatus")]
public class PlayerStatusDataSO : ScriptableObject
{
    [Header("Default Player Status Settings")]
    public float DefaultHp = 100f;
    public float DefaultMoveSpeed = 5f;

    [Header("Default Attack Settings")]
    public float DefaultAmmo = 3f;
    public float DefaultAttackSpeed = 5f;
    public float DefaultDamage = 10f;
    public float DefaultReloadSpeed = 2f;
}