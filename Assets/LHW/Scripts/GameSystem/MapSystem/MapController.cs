using DG.Tweening;
using Photon.Pun;
using System.Collections;
using UnityEngine;
using Photon.Realtime;

/// <summary>
/// ������ ���� 3���� ���� �� ���� ���� �� �̵��ϰ� �ϴ� ��ũ��Ʈ
/// </summary>
public class MapController : MonoBehaviourPunCallbacks
{
    [Header("Offset")]
    [Tooltip("�� ��ȯ ���� ������")]
    [SerializeField] private float mapChangeDelay = 1f;

    [SerializeField] private GameObject[] rounds;
    public float MapChangeDelay => mapChangeDelay;

    private Coroutine moveCoroutine;

    private void OnEnable()
    {
        InGameManager.OnRoundEnd += GoToNextStage;
    }

    private void OnDisable()
    {
        InGameManager.OnRoundEnd -= GoToNextStage;
    }

    public void GoToNextStage()
    {
        MapShake();
        MapMove();
    }

    /// <summary>
    /// ���� ���� �� ���� �� �� ��鸲
    /// </summary>
    private void MapShake()
    {
        gameObject.transform.DOShakePosition(0.5f, 1, 10, 90);
    }

    /// <summary>
    /// ������ �ð� ���� ���� �������� ���۵�
    /// </summary>
    private void MapMove()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (InGameManager.Instance.CurrentMatch > 2) return;

        moveCoroutine = StartCoroutine(MovementCoroutine());
    }

    private IEnumerator MovementCoroutine()
    {
        var delay = new WaitForSeconds(mapChangeDelay);

        var movements = rounds[InGameManager.Instance.CurrentMatch].GetComponentsInChildren<MapDynamicMovement>();

        for (int i = 0; i < movements.Length; i++)
        {
            if (movements[i] != null)
            {
                movements[i].DynamicMove();
                yield return delay;
            }
        }

        moveCoroutine = null;

        //todo 여기에서 player 위치 초기화 및 중력 1로 설정해줘야 함
        
        // MapController에서 직접 플레이어 제어
        photonView.RPC(nameof(InitPlayers), RpcTarget.All);
    }

    //# 20250807 추가작업분
    [PunRPC]
    private void InitPlayers()
    {
        
        var allPlayers = FindObjectsOfType<PlayerController>();
    
        foreach (var player in allPlayers)
        {
            // 일단 IsMine 으로 떄렸는데 혹시 의도가 맞지 않으면 수정해주시면 됩니다.
            if (player.photonView.IsMine) 
            {
                //플레이어 위치 초기화
                ResetPlayerPosition(player);
            
                //중력 활성화
                SetPlayerGravity(player, true);
            
                //모든 시스템 활성화
                InGameManager.OnplayerSystemActivate?.Invoke(true);
            }
        }
        
    }

    /// <summary>
    /// 플레이어 위치 리셋 메서드입니다.
    /// 각 라운드별로 다른 스폰 위치로 이동시키기위함입니다.
    /// </summary>
    private void ResetPlayerPosition(PlayerController player)
    {
        player.SetPosition(Vector3.zero);
    }

    /// <summary>
    /// 플레이어 중력 설정
    /// </summary>
    private void SetPlayerGravity(PlayerController player, bool isActive)
    {
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = isActive ? 1f : 0f;
        }
    }
    
    
    
    
    
    
    
}