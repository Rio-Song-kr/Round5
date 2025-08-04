using UnityEngine;
using Photon.Pun;
using System.Reflection;
using UnityEngine.Android;

public class USW_PlayerStatus : MonoBehaviourPun
{
    [SerializeField] private float statusEffectRopeMultiplier = 0.8f;

    // 컴포넌트들
    private PlayerController playerController;
    private PlayerStatus playerStatus;
    private RopeSwingSystem ropeSystem;

    // 원래 값들 저장 
    private float originalRopeClimbSpeed;
    private float originalSwingForce;
    private bool isStatusEffectActive;


    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerStatus = GetComponent<PlayerStatus>();
        ropeSystem = GetComponent<RopeSwingSystem>();

        StoreOriginalValues();

        if (photonView.IsMine)
        {
            SetupEventListeners();
        }
    }

    private void OnDestroy()
    {
        if (photonView.IsMine)
        {
            RemoveEventListeners();
        }
    }


    #region RPCs

    [PunRPC]
    private void OnStatusEffectApplied(int effectType, float value, float duration, bool isPermanent)
    {
        if (!photonView.IsMine) return;

        StatusEffectType type = (StatusEffectType)effectType;

        if (playerStatus != null)
        {
            playerStatus.ApplyStatusEffect(type, value, duration, isPermanent);
        }
    }

    [PunRPC]
    private void OnStatusEffectRemoved(int effectType)
    {
        if (!photonView.IsMine) return;

        StatusEffectType type = (StatusEffectType)effectType;

        if (playerStatus != null)
        {
            playerStatus.RemoveStatusEffect(type);
        }
    }

    [PunRPC]
    private void OnEmergencyReset()
    {
        if (!photonView.IsMine) return;

        // 모든 상태 효과 제거
        if (playerStatus != null)
        {
            playerStatus.RemoveStatusEffect(StatusEffectType.ReduceSpeed);
            playerStatus.RemoveStatusEffect(StatusEffectType.FreezePlayer);
            playerStatus.RemoveStatusEffect(StatusEffectType.UnableToAttack);
            playerStatus.RemoveStatusEffect(StatusEffectType.Invincibility);
        }

        // 로프 해제
        if (ropeSystem != null && ropeSystem.IsHookAttached())
        {
            ropeSystem.ForceDetachHook();
        }

        // 플레이어 상태 초기화
        if (playerController != null)
        {
            playerController.EnableJumpState();
            playerController.ResetMoveSpeed();
        }

        // 로프 값 복구
        RestoreOriginalRopeValues();
        isStatusEffectActive = false;
    }

    [PunRPC]
    private void OnVisualEffectChanged(int effectType, bool isActive)
    {
        // 모든 클라이언트에서 시각적 효과 적용/제거
        StatusEffectType type = (StatusEffectType)effectType;

        switch (type)
        {
            case StatusEffectType.Invincibility:
                if (isActive) ApplyInvincibilityVisualEffect();
                else RemoveInvincibilityVisualEffect();
                break;

            case StatusEffectType.UnableToAttack:
                if (isActive) ApplyCannotAttackVisualEffect();
                else RemoveCannotAttackVisualEffect();
                break;

            case StatusEffectType.ReduceSpeed:
            case StatusEffectType.FreezePlayer:
                if (isActive) ApplySpeedEffectVisualEffect();
                else RemoveSpeedEffectVisualEffect();
                break;
        }
    }

    #endregion

    #region Visual Effect Methods

    private void ApplyInvincibilityVisualEffect()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.7f);
        }
        // TODO:
        // 추가 시각적 효과 (파티클, 깜빡임 등)
        // 파티클시스템이라던지 다른거 집어넣기
    }

    private void RemoveInvincibilityVisualEffect()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    private void ApplyCannotAttackVisualEffect()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 0.5f, 0.5f, 1f);
        }
    }

    private void RemoveCannotAttackVisualEffect()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    private void ApplySpeedEffectVisualEffect()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(0.7f, 0.7f, 1f, 1f);
        }

        //TODO: 이동 트레일 변경, 파티클 효과 등
    }

    private void RemoveSpeedEffectVisualEffect()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    #endregion

    #region 셋업

    private void StoreOriginalValues()
    {
        originalRopeClimbSpeed = 5f;
        originalSwingForce = 300f;
    }

    private void SetupEventListeners()
    {
        if (playerStatus != null)
        {
            playerStatus.OnInvincibilityValueChanged += HandleInvincibilityChange;
            playerStatus.OnPlayerCanAttackValueChanged += HandleCanAttackChange;
        }
    }

    private void RemoveEventListeners()
    {
        if (playerStatus != null)
        {
            playerStatus.OnInvincibilityValueChanged -= HandleInvincibilityChange;
            playerStatus.OnPlayerCanAttackValueChanged -= HandleCanAttackChange;
        }
    }

    #endregion

    #region 이벤트핸들러 (로컬 플레이어용)

    /// <summary>
    /// 무적 상태 변경 시 호출 - 로컬 플레이어만
    /// </summary>
    private void HandleInvincibilityChange(bool isInvincible)
    {
        if (!photonView.IsMine) return;

        // 시각적 효과 동기화
        photonView.RPC("OnVisualEffectChanged", RpcTarget.All, (int)StatusEffectType.Invincibility, isInvincible);
    }

    /// <summary>
    /// 공격 가능 상태 변경 시 호출 - 로컬 플레이어만
    /// </summary>
    private void HandleCanAttackChange(bool canAttack)
    {
        if (!photonView.IsMine) return;

        // 시각적 효과 동기화
        photonView.RPC("OnVisualEffectChanged", RpcTarget.All, (int)StatusEffectType.UnableToAttack, !canAttack);
    }

    #endregion

    #region Status Effect Application

    /// <summary>
    /// 상태 효과를 로프 시스템에 적용
    /// </summary>
    private void ApplyStatusEffectToRope(float speedMultiplier)
    {
        if (!photonView.IsMine) return;

        float baseSpeed = GetBaseSpeed();
        float effectMultiplier = speedMultiplier / baseSpeed;

        // 상태 효과가 적용되었는지 확인
        if (Mathf.Abs(effectMultiplier - 1f) > 0.1f)
        {
            isStatusEffectActive = true;

            // 로프 성능에 상태 효과 적용
            float ropeEffectMultiplier = Mathf.Lerp(statusEffectRopeMultiplier, 1f, effectMultiplier);

            float newClimbSpeed = originalRopeClimbSpeed * ropeEffectMultiplier;
            float newSwingForce = originalSwingForce * ropeEffectMultiplier;

            // RopeSwingSystem에 새로운 값 적용
            if (ropeSystem != null)
            {
                ropeSystem.SetRopeClimbSpeed(newClimbSpeed);
                ropeSystem.SetSwingForce(newSwingForce);
            }
        }
        else
        {
            // 상태 효과가 해제됨
            if (isStatusEffectActive)
            {
                RestoreOriginalRopeValues();
                isStatusEffectActive = false;
            }
        }
    }

    /// <summary>
    /// 로프 시스템 값을 원래대로 복구
    /// </summary>
    private void RestoreOriginalRopeValues()
    {
        if (ropeSystem != null)
        {
            ropeSystem.SetRopeClimbSpeed(originalRopeClimbSpeed);
            ropeSystem.SetSwingForce(originalSwingForce);
        }
    }

    /// <summary>
    /// 기본 속도 가져오기
    /// </summary>
    private float GetBaseSpeed()
    {
        if (playerStatus != null)
        {
            // PlayerStatusDataSO에서 기본 속도 가져오기 
            var playerDataField = playerStatus.GetType().GetField("_playerData",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (playerDataField != null)
            {
                var playerData = playerDataField.GetValue(playerStatus) as PlayerStatusDataSO;
                if (playerData != null)
                {
                    return playerData.DefaultGroundSpeed;
                }
            }
        }

        return 5f;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 특정 상태 효과를 플레이어에게 적용 
    /// </summary>
    public void ApplyStatusEffect(StatusEffectType type, float value, float duration, bool isPermanent = false)
    {
        if (photonView.IsMine)
        {
            // 로컬에서 직접 적용
            if (playerStatus != null)
            {
                playerStatus.ApplyStatusEffect(type, value, duration, isPermanent);
            }
        }
        else
        {
            // 다른 플레이어에게 RPC로 전송
            photonView.RPC("OnStatusEffectApplied", RpcTarget.All, (int)type, value, duration, isPermanent);
        }
    }

    #endregion
}