using System.Collections;
using UnityEngine;
using Photon.Pun;

public class RopeSwingSystem : MonoBehaviourPun, IPunObservable
{
    [Header("후크 세팅")] [SerializeField]
    private GameObject hookCrosshair;

    [SerializeField] private LineRenderer hookLineRenderer;
    [SerializeField] private GameObject hookHitEffect;
    [SerializeField] private GameObject hookObject;
    [SerializeField] private LayerMask hookableLayerMask = -1;
    [SerializeField] private float maxHookDistance = 15f;

    [Header("로프 물리")] [SerializeField]
    private float ropeMaxLength = 10f;

    [SerializeField] private float ropeSpringForce = 150f;
    [SerializeField] private float ropeDampening = 7f;
    [SerializeField] private float ropeClimbSpeed = 5f;
    [SerializeField] private float swingForce = 300f;

    [Header("입력 세팅")] [SerializeField]
    private KeyCode hookKey = KeyCode.E;

    [SerializeField] private KeyCode ropeClimbUpKey = KeyCode.W;
    [SerializeField] private KeyCode ropeClimbDownKey = KeyCode.S;

    
    private Rigidbody2D rb;
    private LineRenderer trajectoryRenderer;

    
    private RaycastHit2D[] hits = new RaycastHit2D[10];
    private GameObject hookInstance;
    private Vector2 hookPoint;
    private bool isHookAttached;
    private bool isSwinging;
    private float currentRopeLength;

    // 로프 물리
    private SpringJoint2D ropeJoint;
    private Vector2 ropeDirection;
    private float ropeDistance;

