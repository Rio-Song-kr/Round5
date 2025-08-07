using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class TutoOfflineMode : MonoBehaviourPunCallbacks
{
    private void Start()
    {
        StartCoroutine(ForceDisconnectAndSetOfflineMode());
    }

    /// <summary>
    /// Photon에 연결되어 있다면 강제로 끊고 오프라인 모드로 전환함
    /// </summary>
    private IEnumerator ForceDisconnectAndSetOfflineMode()
    {
        // 이미 오프라인 모드라면 아무것도 하지 않음
        if (PhotonNetwork.OfflineMode)
        {
            Debug.Log("이미 오프라인 모드입니다.");
            yield break;
        }

        // Photon에 연결되어 있으면 Disconnect 시도
        if (PhotonNetwork.IsConnected)
        {
            Debug.LogWarning("Photon에 연결되어 있어 Disconnect 실행...");
            PhotonNetwork.Disconnect();

            // 완전히 끊길 때까지 기다림
            yield return new WaitUntil(() =>
                PhotonNetwork.NetworkClientState == ClientState.Disconnected
            );
        }

        // 연결이 끊긴 후 오프라인 모드 설정
        PhotonNetwork.OfflineMode = true;
        Debug.Log("오프라인 모드 전환 완료: PhotonNetwork.OfflineMode = " + PhotonNetwork.OfflineMode);
    }
}