using UnityEngine;

public class RopeController : MonoBehaviour
{
    /// <summary>
    /// 플렛폼이 파괴되었을 때 로프 오브젝트 비활성화
    /// </summary>
    public void RopeDestroy()
    {
        gameObject.SetActive(false);
    }
}