using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDefenseCard", menuName = "CardBase/DefenseCard")]
public class DefenseCard : CardBase
{
    [Header("방어 카드")]
    [Header("방어 카드 정보")]
    public float HpMultiplier;
    public float DefenseSkillCooldownMultiplier;
    public float DefenseSkillCooldownAddition;

    [Header("방어 기술 정보")]
    // 0 = AbyssalCountdown, 1 = Emp, 2 = FrostSlam, 3 = 능력치 카드
    public int DefenseSkillIndex;
}
