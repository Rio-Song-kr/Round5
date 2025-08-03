using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAttackCard", menuName = "CardBase/AttackCard")]
public class AttackCard : CardBase
{
    [Header("공격 카드")]
    [Header("공격 카드 정보")]
    public float BulletSpeedMultiplier;
    public float DamageMultiplier;
    public float ReloadTimeMultiplier;
    public float ReloadTimeAddition;
    public int AmmoIncrease;
    [Header("무기 정보")]
    // 0은 스텟 관련 카드, 1 = Laser, 2 = Explosive, 3 = Barrage
    public int WeaponIndex;
}

