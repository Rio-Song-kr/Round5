using UnityEngine;

public class FinishedMapDisable : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.isActiveAndEnabled) collision.gameObject.GetComponentInParent<MapDynamicMovement>().gameObject.SetActive(false);
    }
}
