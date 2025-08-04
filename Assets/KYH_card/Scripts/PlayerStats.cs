using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float reloadSpeed = 1.0f;
    public float projectileSpeed = 1.0f;
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public float blockCooldown = 5.0f;
    private Vector3 baseScale;

    private void Start()
    {
        baseScale = transform.localScale;
    }

    public void IncreaseMaxHealth(float percent)
    {
        maxHealth *= (1f + percent);
        currentHealth = maxHealth;
    }

    public void ChangeScale(float scaleMultiplier)
    {
        transform.localScale = baseScale * scaleMultiplier;
    }
}
