using UnityEngine;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    private VideoPlayer videoPlayer;

    void Start()
    {
        // 재정의 용 겟 컴퍼넌트
        videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer != null)
        {
            return;
        }
    }
}