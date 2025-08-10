using System;
using Photon.Pun;
using UnityEngine;

public class ExplosiveBullet : MonoBehaviourPun
{
    private Collider2D[] _colls = new Collider2D[20];
    private int _count = 0;

    private void Start()
    {
        ExplosionShock();
        SoundManager.Instance.PlaySFX("ExplosionSound" + UnityEngine.Random.Range(1, 3));
        CameraShake.Instance.ShakeCaller(0.65f, 0.1f);
    }

    public void ExplosionShock()
    {
        Array.Clear(_colls, 0, _colls.Length); // Clear the array before use
        // Radius는 폭발 범위
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, 1f, _colls);

        if (count > 0 && photonView.IsMine)
        {
            for (int i = 0; i < count; i++)
            {
                var damagable = _colls[i].GetComponent<IDamagable>();
                var rb = _colls[i].GetComponent<Rigidbody2D>();
                var distance = _colls[i].transform.position - transform.position;

                var hitPosition = _colls[i].ClosestPoint(transform.position);
                Vector2 hitNormal = distance.normalized;

                if (rb != null)
                {
                    // (n / distance.sqrMagnitude) n 부분 숫자가 높으면 폭발 강도가 세집니다.

                    if (distance.sqrMagnitude < 0.001f) continue; // 너무 가까우면 무시
                    rb.AddForce(distance * (0.1f / distance.sqrMagnitude), ForceMode2D.Impulse);
                }

                if (damagable != null)
                {
                    // 6f은 피해량. 이후 스텟 최종 피해량으로 변경 필요
                    float damage = 0.5f / distance.sqrMagnitude;
                    damage = Mathf.Clamp(damage, 0.1f, 3f); // 최소 0.1, 최대 3으로 제한
                    damagable.TakeDamage(damage, hitPosition, hitNormal);
                }
            }
        }
    }
}