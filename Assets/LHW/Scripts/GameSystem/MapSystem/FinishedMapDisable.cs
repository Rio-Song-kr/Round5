using UnityEngine;


/// <summary>
/// 맵이 전환될 때, 맵의 개수가 늘어나서 생기는 딜레이를 줄이기 위해 이미 사용된 맵을 비활성화하는 스크립트
/// 오브젝트에 부착하여 맵이 해당 영역에 들어왔을 시 비활성화
/// </summary>
public class FinishedMapDisable : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.isActiveAndEnabled) collision.gameObject.GetComponentInParent<MapDynamicMovement>().gameObject.SetActive(false);
    }
}