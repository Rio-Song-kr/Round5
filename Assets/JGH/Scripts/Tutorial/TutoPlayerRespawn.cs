using UnityEngine;

public class TutoPlayerRespawn : MonoBehaviour
{
    [SerializeField] private TutoCheckpointManager checkpointManager;

    private Vector3 initialPosition;

    private void Start()
    {
        // 처음 위치 저장
        initialPosition = transform.position;
    }

    public void Respawn()
    {
        Vector3 respawnPos;
        
        GetComponent<RopeSwingSystem>().DetachHook();

        if (checkpointManager.HasCheckpoint())
        {
            respawnPos = checkpointManager.GetLastCheckpointPosition();
        }
        else
        {
            respawnPos = initialPosition;
        }

        transform.position = respawnPos;
    }
}