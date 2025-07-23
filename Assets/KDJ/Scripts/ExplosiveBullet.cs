using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveBullet : MonoBehaviour
{
    private Collider2D[] _colls = new Collider2D[20];
    private CameraShake _cameraShake;

    void Start()
    {
        ExplosionShock();
        _cameraShake = Camera.main.GetComponent<CameraShake>();
        _cameraShake.ShakeCaller(0.65f, 0.1f);

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
                {
                    Vector3 distance = _colls[i].transform.position - transform.position;
                    rb.AddForce(distance * (1.5f / distance.sqrMagnitude), ForceMode2D.Impulse);
                }
            }
        }
    }
}
