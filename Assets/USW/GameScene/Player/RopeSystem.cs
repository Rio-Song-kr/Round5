using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class RopeSystem : MonoBehaviourPun
{
    [Header("로프 시스템")] [SerializeField] private float ropeReleaseJumpBonus = 1.5f;
    [SerializeField] private float ropeMovementMultiplier = 0.3f;


    private PlayerController playerController;
    private Rigidbody2D rb;
    private RopeSwingSystem ropeSystem;


    private bool wasSwingingLastFrame;
    private Vector2 ropeReleaseVelocity;

    private void Update()
    {
        if (!PhotonNetwork.OfflineMode){
            if (!photonView.IsMine) return;
        }

        HandleRopeIntegration();
        HandleRopeMovement();
    }


    #region Init

    public void Initialize(PlayerController controller, Rigidbody2D rigidbody)
    {
        playerController = controller;
        rb = rigidbody;
        ropeSystem = GetComponent<RopeSwingSystem>();
    }

    #endregion

    #region 로프 관련 핸들링

    private void HandleRopeIntegration()
    {
        if (ropeSystem == null) return;

        bool isCurrentlySwinging = ropeSystem.IsSwinging();

        // 로프에서 떨어졌을 때의 처리
        if (wasSwingingLastFrame && !isCurrentlySwinging)
        {
            OnRopeReleased();
        }

        // 로프에 새로 매달렸을 때의 처리
        if (!wasSwingingLastFrame && isCurrentlySwinging)
        {
            OnRopeAttached();
        }

        wasSwingingLastFrame = isCurrentlySwinging;
    }

    private void OnRopeAttached()
    {
        // 점프 상태 활성화
        if (playerController != null)
        {
            playerController.SetCanJump(true);
        }
    }

    private void OnRopeReleased()
    {
        // 로프에서 떨어질 때 
        ropeReleaseVelocity = rb.velocity;

        // 로프에서 떨어진 직후에는 공중 점프 가능
        if (playerController != null && !playerController.IsGrounded())
        {
            playerController.SetCanJump(true);
        }
    }

    #endregion

    #region 로프 움직임

    private void HandleRopeMovement()
    {
        // 로프에 매달려 있지 않으면 리턴
        if (ropeSystem == null || !ropeSystem.IsSwinging()) return;
        if (playerController == null) return;

        float moveInput = playerController.GetMoveInput();

        // 로프에 매달려 있을 때의 추가 이동
        if (moveInput != 0 && !playerController.IsGrounded())
        {
            float groundSpeed = playerController.GetCurrentGroundSpeed();
            float ropeMovement = moveInput * groundSpeed * ropeMovementMultiplier;
            rb.AddForce(Vector2.right * ropeMovement);
        }
    }

    #endregion

    #region 로프 점프 관련

    public bool TryHandleJump()
    {
        // 로프에서 점프하는 경우만 처리
        if (ropeSystem != null && ropeSystem.IsSwinging())
        {
            ExecuteRopeJump();
            return true;
        }

        return false;
    }

    private void ExecuteRopeJump()
    {
        if (playerController == null) return;

        Vector2 currentVel = rb.velocity;
        float jumpForce = playerController.GetJumpForce();
        float jumpPower = jumpForce * ropeReleaseJumpBonus;

        // 로프 해제
        ropeSystem.ForceDetachHook();

        // 점프 방향 계산
        Vector2 jumpDirection = Vector2.up;
        if (currentVel.magnitude > 1f)
        {
            jumpDirection = (Vector2.up + currentVel.normalized * 0.3f).normalized;
        }

        // 점프 실행
        playerController.ResetVelocityForJump(preserveHorizontal: true);
        rb.AddForce(jumpDirection * jumpPower, ForceMode2D.Impulse);

        // 점프 상태 비활성화
        playerController.SetCanJump(false);
    }

    #endregion

    #region 퍼블릭 메서드

    /// <summary>
    /// 현재 로프에 매달려 있는지 확인
    /// </summary>
    public bool IsSwinging()
    {
        return ropeSystem != null && ropeSystem.IsSwinging();
    }

    /// <summary>
    /// 현재 훅이 연결되어 있는지 확인
    /// </summary>
    public bool IsHookAttached()
    {
        return ropeSystem != null && ropeSystem.IsHookAttached();
    }

    /// <summary>
    /// 강제로 로프 해제
    /// </summary>
    public void ForceDetach()
    {
        if (ropeSystem != null && ropeSystem.IsSwinging())
        {
            ropeSystem.ForceDetachHook();
        }
    }

    /// <summary>
    /// 로프 릴리즈 점프 보너스 설정
    /// </summary>
    public void SetRopeReleaseJumpBonus(float newBonus)
    {
        ropeReleaseJumpBonus = Mathf.Max(newBonus, 1f);
    }

    /// <summary>
    /// 로프 이동 배율 설정
    /// </summary>
    public void SetRopeMovementMultiplier(float newMultiplier)
    {
        ropeMovementMultiplier = Mathf.Clamp(newMultiplier, 0f, 1f);
    }

    /// <summary>
    /// 로프 시스템 컴포넌트 가져오기
    /// </summary>
    public RopeSwingSystem GetRopeSwingSystem()
    {
        return ropeSystem;
    }

    /// <summary>
    /// 현재 로프 길이 가져오기
    /// </summary>
    public float GetCurrentRopeLength()
    {
        return ropeSystem != null ? ropeSystem.GetCurrentRopeLength() : 0f;
    }

    /// <summary>
    /// 훅 포인트 위치 가져오기
    /// </summary>
    public Vector2 GetHookPoint()
    {
        return ropeSystem != null ? ropeSystem.GetHookPoint() : Vector2.zero;
    }

    /// <summary>
    /// 로프 상태 정보
    /// </summary>
    public string GetRopeStateInfo()
    {
        if (ropeSystem == null) return "No RopeSystem";

        string playerName = photonView.Owner != null ? photonView.Owner.NickName : "Local";
        return
            $"Player: {playerName}, IsSwinging: {IsSwinging()}, IsHookAttached: {IsHookAttached()}, RopeLength: {GetCurrentRopeLength():F1}";
    }

    #endregion
}