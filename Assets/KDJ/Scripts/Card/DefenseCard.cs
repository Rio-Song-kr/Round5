using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDefenseCard", menuName = "CardBase/DefenseCard")]
public class DefenseCard : CardBase
{
    [Header("방어 카드")]
    [Header("방어 카드 정보")]
    public DefenceSkills DefenceSkill;
}
