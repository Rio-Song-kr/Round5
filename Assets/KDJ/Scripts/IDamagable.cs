using System;
using UnityEngine;

public interface IDamagable
{
    void TakeDamage(float amount, Vector2 position, Vector2 direction);
    bool IsAlive { get; }
    event Action OnDeath;
}