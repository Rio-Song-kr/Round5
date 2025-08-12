using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class NetworkSetting : MonoBehaviourPunCallbacks
{
    [SerializeField] private PlayerController _player;
    [SerializeField] private RopeSwingSystem _ropeSwing;

    private void Start()
    {
        // PhotonNetwork.OfflineMode = true; // 오프라인 모드로 설정

        // _player.SetIsStarted(true);
        InGameManager.Instance.SetStartedOffline(true);
        SoundManager.Instance.PlayBGMLoop("BGM1");
    }
}

