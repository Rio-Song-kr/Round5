using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NetworkCheck : MonoBehaviour
{
    [SerializeField] private TMP_Text _statusText;

    private void Update()
    {
        _statusText.text = "Network Status: " + (Photon.Pun.PhotonNetwork.NetworkClientState);
    }
}
