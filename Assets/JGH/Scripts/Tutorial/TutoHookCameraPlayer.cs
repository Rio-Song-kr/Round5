using Cinemachine;
using Photon.Pun;

public class TutoHookCameraPlayer : MonoBehaviourPunCallbacks
{
    private CinemachineVirtualCamera virtualCamera;

    void Start()
    {
        virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();

        if (photonView.IsMine)
        {
            // Follow는 내 로컬 플레이어일 때만 지정
            if (virtualCamera != null)
            {
                virtualCamera.Follow = transform;
                virtualCamera.LookAt = transform;
            }
        }
    }
    
}
