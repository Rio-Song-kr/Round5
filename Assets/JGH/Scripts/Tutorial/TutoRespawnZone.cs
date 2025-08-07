using UnityEngine;

public class TutoRespawnZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            TutoPlayerRespawn respawn = other.GetComponent<TutoPlayerRespawn>();
            if (respawn != null)
            {
                respawn.Respawn();
            }
        }
    }
}