using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPun, IPunObservable
{
    [Header("참조")] 
    [SerializeField] private PlayerStatusDataSO playerData;

    [Header("이동 관련 변수")] [SerializeField]
    private float acceleration = 50f;

    [SerializeField] private float deceleration = 50f;
    [SerializeField] private float velocityPower = 0.9f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;

    [Header("땅 체크")] [SerializeField]
    private Transform groundCheck;

    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.8f, 0.1f);
    [SerializeField] private LayerMask groundLayerMask;

    [Header("이동 네트워크 ")] [SerializeField]
    private float positionLerpRate = 10f;

    [SerializeField] private float rotationLerpRate = 15f;
    [SerializeField] private float velocityLerpRate = 8f;

    // 컴포넌트들
    private Rigidbody2D rb;
    private Collider2D col;
    private PlayerWallClimbing wallSystem;
    private RopeSystem ropeSystem;

    // 이동 변수들
    private float currentGroundSpeed;
    private float currentAirSpeed;
    private float moveInput;
    private bool facingRight = true;

    // 점프 변수들
    private bool isGrounded;
    private bool canJump;
    private bool hasJumpedInAir;

    // 입력 변수들
    private bool jumpInput;
    private bool jumpInputDown;
    private bool jumpInputUp;

    // 네트워크 동기화 변수들 
    private Vector3 remotePosition;
    private Vector2 remoteVelocity;
    private Quaternion remoteRotation;
    private bool remoteFacingRight;
    private bool remoteIsGrounded;
    private double lastReceivedTime;

 

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        wallSystem = GetComponent<PlayerWallClimbing>();
        ropeSystem = GetComponent<RopeSystem>();
        
        remotePosition = transform.position;
        remoteRotation = transform.rotation;
        remoteVelocity = Vector2.zero;
        remoteFacingRight = facingRight;
        remoteIsGrounded = isGrounded;

        SetupGroundCheck();
        InitializeStats();
        SetupSystemReferences();
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            HandleInput();
            UpdateGroundCheck();
            HandleJump();
        }
        else
        {
            SmoothMoveRemotePlayer();
            UpdateRemotePlayerState();
        }
    }

    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            HandleMovement();
        }
    }

  

    #region 네트워크 동기화 부분 

    /// <summary>
    /// 원격 플레이어 위치 및 회전 보간과 지연 보상
    /// </summary>
    private void SmoothMoveRemotePlayer()
    {
        // 네트워크 지연 계산
        double lag = PhotonNetwork.Time - lastReceivedTime;
        if (lag < 0) lag = 0;

        // 속도와 지연을 기반으로 위치 예측
        Vector3 predictedPosition = remotePosition + (Vector3)(remoteVelocity * (float)lag);

        // 보간
        transform.position = Vector3.Lerp(transform.position, predictedPosition, Time.deltaTime * positionLerpRate);
        transform.rotation = Quaternion.Slerp(transform.rotation, remoteRotation, Time.deltaTime * rotationLerpRate);
        rb.velocity = Vector2.Lerp(rb.velocity, remoteVelocity, Time.deltaTime * velocityLerpRate);
    }

    /// <summary>
    /// 플레이어 상태 업데이트
    /// </summary>
    private void UpdateRemotePlayerState()
    {
        facingRight = remoteFacingRight;
        isGrounded = remoteIsGrounded;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 데이터 전송 (로컬 플레이어) - 
            stream.SendNext(transform.position.x);
            stream.SendNext(transform.position.y);
            stream.SendNext(transform.position.z);
            stream.SendNext(transform.rotation.x);
            stream.SendNext(transform.rotation.y);
            stream.SendNext(transform.rotation.z);
            stream.SendNext(transform.rotation.w);
            stream.SendNext(rb.velocity.x);
            stream.SendNext(rb.velocity.y);
            stream.SendNext(facingRight ? 1f : 0f); 
            stream.SendNext(isGrounded ? 1f : 0f);
            stream.SendNext(canJump ? 1f : 0f);
            stream.SendNext(hasJumpedInAir ? 1f : 0f);
        }
        else
        {
            // 포지션
            float posX = (float)stream.ReceiveNext();
            float posY = (float)stream.ReceiveNext();
            float posZ = (float)stream.ReceiveNext();
            remotePosition = new Vector3(posX, posY, posZ);

            // 회전
            float rotX = (float)stream.ReceiveNext();
            float rotY = (float)stream.ReceiveNext();
            float rotZ = (float)stream.ReceiveNext();
            float rotW = (float)stream.ReceiveNext();
            remoteRotation = new Quaternion(rotX, rotY, rotZ, rotW);

            // 속도
            float velX = (float)stream.ReceiveNext();
            float velY = (float)stream.ReceiveNext();
            remoteVelocity = new Vector2(velX, velY);

            // 불상태들
            remoteFacingRight = (float)stream.ReceiveNext() > 0.5f;
            remoteIsGrounded = (float)stream.ReceiveNext() > 0.5f;
            canJump = (float)stream.ReceiveNext() > 0.5f;
            hasJumpedInAir = (float)stream.ReceiveNext() > 0.5f;

            lastReceivedTime = info.SentServerTime;
        }
    }

    #endregion

    #region RPCs

    [PunRPC]
    private void OnNormalJump(float jumpPower)
    {
        if (!photonView.IsMine)
        {
            // 플레이어 시각적 피드백
            PlayJumpEffect();
            return;
        }

        ResetVelocityForJump(preserveHorizontal: true);
        rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
        canJump = false;

        if (!isGrounded)
        {
            hasJumpedInAir = true;
        }
    }

    [PunRPC]
    private void OnForceJump(float jumpPower)
    {
        if (!photonView.IsMine)
        {
            // 플레이어 시각적 피드백
            PlayJumpEffect();
            return;
        }

        // 다른 시스템들 상태 리셋
        if (ropeSystem != null && ropeSystem.IsSwinging())
        {
            ropeSystem.ForceDetach();
        }

        if (wallSystem != null)
        {
            wallSystem.ResetWallState();
        }

        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
        canJump = false;
    }

    [PunRPC]
    private void OnSetPosition(Vector3 newPosition)
    {
        if (!photonView.IsMine)
        {
            // 위치 업데이트
            remotePosition = newPosition;
            return;
        }

        // 다른 시스템들 상태 리셋
        if (ropeSystem != null && ropeSystem.IsSwinging())
        {
            ropeSystem.ForceDetach();
        }

        if (wallSystem != null)
        {
            wallSystem.ResetWallState();
        }

        transform.position = newPosition;
        rb.velocity = Vector2.zero;
    }

    [PunRPC]
    private void OnSpriteFlip(bool newFacingRight)
    {
        // 모든 클라이언트에서 스프라이트 방향 동기화
        if (photonView.IsMine)
        {
            facingRight = newFacingRight;
            UpdateSpriteDirection();
        }
        else
        {
            remoteFacingRight = newFacingRight;
        }
    }

    [PunRPC]
    private void OnJumpStateChanged(bool newCanJump, bool newHasJumpedInAir)
    {
        // 점프 상태 시각적 동기화 (추후 추가할 이펙트, 애니메이션 등)
        if (!photonView.IsMine)
        {
            canJump = newCanJump;
            hasJumpedInAir = newHasJumpedInAir;
        }
    }

    /// <summary>
    /// 로컬 플레이어용 시각적 점프 이펙트
    /// </summary>
    private void PlayJumpEffect()
    {
        // 점프 파티클 이펙트, 사운드 이펙트, 애니메이션 등을 여기에 추가할 예정
    }

    #endregion

    #region 셋업

    private void SetupSystemReferences()
    {
        // 각 시스템에 메인 컨트롤러 참조 전달
        if (wallSystem != null)
        {
            wallSystem.Initialize(this, rb, col, groundLayerMask);
        }

        if (ropeSystem != null)
        {
            ropeSystem.Initialize(this, rb);
        }
    }

    private void SetupGroundCheck()
    {
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.parent = transform;
            groundCheckObj.transform.localPosition = new Vector3(0f, -col.bounds.extents.y, 0f);
            groundCheck = groundCheckObj.transform;
        }
    }

    private void InitializeStats()
    {
        if (playerData != null)
        {
            currentGroundSpeed = playerData.DefaultGroundSpeed;
            currentAirSpeed = playerData.DefaultAirSpeed;
        }
    }

    #endregion

    #region 입력 핸들링

    private void HandleInput()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        jumpInputDown = Input.GetKeyDown(KeyCode.Space);
        jumpInput = Input.GetKey(KeyCode.Space);
        jumpInputUp = Input.GetKeyUp(KeyCode.Space);
    }

    #endregion

    #region 땅체크

    private void UpdateGroundCheck()
    {
        bool wasGrounded = isGrounded;

        // 로프에 매달려 있을 때는 ground check를 조금 다르게 처리
        if (ropeSystem != null && ropeSystem.IsSwinging())
        {
            isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize * 0.8f, 0f, groundLayerMask);
        }
        else
        {
            isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayerMask);
        }

        // 땅에 착지했을 때 점프 상태 활성화
        if (isGrounded && !wasGrounded)
        {
            canJump = true;
            hasJumpedInAir = false;

            // 점프 상태 변경을 다른 클라이언트에 알림
            photonView.RPC("OnJumpStateChanged", RpcTarget.Others, canJump, hasJumpedInAir);
        }
    }

    #endregion

    #region 이동 관련 변수

    private void HandleMovement()
    {
        // 다른 시스템이 이동을 제어하고 있는지 확인
        if (ShouldSkipMovement())
        {
            return;
        }

        // 일반 이동 처리
        float targetSpeed = moveInput * currentGroundSpeed;
        float speedDiff = targetSpeed - rb.velocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDiff) * accelRate, velocityPower) * Mathf.Sign(speedDiff);

        // 공중에서는 이동력 감소
        if (!isGrounded)
        {
            movement *= currentAirSpeed;
        }

        rb.AddForce(movement * Vector2.right);

        // 스프라이트 방향 변경
        if (moveInput != 0)
        {
            HandleSpriteFlip();
        }
    }

    private bool ShouldSkipMovement()
    {
        // 로프에 매달려 있거나 벽과 상호작용 중일 때 이동 제한
        bool isRopeSwinging = ropeSystem != null && ropeSystem.IsSwinging();
        bool isWallInteracting = wallSystem != null && wallSystem.IsWallInteracting();

        return isRopeSwinging || isWallInteracting;
    }

    private void HandleSpriteFlip()
    {
        bool shouldFlip = false;

        if (moveInput > 0 && !facingRight)
        {
            shouldFlip = true;
        }
        else if (moveInput < 0 && facingRight)
        {
            shouldFlip = true;
        }

        if (shouldFlip)
        {
            FlipSprite();
        }
    }

    private void FlipSprite()
    {
        facingRight = !facingRight;

        // 네트워크로 방향 변경 동기화
        photonView.RPC("OnSpriteFlip", RpcTarget.All, facingRight);
    }

    /// <summary>
    /// 바라보는 방향에 따라 스프라이트 방향 업데이트
    /// </summary>
    private void UpdateSpriteDirection()
    {
        if (facingRight)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
    }

    #endregion

    #region 점프 관련 부분 

    private void HandleJump()
    {
        // 점프 입력 처리
        if (jumpInputDown && canJump)
        {
            // 각 시스템에서 점프를 처리하는지 확인
            bool jumpHandled = false;

            // 로프 시스템에서 점프 처리
            if (ropeSystem != null && ropeSystem.TryHandleJump())
            {
                jumpHandled = true;
            }
            // 벽 시스템에서 점프 처리
            else if (wallSystem != null && wallSystem.TryHandleJump())
            {
                jumpHandled = true;
            }

            // 아무도 처리하지 않았으면 일반 점프
            if (!jumpHandled)
            {
                ExecuteNormalJump();
            }
        }

        // 점프 컷
        if (jumpInputUp && rb.velocity.y > 0f && CanCutJump())
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
        }
    }

    private bool CanCutJump()
    {
        // 로프나 벽 상호작용 중이 아닐 때만 점프 컷 가능
        bool isRopeSwinging = ropeSystem != null && ropeSystem.IsSwinging();
        bool isWallInteracting = wallSystem != null && wallSystem.IsWallInteracting();

        return !isRopeSwinging && !isWallInteracting;
    }

    private void ExecuteNormalJump()
    {
        // 네트워크 RPC로 일반 점프 실행
        photonView.RPC("OnNormalJump", RpcTarget.All, jumpForce);
    }

    /// <summary>
    /// 점프를 위한 속도 초기화
    /// </summary>
    /// <param name="preserveHorizontal">수평 속도를 유지할지 여부</param>
    public void ResetVelocityForJump(bool preserveHorizontal = true)
    {
        if (preserveHorizontal)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    #endregion

    #region 퍼블릭 메서드

    /// <summary>
    /// 점프 상태를 설정할 때 사용
    /// </summary>
    public void SetCanJump(bool value)
    {
        bool prevCanJump = canJump;
        bool prevHasJumpedInAir = hasJumpedInAir;

        canJump = value;
        if (value)
        {
            hasJumpedInAir = false;
        }

        // 상태가 변경되었으면 네트워크 동기화
        if (prevCanJump != canJump || prevHasJumpedInAir != hasJumpedInAir)
        {
            photonView.RPC("OnJumpStateChanged", RpcTarget.Others, canJump, hasJumpedInAir);
        }
    }

    /// <summary>
    /// 현재 이동 입력 값
    /// </summary>
    public float GetMoveInput()
    {
        return moveInput;
    }

    /// <summary>
    /// 점프 입력 상태들
    /// </summary>
    public bool GetJumpInputDown() => jumpInputDown;

    public bool GetJumpInput() => jumpInput;
    public bool GetJumpInputUp() => jumpInputUp;

    /// <summary>
    /// 점프 힘 값
    /// </summary>
    public float GetJumpForce() => jumpForce;

    /// <summary>
    /// 현재 지상 속도 가져오기
    /// </summary>
    public float GetCurrentGroundSpeed()
    {
        return currentGroundSpeed;
    }

    /// <summary>
    /// 현재 공중 속도 가져오기
    /// </summary>
    public float GetCurrentAirSpeed()
    {
        return currentAirSpeed;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 이동 속도를 변경합니다 (상태 효과용)
    /// </summary>
    public void SetMoveSpeed(float newGroundSpeed, float newAirSpeed)
    {
        currentGroundSpeed = newGroundSpeed;
        currentAirSpeed = newAirSpeed;
    }

    /// <summary>
    /// 원래 속도로 되돌립니다
    /// </summary>
    public void ResetMoveSpeed()
    {
        if (playerData != null)
        {
            currentGroundSpeed = playerData.DefaultGroundSpeed;
            currentAirSpeed = playerData.DefaultAirSpeed;
        }
    }

    /// <summary>
    /// 강제로 점프시킵니다 ( 맵밖에 나갔을때 대비 ) 
    /// </summary>
    public void ForceJump(float customJumpForce = -1f)
    {
        if (!photonView.IsMine) return;

        float jumpPower = customJumpForce > 0 ? customJumpForce : jumpForce;
        photonView.RPC("OnForceJump", RpcTarget.All, jumpPower);
    }

    /// <summary>
    /// 플레이어를 특정 위치로 이동시킵니다 ( 라운드 승패시 그.. 이동할때 ) 
    /// </summary>
    public void SetPosition(Vector3 newPosition)
    {
        if (!photonView.IsMine) return;

        photonView.RPC("OnSetPosition", RpcTarget.All, newPosition);
    }

    /// <summary>
    /// 현재 플레이어가 땅에 있는지 확인
    /// </summary>
    public bool IsGrounded()
    {
        return isGrounded;
    }

    /// <summary>
    /// 현재 점프 가능 상태인지 확인
    /// </summary>
    public bool CanJump()
    {
        return canJump;
    }

    /// <summary>
    /// 점프 상태를 강제로 활성화
    /// </summary>
    public void EnableJumpState()
    {
        SetCanJump(true);
    }

    /// <summary>
    /// 점프 상태를 강제로 비활성화
    /// </summary>
    public void DisableJumpState()
    {
        SetCanJump(false);
    }

    /// <summary>
    /// 현재 플레이어가 점프 중인지 확인
    /// </summary>
    public bool IsJumping()
    {
        return !isGrounded && rb.velocity.y > 0.1f;
    }

    /// <summary>
    /// 현재 이동 방향 확인
    /// </summary>
    public float GetMoveDirection()
    {
        return moveInput;
    }

    /// <summary>
    /// 현재 속도 확인
    /// </summary>
    public Vector2 GetVelocity()
    {
        return rb.velocity;
    }

    /// <summary>
    /// 플레이어가 바라보는 방향 확인
    /// </summary>
    public bool IsFacingRight()
    {
        return facingRight;
    }
    
    #endregion
}