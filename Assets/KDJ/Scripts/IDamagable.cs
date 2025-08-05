using System;


public interface IDamagable
{
    void TakeDamage(float amount);
    bool IsAlive { get; }
    event Action OnDeath;
}

