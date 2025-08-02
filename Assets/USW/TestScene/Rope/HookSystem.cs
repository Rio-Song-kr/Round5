
using UnityEngine;
using UnityEngine.VFX;
using System.Collections.Generic;

public class HookSystem : MonoBehaviour
{
    [Header("VFX Settings")]
    [SerializeField] private VisualEffect _hookEffect;
    
    [Header("Hook Settings")]
    public float hookSpeed = 150f;
    public float maxLength = 30f;
    public float ropeClimbSpeed = 1f;
    public LayerMask hookableLayer = -1;
    
    [Header("Rope Physics")]
    public float linecastOffset = 0.01f;
    public float returnLCDist = 0.2f;
    
    // 상태 변수들
    private bool _isHooked = false;
    private bool _isHookActive = false;
    private Vector2 _hookPoint;
    private List<Vector2> _anchors = new List<Vector2>();
    private float _combinedAnchorLen = 0f;
    
    // 컴포넌트 참조
    private SpringJoint2D _spring;
    private RaycastHit2D[] _hits = new RaycastHit2D[10];
    
    // 애니메이션용
    private float _currentHookDistance = 0f;
    private Vector2 _hookDirection;
    private bool _isHookExtending = false;

    private void Awake()
    {
        _spring = GetComponent<SpringJoint2D>();
        if (_spring == null)
        {
            _spring = gameObject.AddComponent<SpringJoint2D>();
        }
        _spring.enabled = false;
        
        if (_hookEffect != null)
        {
            _hookEffect.enabled = false;
        }
    }

    private void Update()
    {
        HandleInput();
        UpdateAiming();
        
        if (_isHookExtending)
        {
            UpdateHookExtension();
        }
        else if (_isHooked)
        {
            HandleRopeClimbing();
        }
    }

    private void FixedUpdate()
    {
        if (_isHooked && _anchors.Count > 0)
        {
            UpdateRopePhysics();
        }
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (_isHookActive)
            {
                DestroyHook();
            }
            else
            {
                FireHook();
            }
        }
        
