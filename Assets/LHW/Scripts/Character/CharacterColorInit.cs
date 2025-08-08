using Photon.Pun;
using UnityEngine;

public class CharacterColorInit : MonoBehaviourPun
{
    [SerializeField] private MeshRenderer[] meshRenderer;
    [SerializeField] private Material[] materials;
    private int playerColorIndex;

    private void Start()
    {
        if (photonView.IsMine)
        {
            playerColorIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
            photonView.RPC(nameof(SetPlayerColorRPC), RpcTarget.All, playerColorIndex);
        }
    }

    [PunRPC]
    private void SetPlayerColorRPC(int index)
    {
        playerColorIndex = index;
        for(int i = 0; i < meshRenderer.Length; i++)
        {
            meshRenderer[i].material = materials[playerColorIndex];
        }
    }
}
