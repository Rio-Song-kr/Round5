using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
//using Managers;

public class TestNetwork : MonoBehaviourPunCallbacks
{
    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    /// <summary>
    /// OnConnectedToMaster 있고
    /// OnDisconnectedFromServer
    /// OnJoinRoom
    /// OnLeftRoom
    /// 플레이어 관리도 얘가 해야하나 ?
    /// 
    /// </summary>
    public void OnConnectedToServer()
    {
        Debug.Log("서버 연결 시도");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("끊기");
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnected();
        Debug.Log("연결 됨");
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Debug.Log("방 들어감");
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 4 };
        PhotonNetwork.JoinOrCreateRoom("TestRoom", roomOptions, TypedLobby.Default);
        //Manager.InitPool();
    }
    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        Debug.Log("방 만듬");
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("마스터 클라이언트 입니다.");
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("온조인룸");
    }
}