using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CardEffect : MonoBehaviour
{
    public string cardName;
    public string description;
    public abstract void ApplyShotEffect(PlayerStats playerStats);

    public abstract void ApplyStatusEffect(PlayerStats playerStats);
}
