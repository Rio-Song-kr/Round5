using UnityEngine;

public class TutoCheckpointManager : MonoBehaviour
{
    private Vector3 lastCheckpointPosition;
    private int lastCheckpointID = -1;

    public void SetCheckpoint(int id, Vector3 position)
    {
        if (id > lastCheckpointID)
        {
            lastCheckpointID = id;
            lastCheckpointPosition = position;
        }
    }

    public Vector3 GetLastCheckpointPosition()
    {
        return lastCheckpointPosition;
    }

    public bool HasCheckpoint()
    {
        return lastCheckpointID != -1;
    }
}