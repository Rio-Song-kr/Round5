using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class GameEndManager : MonoBehaviourPunCallbacks
{

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    void Update()
    {
        // 게임이 끝나는 상황을 임의로 출력함.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 마스터가 true, 다른 애는 false
            photonView.RPC(nameof(SetIsWinner), RpcTarget.All, PhotonNetwork.IsMasterClient);
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel("CardTest");
            }
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            // 마스터가 false, 다른 애는 true
            photonView.RPC(nameof(SetIsWinner), RpcTarget.All, !PhotonNetwork.IsMasterClient);
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel("CardTest");
            }
        }
    }

    [PunRPC]
    public void SetIsWinner(bool masterIsWinner)
    {
        bool isWinner = PhotonNetwork.IsMasterClient ? masterIsWinner : !masterIsWinner;

        ExitGames.Client.Photon.Hashtable winnerProp = new ExitGames.Client.Photon.Hashtable
        {
            { "isWinner", isWinner }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(winnerProp);
        Debug.Log($"[GameEndManager] 나는 {(isWinner ? "승자" : "패자")}입니다.");
    }
}
