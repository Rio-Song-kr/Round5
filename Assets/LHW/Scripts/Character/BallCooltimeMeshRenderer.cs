using System;
using Photon.Pun;
using UnityEngine;

public class BallCooltimeMeshRenderer : MonoBehaviourPun
{
    [SerializeField] private PlayerStatus status;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Material[] materials;

    private bool lastIsCooldownZero;
    private DefenceSkillManager _defenceSkillManager;

    private void Start()
    {
        if (status == null)
            status = GetComponent<PlayerStatus>();

        if (_defenceSkillManager == null)
            _defenceSkillManager = GetComponentInParent<DefenceSkillManager>();

        // UpdateMesh(status.InvincibilityCooldown <= 0);
    }

    private void OnEnable()
    {
        if (_defenceSkillManager == null)
            _defenceSkillManager = GetComponentInParent<DefenceSkillManager>();

        _defenceSkillManager.OnCanUseActiveSkill += UpdateMesh;
    }

    private void OnDisable()
    {
        if (_defenceSkillManager != null)
            _defenceSkillManager.OnCanUseActiveSkill -= UpdateMesh;
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        //
        // bool isCooldownZero = status.InvincibilityCooldown <= 0;
        // if (isCooldownZero != lastIsCooldownZero)
        // {
        //     UpdateMesh(isCooldownZero);
        //     lastIsCooldownZero = isCooldownZero;
        // }
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