using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
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
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, 1f, _colls);

        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                IDamagable damagable = _colls[i].GetComponent<IDamagable>();
                Rigidbody2D rb = _colls[i].GetComponent<Rigidbody2D>();
                Vector3 distance = _colls[i].transform.position - transform.position;
                if (rb != null)
                {
                    // (n / distance.sqrMagnitude) n 부분 숫자가 높으면 폭발 강도가 세집니다.

                    if (distance.sqrMagnitude < 0.001f) continue; // 너무 가까우면 무시
                    rb.AddForce(distance * (0.1f / distance.sqrMagnitude), ForceMode2D.Impulse);
                }

                if (damagable != null)
                {
                    // 10f은 피해량. 이후 스텟 최종 피해량으로 변경 필요
                    float damage = 1f / distance.sqrMagnitude;
                    damage = Mathf.Clamp(damage, 0.1f, 10f); // 최소 0.1, 최대 10으로 제한
                    damagable.TakeDamage(damage);
                }
            }
        }
    }
}
