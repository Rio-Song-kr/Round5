using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class GameEndManager : MonoBehaviourPunCallbacks
{
    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    private void Update()
    {
        // // ������ ������ ��Ȳ�� ���Ƿ� �����.
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     // �����Ͱ� true, �ٸ� �ִ� false
        //     photonView.RPC(nameof(SetIsWinner), RpcTarget.All, PhotonNetwork.IsMasterClient);
        //     if (PhotonNetwork.IsMasterClient)
        //     {
        //         PhotonNetwork.LoadLevel("CardTest");
        //     }
        // }
        // else if (Input.GetKeyDown(KeyCode.Return))
        // {
        //     // �����Ͱ� false, �ٸ� �ִ� true
        //     photonView.RPC(nameof(SetIsWinner), RpcTarget.All, !PhotonNetwork.IsMasterClient);
        //     if (PhotonNetwork.IsMasterClient)
        //     {
        //         PhotonNetwork.LoadLevel("CardTest");
        //     }
        // }
    }
    //
    // [PunRPC]
    // public void SetIsWinner(bool masterIsWinner)
    // {
    //     bool isWinner = PhotonNetwork.IsMasterClient ? masterIsWinner : !masterIsWinner;
    //
    //     ExitGames.Client.Photon.Hashtable winnerProp = new ExitGames.Client.Photon.Hashtable
    //     {
    //         { "isWinner", isWinner }
    //     };
    //
    //     PhotonNetwork.LocalPlayer.SetCustomProperties(winnerProp);
    //     Debug.Log($"[GameEndManager] ���� {(isWinner ? "����" : "����")}�Դϴ�.");
    // }
}