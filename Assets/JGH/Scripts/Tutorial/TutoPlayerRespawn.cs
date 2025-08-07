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

        if (checkpointManager.HasCheckpoint())
        {
            respawnPos = checkpointManager.GetLastCheckpointPosition();
            Debug.Log("✔ 마지막 체크포인트 위치로 리스폰됨");
        }
        else
        {
            respawnPos = initialPosition;
            Debug.Log("✔ 체크포인트 없어서 초기 위치로 리스폰됨");
        }

        transform.position = respawnPos;
    }
}