        if (Input.GetButtonDown("Jump") && _isHookActive)
        {
            DestroyHook();
        }
    }

    private void UpdateAiming()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        
        if (Vector3.Distance(mousePos, transform.position) > 0.5f)
        {
            _hookDirection = (mousePos - transform.position).normalized;
            transform.up = _hookDirection;
        }
    }

    private void FireHook()
    {
        if (_hookEffect == null) return;

        _hookDirection = transform.up;
        _isHookActive = true;
        _isHookExtending = true;
        _currentHookDistance = 0f;
        _anchors.Clear();
        _combinedAnchorLen = 0f;
        
        _hookEffect.enabled = true;
        _hookEffect.SetVector3("StartPos", transform.position);
        
        // 최대 거리나 충돌 지점까지의 거리 계산
        Vector2 targetPoint = CalculateHookTarget();
        _hookPoint = targetPoint;
        
        StartCoroutine(AnimateHookExtension());
    }

    private Vector2 CalculateHookTarget()
    {
        Vector2 startPos = transform.position;
        Vector2 direction = _hookDirection;
        
        if (Physics2D.RaycastNonAlloc(startPos, direction, _hits, maxLength, hookableLayer) > 0)
        {
            // 충돌 지점에서 약간 오프셋
            Vector2 hitPoint = _hits[0].point + (_hits[0].normal.normalized * linecastOffset);
            return hitPoint;
        }
        else
        {
            // 최대 거리까지
            return startPos + direction * maxLength;
        }
    }

    private System.Collections.IEnumerator AnimateHookExtension()
    {
        Vector2 startPos = transform.position;
        float targetDistance = Vector2.Distance(startPos, _hookPoint);
        float animationTime = targetDistance / hookSpeed;
        float elapsedTime = 0f;

        while (elapsedTime < animationTime && _isHookExtending)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / animationTime;
            
            Vector2 currentHookPos = Vector2.Lerp(startPos, _hookPoint, progress);
            _hookEffect.SetVector3("EndPos", currentHookPos);
            
            // 중간에 충돌 체크
            if (Physics2D.RaycastNonAlloc(startPos, currentHookPos - startPos, _hits, 
                Vector2.Distance(startPos, currentHookPos), hookableLayer) > 0)
            {
                // 충돌 발생 - 갈고리 고정
                _hookPoint = _hits[0].point + (_hits[0].normal.normalized * linecastOffset);
                _hookEffect.SetVector3("EndPos", _hookPoint);
                ProcessHookHit();
                yield break;
            }
            
            yield return null;
        }

        // 애니메이션 완료 - 갈고리가 목표에 도달했는지 확인
        if (_isHookExtending)
        {
            if (Physics2D.OverlapPoint(_hookPoint, hookableLayer))
            {
                ProcessHookHit();
            }
            else
            {
                // 갈고리가 아무것도 잡지 못함 - 제거
                DestroyHook();
            }
        }
    }

    private void ProcessHookHit()
    {
        _isHookExtending = false;
        _isHooked = true;
        
        // 첫 번째 앵커 추가
        AddAnchor(_hookPoint);
        
        // VFX 업데이트
        UpdateVFXRope();
    }

    private void UpdateHookExtension()
    {
        // 갈고리 확장 중에는 거리 체크
        if (Vector2.Distance(transform.position, _hookPoint) > maxLength)
        {
            DestroyHook();
        }
    }

    private void HandleRopeClimbing()
    {
        if (_spring.enabled)
        {
            float allowedDistance = maxLength - _combinedAnchorLen;
            float newDistance = _spring.distance + Input.GetAxis("Vertical") * -1 * ropeClimbSpeed * Time.deltaTime;
            _spring.distance = Mathf.Clamp(newDistance, 1f, allowedDistance);
        }
    }

    private void UpdateRopePhysics()
    {
        // 플레이어와 가장 가까운 앵커 사이의 충돌 체크
        if (_anchors.Count > 0)
        {
            RaycastHit2D hit = Physics2D.Linecast(transform.position, _anchors[_anchors.Count - 1], hookableLayer);

            if (hit && hit.collider.gameObject.CompareTag("Hookable"))
            {
                Vector2 newAnchor = hit.point + (hit.normal.normalized * linecastOffset);
                AddAnchor(newAnchor);
            }
        }

        // 앵커 제거 체크 (로프가 장애물을 통과할 수 있게 됐을 때)
        if (_anchors.Count > 1)
        {
            Vector2 directionToPlayer = (transform.position - (Vector3)_anchors[_anchors.Count - 1]).normalized;
            Vector2 checkStart = _anchors[_anchors.Count - 1] + directionToPlayer * returnLCDist;
            
            RaycastHit2D clearCheck = Physics2D.Linecast(checkStart, _anchors[_anchors.Count - 2], hookableLayer);
            
            if (!clearCheck)
            {
                RemoveLastAnchor();
            }
        }

        UpdateVFXRope();
    }

    private void AddAnchor(Vector2 pos)
    {
        _anchors.Add(pos);
        
        if (_anchors.Count > 1)
        {
            float distance = Vector2.Distance(_anchors[_anchors.Count - 1], _anchors[_anchors.Count - 2]);
            _combinedAnchorLen += distance;
            _combinedAnchorLen = Mathf.Round(_combinedAnchorLen * 100f) / 100f;
        }
        
        UpdateSpringJoint();
    }

    private void RemoveLastAnchor()
    {
        if (_anchors.Count > 1)
        {
            float distance = Vector2.Distance(_anchors[_anchors.Count - 1], _anchors[_anchors.Count - 2]);
            _combinedAnchorLen -= distance;
            _combinedAnchorLen = Mathf.Round(_combinedAnchorLen * 100f) / 100f;
        }
        
        _anchors.RemoveAt(_anchors.Count - 1);
        UpdateSpringJoint();
    }

    private void UpdateSpringJoint()
    {
        if (_anchors.Count > 0)
        {
            Vector2 connectionPoint = _anchors[_anchors.Count - 1];
            float distance = Vector2.Distance(transform.position, connectionPoint);
            
            _spring.connectedAnchor = connectionPoint;
            _spring.distance = distance;
            _spring.enabled = true;
            _spring.enableCollision = true;
        }
    }

    private void UpdateVFXRope()
    {
        if (_hookEffect == null) return;

        // VFX에 로프 포인트들 전달
        List<Vector3> ropePoints = new List<Vector3>();
        
        // 갈고리 포인트부터 시작
        if (_anchors.Count > 0)
        {
            ropePoints.Add(_anchors[0]); // 갈고리 포인트
            
            // 중간 앵커들 (역순)
            for (int i = _anchors.Count - 1; i >= 1; i--)
            {
                ropePoints.Add(_anchors[i]);
            }
        }
        
        // 플레이어 포지션
        ropePoints.Add(transform.position);

        // VFX 업데이트
        _hookEffect.SetInt("PointCount", ropePoints.Count);
        for (int i = 0; i < ropePoints.Count && i < 20; i++)
        {
            _hookEffect.SetVector3($"Point{i}", ropePoints[i]);
        }

        // 로프 텐션 계산
        float totalLength = 0f;
        for (int i = 0; i < ropePoints.Count - 1; i++)
        {
            totalLength += Vector3.Distance(ropePoints[i], ropePoints[i + 1]);
        }
        _hookEffect.SetFloat("RopeLength", totalLength);
        _hookEffect.SetFloat("Tension", Mathf.Clamp01(totalLength / maxLength));
    }

    public void DestroyHook()
    {
        _isHookActive = false;
        _isHooked = false;
        _isHookExtending = false;
        
        if (_hookEffect != null)
        {
            _hookEffect.enabled = false;
        }
        
        if (_spring != null)
        {
            _spring.enabled = false;
        }
        
        _anchors.Clear();
        _combinedAnchorLen = 0f;
        
        StopAllCoroutines();
    }

    // 외부에서 상태 확인용
    public bool IsHooked => _isHooked;
    public bool IsHookActive => _isHookActive;
}