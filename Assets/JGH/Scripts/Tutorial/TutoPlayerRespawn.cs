using UnityEngine;
using UnityEngine.SceneManagement;

public class TutoPlayerRespawn : MonoBehaviour
{
    [SerializeField] private TutoCheckpointManager checkpointManager;

    private Vector3 initialPosition;

    private void Start()
    {
        // 처음 위치 저장
        if (SceneManager.GetActiveScene().name == "USW_RopePlayMode")
        {
            if (checkpointManager == null)
            {
                checkpointManager = FindObjectOfType<TutoCheckpointManager>();
            }
        }

        initialPosition = new Vector3(0, -1.65f, 0); // 초기 위치 설정
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