using UnityEngine;

public class TutoCheckpoint : MonoBehaviour
{
    public int checkpointID;
    [SerializeField] private TutoCheckpointManager checkpointManager;

    private int playerLayer;
    private SpriteRenderer spriteRenderer;
    private bool isActivated = false;

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer("Player");

        // SpriteRenderer 가져오기 (자신 또는 자식 포함)
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // 초기 색상: 회색
        if (spriteRenderer != null)
            spriteRenderer.color = Color.red;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isActivated) return;

        if (other.gameObject.layer == playerLayer)
        {
            isActivated = true;
            checkpointManager.SetCheckpoint(checkpointID, transform.position);

            if (spriteRenderer != null)
                spriteRenderer.color = Color.green;
        }
    }
}