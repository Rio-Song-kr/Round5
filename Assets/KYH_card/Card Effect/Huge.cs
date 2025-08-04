using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Huge : CardEffect
{
    private void Awake()
    {
        cardName = "Huge";
        description = "최대 체력 80% 증가, 캐릭터 크기 30% 증가";
    }

    public override void ApplyStatusEffect(PlayerStats playerStats)
    {
        playerStats.IncreaseMaxHealth(0.8f);
        playerStats.ChangeScale(1.3f);
    }

    public override void ApplyShotEffect(PlayerStats playerStats)
    {
        
    }
}
