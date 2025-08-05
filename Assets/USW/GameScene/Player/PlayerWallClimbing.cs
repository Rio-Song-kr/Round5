using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class PlayerWallClimbing : MonoBehaviourPun
{
    [Header("벽타기 Player 셋팅")] [SerializeField]
    private float wallClimbSpeed = 8f;

    [SerializeField] private float wallLeanSlideSpeed = 3f;
    [SerializeField] private float wallHoldSlideSpeed = 1f;
    [SerializeField] private float wallDetectionDistance = 0.3f;
    [SerializeField] private float wallJumpCooldown = 0.1f;

    [Header("벽점프 관련")] [SerializeField] private float wallJumpHorizontalForce = 1.2f;
    [SerializeField] private float wallJumpVerticalForce = 1.6f;

    [Header("벽이 맞는지 확인 용도")] [SerializeField]
    private Transform wallCheckLeft;

    [Header("이펙트")]
    [SerializeField] private float _wallJumpOffset = 0.2f;

    [SerializeField] private Transform wallCheckRight;
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.1f, 0.8f);

    // 시스템 참조
    private PlayerController playerController;
    private Rigidbody2D rb;
    private Collider2D col;
    private LayerMask groundLayerMask;

    private Transform actualLeftSensor, actualRightSensor;

    // 벽 관련 변수
    private bool isWallClimbing;
    private bool isWallLeaning;
    private bool isWallHolding;
    private bool isOnLeftWall;
    private bool isOnRightWall;
    private float wallTimer;
    private WallState currentWallState = WallState.None;

    // 입력 캐시
    private bool upInput;

    private enum WallState
    {
        None,
        Leaning,
        Holding,
        Climbing
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        HandleInput();
        UpdateWallCheck();
        HandleWallInteraction();
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        ApplyWallPhysics();
    }

    private void LateUpdate()
    {
        UpdateWallCheckPositions();
    }

    #region RPCs

    [PunRPC]
    private void OnWallJump(float jumpDirection, float jumpForce)
    {
        if (!photonView.IsMine) return;

        //# 방향에 따른 오프셋 적용
        playerController.PlayJumpEffect(
            Quaternion.Euler(0, 0, 30 * -jumpDirection),
            new Vector3(_wallJumpOffset * -jumpDirection, 0, 0)
        );

        // 벽 상태와 속도를 먼저 완전히 리셋
        ResetWallState();
        rb.velocity = Vector2.zero;

        // 프레임 단위로 대기 한다음 벽점프 적용
        StartCoroutine(DelayedWallJump(jumpDirection, jumpForce));

        if (playerController != null)
        {
            playerController.SetCanJump(true);
        }
    }

    [PunRPC]
    private void OnWallStateChanged(int newStateInt)
    {
        var newState = (WallState)newStateInt;


        // 추후 시각적 효과나 사운드 재생 등 
        string playerName = photonView.Owner != null ? photonView.Owner.NickName : "Local";

        switch (newState)
        {
            case WallState.Climbing:
                Debug.Log($"플레이어 {playerName}: 벽 오르기");
                break;
            case WallState.Holding:
                Debug.Log($"플레이어 {playerName}: 벽 붙잡기");
                break;
            case WallState.Leaning:
                Debug.Log($"플레이어 {playerName}: 벽에 기대기");
                break;
            case WallState.None:
                Debug.Log($"플레이어 {playerName}: 벽 상호작용 종료");
                break;
        }
    }

    #endregion

    #region Init

    public void Initialize(PlayerController controller, Rigidbody2D rigidbody, Collider2D collider,
        LayerMask wallLayerMask)
    {
        playerController = controller;
        rb = rigidbody;
        col = collider;
        groundLayerMask = wallLayerMask;

        SetupWallChecks();
    }

    private void SetupWallChecks()
    {
        if (wallCheckLeft == null)
        {
            var obj = new GameObject("WallCheckLeft");
            obj.transform.parent = transform;
            wallCheckLeft = obj.transform;
        }

        if (wallCheckRight == null)
        {
            var obj = new GameObject("WallCheckRight");
            obj.transform.parent = transform;
            wallCheckRight = obj.transform;
        }

        UpdateWallCheckPositions();
    }

    #endregion

    private void HandleInput()
    {
        upInput = Input.GetKey(KeyCode.W);
    }

    #region 벽 감지 부분

    private bool IsFacingRight()
    {
        float yRotation = transform.eulerAngles.y;
        return Mathf.Abs(yRotation) < 90f || Mathf.Abs(yRotation - 360f) < 90f;
    }

    private void UpdateWallCheckPositions()
    {
        if (col != null)
        {
            float xExtent = col.bounds.extents.x + wallDetectionDistance;

            if (wallCheckLeft != null)
                wallCheckLeft.localPosition = new Vector3(-xExtent, 0f, 0f);
            if (wallCheckRight != null)
                wallCheckRight.localPosition = new Vector3(xExtent, 0f, 0f);
        }
    }

    private void UpdateWallCheck()
    {
        if (playerController != null && playerController.IsGrounded())
        {
            ResetWallState();
            return;
        }

        bool prevLeftWall = isOnLeftWall;
        bool prevRightWall = isOnRightWall;

        bool isFacingRight = IsFacingRight();

        // Transform actualLeftSensor, actualRightSensor;

        if (isFacingRight)
        {
            actualLeftSensor = wallCheckLeft;
            actualRightSensor = wallCheckRight;
        }
        else
        {
            actualLeftSensor = wallCheckRight;
            actualRightSensor = wallCheckLeft;
        }

        var leftHit = Physics2D.OverlapBox(actualLeftSensor.position, wallCheckSize, 0f, groundLayerMask);
        var rightHit = Physics2D.OverlapBox(actualRightSensor.position, wallCheckSize, 0f, groundLayerMask);

        isOnLeftWall = leftHit != null;
        isOnRightWall = rightHit != null;

        if (!isOnLeftWall && !isOnRightWall)
        {
            ResetWallState();
        }
        else if (isOnLeftWall && !prevLeftWall || isOnRightWall && !prevRightWall)
        {
            if (playerController != null)
            {
                playerController.SetCanJump(true);
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // 왼쪽 센서 기즈모 (빨간색)
        if (actualLeftSensor != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(actualLeftSensor.position, new Vector3(wallCheckSize.x, wallCheckSize.y, 0f));
        }

        // 오른쪽 센서 기즈모 (초록색)
        if (actualRightSensor != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(actualRightSensor.position, new Vector3(wallCheckSize.x, wallCheckSize.y, 0f));
        }
    }
#endif

    public void ResetWallState()
    {
        var prevState = currentWallState;

        currentWallState = WallState.None;
        isWallClimbing = false;
        isWallLeaning = false;
        isWallHolding = false;
        wallTimer = 0f;

        // 상태 변경을 다른 클라이언트에 알림 
        if (prevState != WallState.None && photonView.IsMine)
        {
            photonView.RPC("OnWallStateChanged", RpcTarget.Others, (int)WallState.None);
        }
    }

    #endregion

    #region 벽 Interaction

    private void HandleWallInteraction()
    {
        if (playerController != null && playerController.IsGrounded())
        {
            ResetWallState();
            return;
        }

        if (!isOnLeftWall && !isOnRightWall)
        {
            ResetWallState();
            return;
        }

        var newWallState = DetermineWallState();
        if (newWallState != currentWallState)
        {
            OnWallStateChanged(newWallState);
        }

        HandleCurrentWallState();
    }

    private WallState DetermineWallState()
    {
        float moveInput = playerController != null ? playerController.GetMoveInput() : 0f;

        bool pressingTowardsWall = isOnLeftWall && moveInput < 0 || isOnRightWall && moveInput > 0;
        bool tryingToLeaveWall = isOnLeftWall && moveInput > 0 || isOnRightWall && moveInput < 0;

        if (tryingToLeaveWall) return WallState.None;
        if (!pressingTowardsWall) return WallState.None;

        bool jumpInputDown = playerController != null ? playerController.GetJumpInputDown() : false;

        if (pressingTowardsWall && upInput && jumpInputDown) return WallState.Climbing;
        if (pressingTowardsWall && upInput) return WallState.Holding;
        if (pressingTowardsWall) return WallState.Leaning;

        return WallState.None;
    }

    private void OnWallStateChanged(WallState newState)
    {
        var prevState = currentWallState;

        isWallClimbing = false;
        isWallLeaning = false;
        isWallHolding = false;

        currentWallState = newState;
        wallTimer = 0f;

        switch (newState)
        {
            case WallState.Climbing:
                isWallClimbing = true;
                Debug.Log("벽 오르기 시작");
                break;
            case WallState.Holding:
                isWallHolding = true;
                Debug.Log("벽 붙잡기 시작");
                break;
            case WallState.Leaning:
                isWallLeaning = true;
                Debug.Log("벽에 기대기 시작");
                break;
        }

        // 상태 변경을 다른 클라이언트에 전송
        if (photonView.IsMine && newState != prevState)
        {
            photonView.RPC("OnWallStateChanged", RpcTarget.Others, (int)newState);
        }
    }

    private void HandleCurrentWallState()
    {
        wallTimer += Time.deltaTime;
    }

    #endregion

    #region 벽 물리 부분

    private void ApplyWallPhysics()
    {
        switch (currentWallState)
        {
            case WallState.Climbing:
                rb.velocity = new Vector2(0f, wallClimbSpeed);
                ResetWallState();
                break;
            case WallState.Holding:
                if (rb.velocity.y > 0)
                    rb.velocity = new Vector2(rb.velocity.x * 0.1f, rb.velocity.y * 0.8f);
                else
                    rb.velocity = new Vector2(rb.velocity.x * 0.1f, Mathf.Max(rb.velocity.y, -wallHoldSlideSpeed));
                break;
            case WallState.Leaning:
                if (rb.velocity.y > 0)
                    rb.velocity = new Vector2(rb.velocity.x * 0.3f, rb.velocity.y * 0.6f);
                else
                    rb.velocity = new Vector2(rb.velocity.x * 0.3f, Mathf.Max(rb.velocity.y, -wallLeanSlideSpeed));
                break;
        }
    }

    #endregion

    #region 점프 관련 핸들링

    public bool TryHandleJump()
    {
        if (isOnLeftWall || isOnRightWall)
        {
            ExecuteWallJump();
            return true;
        }

        return false;
    }

    private void ExecuteWallJump()
    {
        float jumpDirection = isOnLeftWall ? 1f : -1f;
        float jumpForce = playerController != null ? playerController.GetJumpForce() : 12f;

        // 네트워크 RPC로 벽점프 실행
        if (photonView.IsMine)
        {
            photonView.RPC("OnWallJump", RpcTarget.All, jumpDirection, jumpForce);
        }
    }

    private IEnumerator DelayedWallJump(float jumpDirection, float jumpForce)
    {
        yield return new WaitForFixedUpdate();

        var wallJumpForce = new Vector2(
            jumpDirection * jumpForce * wallJumpHorizontalForce,
            jumpForce * wallJumpVerticalForce
        );

        rb.AddForce(wallJumpForce, ForceMode2D.Impulse);
    }

    #endregion

    #region 퍼블릭 메서드

    public bool IsWallInteracting() => currentWallState != WallState.None;

    #endregion
}