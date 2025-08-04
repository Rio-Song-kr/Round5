using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatus", menuName = "Player/PlayerStatus")]
public class PlayerStatusDataSO : ScriptableObject
{
    [Header("Default Player Status Settings")]
    public float DefaultHp = 13f;
    public float DefaultGroundSpeed = 5f;
    public float DefaultAirSpeed = 0.2f; 

    [Header("Default Attack Settings")]
    public float DefaultAmmo = 3f; // 기본 탄창 갯수
    public float DefaultAttackSpeed = 5f; // 공격 속도
    public float DefaultDamage = 6f; // 공격력
    public float DefaultReloadSpeed = 2f; // 재장전 시간
    // 250803 추가
    public float DefaultBulletSpeed = 10f; // 총알 속도 
    public float DefaultAttackDelay = 10f;
    
    // 250804 추가
    public float AmmoConsumption; // 소비 탄창 갯수

    [Header("Default Defence Settings")]
    public float DefaultInvincibilityCoolTime = 2f;
}