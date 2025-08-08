using Photon.Pun;
using UnityEngine;

public class BallCooltimeMeshRenderer : MonoBehaviourPun
{
    [SerializeField] PlayerStatus status;
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] Material[] materials;

    private bool lastIsCooldownZero;

    private void Start()
    {
        if (status == null)
            status = GetComponent<PlayerStatus>();

        UpdateMesh(status.InvincibilityCooldown <= 0);
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        bool isCooldownZero = status.InvincibilityCooldown <= 0;
        if (isCooldownZero != lastIsCooldownZero)
        {
            UpdateMesh(isCooldownZero);
            lastIsCooldownZero = isCooldownZero;
        }
    }

    private void UpdateMesh(bool isCooltimeZero)
    {
        photonView.RPC(nameof(RPC_MeshChange), RpcTarget.All, isCooltimeZero);
    }

    [PunRPC]
    private void RPC_MeshChange(bool isCooldownZero)
    {
        meshRenderer.material = isCooldownZero ? materials[0] : materials[1];
        Debug.Log(isCooldownZero);
    }
}