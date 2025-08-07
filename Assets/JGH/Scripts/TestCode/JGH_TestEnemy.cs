using Photon.Pun;
using UnityEngine;

public class JGH_TestEnemy : MonoBehaviourPun
{
    [Header("몬스터 체력")]
    public float maxHp = 10f;
    private float currentHp;

    private void Start()
    {
        currentHp = maxHp;
    }
    

    [PunRPC]
    public void TakeDamage(float amount)
    {
        currentHp -= amount;
        Debug.Log($"[{gameObject.name}] 데미지 {amount} 받음 → 남은 체력: {currentHp}");

        if (currentHp <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"[{gameObject.name}] 사망!");
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}