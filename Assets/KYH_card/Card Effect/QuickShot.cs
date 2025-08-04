using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class QuickShot : CardEffect
{
    private void Awake()
    {
        cardName = "Quick Shot";
        description = "투사체 속도 1.5배, 재장전 속도 25% 감소";
    }

    public override void ApplyShotEffect(PlayerStats playerStats)
    {
        playerStats.projectileSpeed *= 1.5f;
        playerStats.reloadSpeed *= 1.25f;
    }

    public override void ApplyStatusEffect(PlayerStats playerStats)
    {
        
    }
}
