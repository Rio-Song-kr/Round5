using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class NetworkSetting : MonoBehaviourPunCallbacks
{
    [SerializeField] private PlayerController _player;

    private void Start()
    {
        PhotonNetwork.OfflineMode = true; // 오프라인 모드로 설정
        _player.SetIsStarted(true);
    }

    void Update()
    {
        Debug.Log("오프라인 모드 체크 : " + PhotonNetwork.OfflineMode);
    }
}
