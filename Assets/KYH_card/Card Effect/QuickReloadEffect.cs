using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickReloadEffect : CardEffect
{
    public float reloadSpeedMultiplier = 0.3f;

    public override void ApplyEffect(GameObject player)
    {
       // Gun gun = player.GetComponentInChildren<Gun>();
       // if (gun != null)
       // {
       //     gun.reloadTime *= reloadSpeedMultiplier;
       // }
    }
}
