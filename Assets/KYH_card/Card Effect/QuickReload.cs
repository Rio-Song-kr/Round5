using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickReload : CardEffect
{
    private void Awake()
    {
        cardName = "Quick Reload";
        description = "재장전 속도 70% 증가";
    }

    public override void ApplyShotEffect(PlayerStats playerStats)
    {
        playerStats.reloadSpeed *= 0.3f;
    }

    public override void ApplyStatusEffect(PlayerStats playerStats)
    {
        
    }


}
