using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveBullet : MonoBehaviour
{
    private Collider2D[] _colls = new Collider2D[20];

    void Start()
    {
        ExplosionShock();
        CameraShake.Instance.ShakeCaller(0.65f, 0.1f);

    }

    public void ExplosionShock()
    {
        // Radius는 폭발 범위
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, 1.5f, _colls);

        if (count > 0) 
        {
            for (int i = 0; i < count; i++)
            {
                Rigidbody2D rb = _colls[i].GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    // (n / distance.sqrMagnitude) n 부분 숫자가 높으면 폭발 강도가 세집니다.
                    Vector3 distance = _colls[i].transform.position - transform.position;
                    if (distance.sqrMagnitude < 0.001f) continue; // 너무 가까우면 무시
                    rb.AddForce(distance * (0.1f / distance.sqrMagnitude), ForceMode2D.Impulse);

                    // 플레이어가 있다면 거리에 비례해 피해를 입히는 로직
                }
            }
        }
    }
}
