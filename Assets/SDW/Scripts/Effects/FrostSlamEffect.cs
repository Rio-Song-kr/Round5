using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 특정 환경이나 게임 내에서 얼음과 관련된 슬램 효과를 관리하고 구현하는 데 사용
/// </summary>
public class FrostSlamEffect : MonoBehaviour
{
    [Header("Expansion Settings")]
    // # 초당 확장 속도
    [SerializeField] private float _expansionSpeed = 10f;
    // # 확장 시 최대 반지름
    [SerializeField] private float _maxRadius = 3f;
    // # 확장 전 시작 반지름
    [SerializeField] private float _initialRadius = 0.1f;
    // # Circle의 포인트 수
    [SerializeField] private int _pointCount = 64;
    // # 감지할 대상 레이어
    [SerializeField] private LayerMask _targetLayer;
    // # 감지 거리
    [SerializeField] private float _detectOffset = 0.01f;
    // # sphere 충돌 범위
    [SerializeField] private float _circleRadius = 0.1f;

    [Header("Line Settings")]
    // # Line Renderer에 사용할 Material
    [SerializeField] private Material _lineMaterial;
    // # Line의 두께
    [SerializeField] private float _lineWidth = 0.02f;

    [Header("Straighten Settings")]
    // # 직선화 적용 기준 각도 (이 각도 이상 꺾이면 포인트 유지, 단위: 도)
    [SerializeField] private float _angleThresholdForStraighten = 120f;

    [Header("Fade Settings")]
    // # 사라지기 전 대기 시간
    [SerializeField] private float _fadeDelay = 1f;
    // # 사라지는 데 걸리는 시간
    [SerializeField] private float _fadeDuration = 2f;
    // # 멈췄다고 판단할 속도 임계값
    [SerializeField] private float _stopVelocityThreshold = 0.01f;

    // # Collider를 자유롭게 수정할 수 있도록 PolygonCollider 사용
    private PolygonCollider2D _polyCollider;
    // # Circle의 포인트
    private List<Vector2> _points;
    // # 이전 프레임의 포인트 위치 (속도 계산용)
    private List<Vector2> _lastFramePoints;
    // # 해당 Point가 움직이지 않고 고정되었는지 여부
    private List<bool> _isFixed;
    // # 현재 반지름
    private float _currentRadius;
    // # 라인 렌더러
    private LineRenderer _lineRenderer;
    // # 페이드 효과가 진행 중인지 여부
    private bool _isFading = false;

    /// <summary>
    /// 컴포넌트와 변수들을 초기화하고, 초기 원형을 생성
    /// </summary>
    private void Start()
    {
        _polyCollider = GetComponent<PolygonCollider2D>();
        _points = new List<Vector2>(_pointCount);
        _lastFramePoints = new List<Vector2>(_pointCount);
        _isFixed = new List<bool>(_pointCount);

        _currentRadius = _initialRadius;

        InitializeLineRenderer();

        for (int i = 0; i < _pointCount; i++)
        {
            float angle = i * 2f * Mathf.PI / _pointCount;
            var point = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _currentRadius;
            _points.Add(point);
            _lastFramePoints.Add(point);
            _isFixed.Add(false);
        }

        _polyCollider.SetPath(0, _points);
        UpdateLineRenderer();
    }

    /// <summary>
    /// 매 프레임마다 원을 확장하고, 충돌을 감지하며, 모든 움직임이 멈추면 사라지는 효과를 시작
    /// </summary>
    private void Update()
    {
        if (_isFading) return;

        bool expansionCompleted = _currentRadius >= _maxRadius;
        if (!expansionCompleted)
        {
            float progress = Mathf.Clamp01((_currentRadius - _initialRadius) / (_maxRadius - _initialRadius));
            float speedMultiplier = Mathf.Cos(progress * Mathf.PI * 0.5f);
            _currentRadius += _expansionSpeed * speedMultiplier * Time.deltaTime;
            if (_currentRadius >= _maxRadius)
            {
                _currentRadius = _maxRadius;
                expansionCompleted = true;
            }
        }

        for (int i = 0; i < _pointCount; i++)
        {
            if (_isFixed[i]) continue;

            var direction = new Vector2(Mathf.Cos(i * 2f * Mathf.PI / _pointCount), Mathf.Sin(i * 2f * Mathf.PI / _pointCount));

            if (expansionCompleted)
            {
                _points[i] = direction * _maxRadius;
                _isFixed[i] = true;
            }
            else
            {
                float targetDistance = _currentRadius;
                var targetPosition = (Vector2)transform.position + direction * (targetDistance + _detectOffset);
                var collider = Physics2D.OverlapCircle(targetPosition, _circleRadius, _targetLayer);

                if (collider != null)
                {
                    float minDistance = _initialRadius;
                    float maxDistance = targetDistance + _detectOffset;
                    float stopDistance = minDistance;

                    for (int j = 0; j < 5; j++)
                    {
                        float testDistance = (minDistance + maxDistance) * 0.5f;
                        var testPosition = (Vector2)transform.position + direction * testDistance;
                        if (Physics2D.OverlapCircle(testPosition, _circleRadius, _targetLayer) != null)
                        {
                            maxDistance = testDistance;
                        }
                        else
                        {
                            minDistance = testDistance;
                            stopDistance = testDistance;
                        }
                    }
                    stopDistance = Mathf.Max(stopDistance - _detectOffset, _initialRadius);
                    _points[i] = direction * stopDistance;
                    _isFixed[i] = true;
                }
                else
                {
                    _points[i] = direction * targetDistance;
                }
            }
        }

        ProcessFixedSegments();
        _polyCollider.SetPath(0, _points);
        UpdateLineRenderer();

        if (AreAllPointsBelowVelocityThreshold())
        {
            _isFading = true;
            StartCoroutine(FadeOutAndDestroy());
        }

        // # 다음 프레임의 속도 계산을 위해 현재 포인트 위치를 기록
        for (int i = 0; i < _pointCount; i++)
        {
            _lastFramePoints[i] = _points[i];
        }
    }

