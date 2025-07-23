using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class TestManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject _PlayerPrefab;
    [SerializeField] private Camera _camera;

    public override void OnJoinedRoom()
    {
        Debug.Log("방에 참가하셨습니다.");
        PhotonNetwork.Instantiate("TestPlayer", new Vector2(Random.Range(8f, 0), Random.Range(-4f, 4f)), Quaternion.identity);
    }
}
