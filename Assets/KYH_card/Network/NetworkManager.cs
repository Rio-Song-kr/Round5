using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
public class NetworkManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI StateText;
    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("마스터 서버 연결됨");
        loadingPanel.SetActive(false);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.Log($"{cause} 로 인해 연결이 끊어짐");
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("재연결중");
    }
    private void Update()
    {
        StateText.text = $"Current State : {PhotonNetwork.NetworkClientState}";
    }
}