    /// <summary>
    /// LineRenderer 컴포넌트를 초기화하고 기본 설정을 적용
    /// </summary>
    private void InitializeLineRenderer()
    {
        _lineRenderer = gameObject.AddComponent<LineRenderer>();
        _lineRenderer.material = _lineMaterial;
        _lineRenderer.startWidth = _lineWidth;
        _lineRenderer.endWidth = _lineWidth;
        _lineRenderer.loop = true;
        _lineRenderer.useWorldSpace = false;
        _lineRenderer.startColor = Color.white;
        _lineRenderer.endColor = Color.white;
    }

    /// <summary>
    /// 모든 포인트의 속도가 설정된 임계값보다 낮은지 확인
    /// </summary>
    private bool AreAllPointsBelowVelocityThreshold()
    {
        if (Time.deltaTime <= 0) return false;

        for (int i = 0; i < _pointCount; i++)
        {
            float distance = Vector2.Distance(_points[i], _lastFramePoints[i]);
            float velocity = distance / Time.deltaTime;
            if (velocity > _stopVelocityThreshold)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 고정된 세그먼트를 찾아 직선으로 보간하여 시각적으로 부드럽게 만듬
    /// </summary>
    private void ProcessFixedSegments()
    {
        int currentFixedSegmentStart = -1;
        bool inFixedSegment = false;
        var interpolationRanges = new List<Tuple<int, int>>();

        for (int i = 0; i < _pointCount * 2; i++)
        {
            int currentIndex = i % _pointCount;
            int prevIndex = (currentIndex - 1 + _pointCount) % _pointCount;

            if (_isFixed[currentIndex])
            {
                if (!inFixedSegment)
                {
                    currentFixedSegmentStart = currentIndex;
                    inFixedSegment = true;
                }

                if (inFixedSegment && currentIndex != currentFixedSegmentStart)
                {
                    var vec1 = _points[currentIndex] - _points[prevIndex];
                    int nextPotentialIndex = (currentIndex + 1) % _pointCount;
                    var vec2 = Vector2.zero;

                    if (_isFixed[nextPotentialIndex])
                        vec2 = _points[nextPotentialIndex] - _points[currentIndex];

                    if (vec1.sqrMagnitude > 0.0001f && vec2.sqrMagnitude > 0.0001f)
                    {
                        float angle = Vector2.Angle(vec1, vec2);
                        if (angle < 180f - _angleThresholdForStraighten)
                        {
                            interpolationRanges.Add(new Tuple<int, int>(currentFixedSegmentStart, prevIndex));
                            currentFixedSegmentStart = currentIndex;
                        }
                    }
                }
            }
            else
            {
                if (inFixedSegment)
                {
                    interpolationRanges.Add(new Tuple<int, int>(currentFixedSegmentStart, prevIndex));
                    inFixedSegment = false;
                    currentFixedSegmentStart = -1;
                }
            }
        }

        if (inFixedSegment && currentFixedSegmentStart != -1)
        {
            interpolationRanges.Add(new Tuple<int, int>(currentFixedSegmentStart, _pointCount - 1));
        }

        foreach (var range in interpolationRanges)
        {
            ApplyLinearInterpolation(range.Item1, range.Item2);
        }
    }

    /// <summary>
    /// 주어진 시작점과 끝점 사이의 포인트들을 직선으로 보간
    /// </summary>
    private void ApplyLinearInterpolation(int segStartIdx, int segEndIdx)
    {
        if (segStartIdx == segEndIdx) return;

        var startPoint = _points[segStartIdx];
        var endPoint = _points[segEndIdx];
        int count = segEndIdx >= segStartIdx ? segEndIdx - segStartIdx + 1 : _pointCount - segStartIdx + segEndIdx + 1;

        for (int i = 0; i < count; i++)
        {
            int pointIndex = (segStartIdx + i) % _pointCount;
            float t = (float)i / (count - 1);
            _points[pointIndex] = Vector2.Lerp(startPoint, endPoint, t);
        }
    }

    /// <summary>
    /// LineRenderer의 포인트 위치를 현재 포인트 리스트에 맞춰 업데이트
    /// </summary>
    private void UpdateLineRenderer()
    {
        var linePoints = new Vector3[_points.Count];
        for (int i = 0; i < _points.Count; i++)
        {
            linePoints[i] = new Vector3(_points[i].x, _points[i].y, 0);
        }
        _lineRenderer.positionCount = linePoints.Length;
        _lineRenderer.SetPositions(linePoints);
    }

    /// <summary>
    /// 설정된 시간 동안 대기한 후, LineRenderer와 Collider를 서서히 사라지게 하고 최종적으로 게임 오브젝트를 파괴
    /// </summary>
    private IEnumerator FadeOutAndDestroy()
    {
        yield return new WaitForSeconds(_fadeDelay);

        float elapsedTime = 0f;
        Color startColor = _lineRenderer.startColor;

        while (elapsedTime < _fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsedTime / _fadeDuration);
            Color newColor = new Color(startColor.r, startColor.g, startColor.b, alpha);
            _lineRenderer.startColor = newColor;
            _lineRenderer.endColor = newColor;
            yield return null;
        }

        if (_polyCollider != null) _polyCollider.enabled = false;
        if (_lineRenderer != null) _lineRenderer.enabled = false;

        Destroy(gameObject);
    }
}
