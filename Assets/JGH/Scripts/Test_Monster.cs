using UnityEngine;

public class Test_Monster : MonoBehaviour, IDamageable
{
    public float hp = 100f;

    public void TakeDamage(float damage)
    {
        hp -= damage;
        Debug.Log($"[DummyTarget] 피격! 현재 HP: {hp}");

        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }
}