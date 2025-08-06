using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitTest : MonoBehaviour
{
    [SerializeField] private GameObject _deadEffect1;
    [SerializeField] private GameObject _deadEffect2;
    private float _hp = 5f;

    public bool IsAlive => throw new NotImplementedException();

    public event Action OnDeath;

    public void TakeDamage(float damage)
    {
        _hp -= damage;
        if (_hp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
        GameObject deadEffect1 = Instantiate(_deadEffect1, transform.position, Quaternion.identity);
        GameObject deadEffect2 = Instantiate(_deadEffect2, transform.position, Quaternion.identity);
        deadEffect1.transform.LookAt(transform.position + Vector3.right);
        deadEffect2.transform.LookAt(transform.position + Vector3.right);
    }
}
