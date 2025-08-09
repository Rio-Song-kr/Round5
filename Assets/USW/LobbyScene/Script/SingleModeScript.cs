using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SingleModeScript : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsSinglePlayerMode())
            {
                ReturnToLobby();
                Cursor.visible = true;
                SoundManager.Instance.PlayMainMenuBGM(); 
            }
        }
    }
    
    public static bool IsSinglePlayerMode()
    {
        if (!PhotonNetwork.InRoom) return false;
        
        string roomName = PhotonNetwork.CurrentRoom.Name;
        return roomName.StartsWith("SM_") && PhotonNetwork.CurrentRoom.PlayerCount == 1;
    }

    public void ReturnToLobby()
    {
        if (IsSinglePlayerMode())
        {
            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene("USW/LobbyScene/LobbyScene");
        }
    }

    public static void GoBackToLobby()
    {
        if (IsSinglePlayerMode())
        {
            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene("USW/LobbyScene/LobbyScene");
        }
    }
    
}
