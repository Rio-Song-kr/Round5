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
        // Radius는 폭발 범위
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, 2f, _colls);

        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                Rigidbody2D rb = _colls[i].GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    // (n / distance.sqrMagnitude) n 부분 숫자가 높으면 폭발 강도가 세집니다.
                    Vector3 distance = _colls[i].transform.position - transform.position;
                    rb.AddForce(distance * (5f / distance.sqrMagnitude), ForceMode2D.Impulse);
                }
            }
        }
    }
}
