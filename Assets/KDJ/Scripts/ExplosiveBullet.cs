using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveBullet : MonoBehaviour
{
    private Collider2D[] _colls = new Collider2D[20];

    void Start()
    {
        ExplosionShock();
    }

    private void ExplosionShock()
    {
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, 1f, _colls);
        
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                Rigidbody2D rb = _colls[i].GetComponent<Rigidbody2D>();
                if (rb != null)
                    rb.AddForce((Vector2)(_colls[i].transform.position - transform.position).normalized * 2f, ForceMode2D.Impulse);
            }
        }
    }
}
