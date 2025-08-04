using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Defender : CardEffect
{
    private void Awake()
    {
        cardName = "Defender";
        description = "블록 쿨타임 30% 감소, 체력 30% 증가";
    }

    public override void ApplyShotEffect(PlayerStats playerStats)
    {
        
    }

    public override void ApplyStatusEffect(PlayerStats playerStats)
    {
        playerStats.blockCooldown *= 0.7f;
        playerStats.IncreaseMaxHealth(0.3f);
    }
}