   // 입력관련
    private bool hookInput;
    private bool hookInputDown;
    private bool hookInputUp;
    private bool fireHookInput;
    private bool fireHookInputUp;
    private bool climbUpInput;
    private bool climbDownInput;
    private float moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        SetupComponents();
        InitializeHook();
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            HandleInput();
            UpdateHookTrajectory();
            HandleHookInput();
            HandleRopeClimbing();
        }

        // 모든 플레이어에게 시각적 업데이트 적용
        UpdateRopeVisuals();
    }

    private void FixedUpdate()
    {
        if (photonView.IsMine && isSwinging)
        {
            HandleSwingPhysics();
        }
    }


    #region RPCs

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 데이터 전송 (로컬 플레이어)
            stream.SendNext(isHookAttached);
            stream.SendNext(hookPoint);
            stream.SendNext(currentRopeLength);
            stream.SendNext(isSwinging);
        }
        else
        {
            // 데이터 수신 (리모트 플레이어)
            bool networkIsHookAttached = (bool)stream.ReceiveNext();
            Vector2 networkHookPoint = (Vector2)stream.ReceiveNext();
            float networkRopeLength = (float)stream.ReceiveNext();
            bool networkIsSwinging = (bool)stream.ReceiveNext();


            if (networkIsHookAttached != isHookAttached)
            {
                isHookAttached = networkIsHookAttached;
                isSwinging = networkIsSwinging;
                hookPoint = networkHookPoint;
                currentRopeLength = networkRopeLength;

                if (isHookAttached)
                {
                    ShowHookAttached(hookPoint);
                }
                else
                {
                    ShowHookDetached();
                }
            }
            else if (isHookAttached)
            {
                hookPoint = Vector2.Lerp(hookPoint, networkHookPoint, Time.deltaTime * 10f);
                currentRopeLength = Mathf.Lerp(currentRopeLength, networkRopeLength, Time.deltaTime * 10f);
            }
        }
    }

    [PunRPC]
    private void OnHookAttached(Vector2 hitPoint)
    {
        hookPoint = hitPoint;
        isHookAttached = true;
        isSwinging = true;
        currentRopeLength = Vector2.Distance(transform.position, hookPoint);

        // 물리 조인트는 오직 소유자만 생성
        if (photonView.IsMine)
        {
            CreateRopeJoint();
        }

        // 시각적 요소는 모든 클라이언트에서 표시
        ShowHookAttached(hitPoint);
    }

    [PunRPC]
    private void OnHookDetached()
    {
        isHookAttached = false;
        isSwinging = false;

        // 물리 조인트는 오직 소유자만 제거
        if (photonView.IsMine)
        {
            DestroyRopeJoint();
        }

        // 시각적 요소는 모든 클라이언트에서 숨김
        ShowHookDetached();
    }

    [PunRPC]
    private void OnHookEffect(Vector2 effectPosition)
    {
        // 이펙트 생성
        if (hookHitEffect != null)
        {
            GameObject effect = Instantiate(hookHitEffect, effectPosition, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }

    #endregion

    #region 셋업

    private void SetupComponents()
    {
        // 궤적용 LineRenderer 설정 (로컬 플레이어만)
        if (photonView.IsMine && trajectoryRenderer == null)
        {
            GameObject trajectoryObj = new GameObject("TrajectoryRenderer");
            trajectoryObj.transform.parent = transform;
            trajectoryRenderer = trajectoryObj.AddComponent<LineRenderer>();

            trajectoryRenderer.material = hookLineRenderer.material;
            trajectoryRenderer.startWidth = 0.02f;
            trajectoryRenderer.endWidth = 0.02f;
            trajectoryRenderer.positionCount = 2;
            trajectoryRenderer.enabled = true;
        }

        if (hookLineRenderer)
        {
            hookLineRenderer.positionCount = 2;
            hookLineRenderer.enabled = false;
        }
    }

    private void InitializeHook()
    {
        if (hookObject != null)
        {
            hookInstance = Instantiate(hookObject, transform.position, Quaternion.identity);
            hookInstance.SetActive(false);
        }

        // 크로스헤어는 로컬 플레이어만
        if (hookCrosshair != null && !photonView.IsMine)
        {
            hookCrosshair.SetActive(false);
        }

        // 초기 궤적 설정 (로컬 플레이어만)
        if (trajectoryRenderer != null)
        {
            trajectoryRenderer.SetPosition(0, transform.position);
            trajectoryRenderer.SetPosition(1, transform.position);
        }
    }

    #endregion

    #region 입력 핸들링

    private void HandleInput()
    {
        hookInputDown = Input.GetKeyDown(hookKey);
        hookInput = Input.GetKey(hookKey);
        hookInputUp = Input.GetKeyUp(hookKey);
        fireHookInput = Input.GetMouseButtonDown(0);
        fireHookInputUp = Input.GetMouseButtonUp(0);

        climbUpInput = Input.GetKey(ropeClimbUpKey);
        climbDownInput = Input.GetKey(ropeClimbDownKey);

        moveInput = Input.GetAxisRaw("Horizontal");
    }

    #endregion

    #region 후크 에임관련 메서드

    private void UpdateHookTrajectory()
    {
        if (trajectoryRenderer == null) return;

        if (!isHookAttached && hookInput)
        {
            Vector2 aimDirection = GetAimDirection();
            Vector2 startPos = transform.position;

            int hitCount = Physics2D.RaycastNonAlloc(startPos, aimDirection, hits, maxHookDistance, hookableLayerMask);

            if (hitCount > 0)
            {
                Vector2 hitPoint = hits[0].point;
                float distance = Vector2.Distance(startPos, hitPoint);

                if (distance <= ropeMaxLength)
                {
                    trajectoryRenderer.SetPosition(0, startPos);
                    trajectoryRenderer.SetPosition(1, hitPoint);

                    if (hookCrosshair != null)
                    {
                        hookCrosshair.transform.position = hitPoint;
                        hookCrosshair.SetActive(true);
                    }
                }
                else
                {
                    Vector2 maxPoint = startPos + aimDirection * ropeMaxLength;
                    trajectoryRenderer.SetPosition(0, startPos);
                    trajectoryRenderer.SetPosition(1, maxPoint);

                    if (hookCrosshair != null)
                    {
                        hookCrosshair.SetActive(false);
                    }
                }
            }
            else
            {
                Vector2 endPoint = startPos + aimDirection * maxHookDistance;
                trajectoryRenderer.SetPosition(0, startPos);
                trajectoryRenderer.SetPosition(1, endPoint);

                if (hookCrosshair != null)
                {
                    hookCrosshair.SetActive(false);
                }
            }
        }
        else
        {
            trajectoryRenderer.SetPosition(0, transform.position);
            trajectoryRenderer.SetPosition(1, transform.position);

            if (hookCrosshair != null)
            {
                hookCrosshair.SetActive(false);
            }
        }
    }

    private Vector2 GetAimDirection()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        return (mousePos - transform.position).normalized;
    }

    #endregion

    #region 후크 시스템

    private void HandleHookInput()
    {
        // E키 + 좌클릭 다운으로 후크 발사
        if (fireHookInput && hookInput && !isHookAttached)
        {
            TryAttachHook();
        }

        // 좌클릭 떼면 후크 해제
        if (fireHookInputUp && isHookAttached)
        {
            DetachHook();
        }
    }

    private void TryAttachHook()
    {
        Vector2 aimDirection = GetAimDirection();
        Vector2 startPos = transform.position;

        int hitCount = Physics2D.RaycastNonAlloc(startPos, aimDirection, hits, maxHookDistance, hookableLayerMask);

        if (hitCount > 0)
        {
            Vector2 hitPoint = hits[0].point;
            float distance = Vector2.Distance(startPos, hitPoint);

            if (distance <= ropeMaxLength)
            {
                AttachHook(hitPoint);
            }
        }
    }

    private void AttachHook(Vector2 hitPoint)
    {
        photonView.RPC("OnHookAttached", RpcTarget.All, hitPoint);
        photonView.RPC("OnHookEffect", RpcTarget.All, hitPoint);
    }

    private void DetachHook()
    {
        photonView.RPC("OnHookDetached", RpcTarget.All);
    }

    private void ShowHookAttached(Vector2 hitPoint)
    {
        if (hookInstance != null)
        {
            hookInstance.transform.position = hitPoint;
            hookInstance.SetActive(true);

            Vector2 hookDirection = (hitPoint - (Vector2)transform.position).normalized;
            float angle = Mathf.Atan2(hookDirection.y, hookDirection.x) * Mathf.Rad2Deg;
            hookInstance.transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        }

        if (hookLineRenderer != null)
        {
            hookLineRenderer.enabled = true;
        }
    }

    private void ShowHookDetached()
    {
        if (hookInstance != null)
        {
            hookInstance.SetActive(false);
        }

        if (hookLineRenderer != null)
        {
            hookLineRenderer.enabled = false;
        }
    }

    #endregion

    #region 로프 물리 메서드

    /// <summary>
    /// 특히 다른 오브젝트 뚫리는 현상 보완.
    /// </summary>
    private void CreateRopeJoint()
    {
        if (ropeJoint != null)
        {
            DestroyRopeJoint();
        }

        ropeJoint = gameObject.AddComponent<SpringJoint2D>();
        ropeJoint.autoConfigureConnectedAnchor = false;
        ropeJoint.connectedAnchor = hookPoint;
        ropeJoint.distance = currentRopeLength;
        ropeJoint.frequency = ropeSpringForce;
        ropeJoint.dampingRatio = ropeDampening;
        ropeJoint.enableCollision = true;
    }

    private void DestroyRopeJoint()
    {
        if (ropeJoint != null)
        {
            Destroy(ropeJoint);
            ropeJoint = null;
        }
    }

    private void HandleSwingPhysics()
    {
        if (!isHookAttached || ropeJoint == null) return;

        ropeDirection = (hookPoint - (Vector2)transform.position).normalized;
        ropeDistance = Vector2.Distance(transform.position, hookPoint);

        if (moveInput != 0)
        {
            Vector2 swingDirection = new Vector2(ropeDirection.y, -ropeDirection.x) * moveInput;
            rb.AddForce(swingDirection * swingForce);
        }

        if (ropeDistance > currentRopeLength + 0.5f)
        {
            Vector2 constrainedPosition = hookPoint - ropeDirection * currentRopeLength;
            transform.position = constrainedPosition;
        }
    }

    #endregion

    #region 로프 클라이밍 부분 메서드

    private void HandleRopeClimbing()
    {
        if (!isHookAttached || ropeJoint == null) return;

        float climbInput = 0f;

        if (climbUpInput) climbInput = 1f;
        else if (climbDownInput) climbInput = -1f;

        if (climbInput != 0)
        {
            float newLength = currentRopeLength - climbInput * ropeClimbSpeed * Time.deltaTime;
            newLength = Mathf.Clamp(newLength, 1f, ropeMaxLength);

            if (climbInput > 0 && newLength < currentRopeLength)
            {
                if (CanClimbUp(newLength))
                {
                    ApplyRopeClimbing(newLength, climbInput);
                }
                else
                {
                    return;
                }
            }
            else if (climbInput < 0)
            {
                ApplyRopeClimbing(newLength, climbInput);
            }
        }
    }

    private bool CanClimbUp(float targetRopeLength)
    {
        Vector2 currentPos = transform.position;
        Vector2 targetDirection = (hookPoint - currentPos).normalized;
        Vector2 targetPos = hookPoint - targetDirection * targetRopeLength;

        float checkDistance = Vector2.Distance(currentPos, targetPos);
        RaycastHit2D pathHit = Physics2D.CircleCast(
            currentPos,
            GetPlayerRadius(),
            targetDirection,
            checkDistance,
            hookableLayerMask
        );

        if (pathHit.collider != null)
        {
            return false;
        }

        Collider2D overlapHit = Physics2D.OverlapCircle(
            targetPos,
            GetPlayerRadius() * 1.1f,
            hookableLayerMask
        );

        if (overlapHit != null)
        {
            return false;
        }

        return true;
    }

    private float GetPlayerRadius()
    {
        CircleCollider2D circleCol = GetComponent<CircleCollider2D>();
        if (circleCol != null)
        {
            return circleCol.radius * transform.localScale.x;
        }

        return 0.5f;
    }

    private void ApplyRopeClimbing(float newLength, float climbInput)
    {
        if (newLength != currentRopeLength)
        {
            currentRopeLength = newLength;
            ropeJoint.distance = currentRopeLength;

            if (climbInput > 0)
            {
                Vector2 pullDirection = ropeDirection;
                rb.AddForce(pullDirection * ropeClimbSpeed * 50f * Time.deltaTime);
            }
        }
    }

    #endregion


    private void UpdateRopeVisuals()
    {
        if (isHookAttached && hookLineRenderer != null)
        {
            hookLineRenderer.SetPosition(0, transform.position);
            hookLineRenderer.SetPosition(1, hookPoint);
        }
    }


    #region 퍼블릭 메서드들

    public bool IsSwinging() => isSwinging;
    public bool IsHookAttached() => isHookAttached;

    public void ForceDetachHook()
    {
        if (isHookAttached && photonView.IsMine)
        {
            DetachHook();
        }
    }

    public void SetRopeLength(float newLength)
    {
        ropeMaxLength = Mathf.Max(newLength, 1f);
    }

    public void SetSwingForce(float newForce)
    {
        swingForce = Mathf.Max(newForce, 0f);
    }

    public void SetRopeClimbSpeed(float newSpeed)
    {
        ropeClimbSpeed = Mathf.Max(newSpeed, 0f);
    }

    public float GetCurrentRopeLength() => currentRopeLength;
    public Vector2 GetHookPoint() => hookPoint;

    #endregion
}