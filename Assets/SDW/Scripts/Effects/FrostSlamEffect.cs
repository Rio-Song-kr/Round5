using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 특정 환경이나 게임 내에서 얼음과 관련된 슬램 효과를 관리하고 구현하는 데 사용
/// </summary>
public class FrostSlamEffect : MonoBehaviour
{
    //# Frost Slam Skill Data
    private FrostSlamSkillDataSO _skillData;
    //# Collider를 자유롭게 수정할 수 있도록 PolygonCollider 사용
    private PolygonCollider2D _polyCollider;
    //# Circle의 포인트
    private List<Vector2> _points;
    //# 이전 프레임의 포인트 위치 (속도 계산용)
    private List<Vector2> _lastFramePoints;
    //# 해당 Point가 움직이지 않고 고정되었는지 여부
    private List<bool> _isFixed;
    //# 현재 반지름
    private float _currentRadius;
    //# 라인 렌더러
    private LineRenderer _lineRenderer;
    //# 페이드 효과가 진행 중인지 여부
    private bool _isFading;
    private bool _isInitialized;

    /// <summary>
    /// 컴포넌트 연결
    /// </summary>
    private void Awake()
    {
        _lineRenderer = gameObject.AddComponent<LineRenderer>();
        _polyCollider = GetComponent<PolygonCollider2D>();
    }

    /// <summary>
    /// 비활성화 시 초기화
    /// </summary>
    private void OnDisable() => _isInitialized = false;

    /// <summary>
    /// 매 프레임마다 원을 확장하고, 충돌을 감지하며, 모든 움직임이 멈추면 사라지는 효과를 시작
    /// </summary>
    private void Update()
    {
        if (!_isInitialized || _isFading) return;

        bool expansionCompleted = _currentRadius >= _skillData.MaxRadius;
        if (!expansionCompleted)
        {
            float progress = Mathf.Clamp01((_currentRadius - _skillData.InitialRadius) /
                                           (_skillData.MaxRadius - _skillData.InitialRadius));
            float speedMultiplier = Mathf.Cos(progress * Mathf.PI * 0.5f);
            _currentRadius += _skillData.ExpansionSpeed * speedMultiplier * Time.deltaTime;
            if (_currentRadius >= _skillData.MaxRadius)
            {
                _currentRadius = _skillData.MaxRadius;
                expansionCompleted = true;
            }
        }

        for (int i = 0; i < _skillData.PointCount; i++)
        {
            if (_isFixed[i]) continue;

            var direction = new Vector2(Mathf.Cos(i * 2f * Mathf.PI / _skillData.PointCount),
                Mathf.Sin(i * 2f * Mathf.PI / _skillData.PointCount));

            if (expansionCompleted)
            {
                _points[i] = direction * _skillData.MaxRadius;
                _isFixed[i] = true;
            }
            else
            {
                float targetDistance = _currentRadius;
                var targetPosition = (Vector2)transform.position + direction * (targetDistance + _skillData.DetectOffset);
                var collider = Physics2D.OverlapCircle(targetPosition, _skillData.CircleRadius, _skillData.TargetLayer);

                if (collider != null)
                {
                    float minDistance = _skillData.InitialRadius;
                    float maxDistance = targetDistance + _skillData.DetectOffset;
                    float stopDistance = minDistance;

                    for (int j = 0; j < 5; j++)
                    {
                        float testDistance = (minDistance + maxDistance) * 0.5f;
                        var testPosition = (Vector2)transform.position + direction * testDistance;
                        if (Physics2D.OverlapCircle(testPosition, _skillData.CircleRadius, _skillData.TargetLayer) != null)
                        {
                            maxDistance = testDistance;
                        }
                        else
                        {
                            minDistance = testDistance;
                            stopDistance = testDistance;
                        }
                    }
                    stopDistance = Mathf.Max(stopDistance - _skillData.DetectOffset, _skillData.InitialRadius);
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
            StartCoroutine(FadeOutAndInactive());
        }

        //# 다음 프레임의 속도 계산을 위해 현재 포인트 위치를 기록
        for (int i = 0; i < _skillData.PointCount; i++)
        {
            _lastFramePoints[i] = _points[i];
        }
    }

    /// <summary>
    /// FrostSlamEffect 컴포넌트 초기화 과정에서 필요한 데이터와 컴포넌트 설정을 수행
    /// </summary>
    /// <param name="skillData">슬롯에 로드된 FrostSlamSkillDataSO 인스턴스</param>
    public void Initialize(FrostSlamSkillDataSO skillData)
    {
        _skillData = skillData;

        _points = new List<Vector2>(_skillData.PointCount);
        _lastFramePoints = new List<Vector2>(_skillData.PointCount);
        _isFixed = new List<bool>(_skillData.PointCount);

        Activate();
        _isInitialized = true;
    }

    /// <summary>
    /// 초기 활성화 및 설정 프로세싱을 수행
    /// 스킬 데이터를 기반으로 효과의 초기 상태를 구성하고 시각적 요소들을 초기화
    /// </summary>
    private void Activate()
    {
        _currentRadius = _skillData.InitialRadius;
        _points.Clear();
        _lastFramePoints.Clear();
        _isFixed.Clear();

        InitializeLineRenderer();

        for (int i = 0; i < _skillData.PointCount; i++)
        {
            float angle = i * 2f * Mathf.PI / _skillData.PointCount;
            var point = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _currentRadius;
            _points.Add(point);
            _lastFramePoints.Add(point);
            _isFixed.Add(false);
        }

        _polyCollider.SetPath(0, _points);
        UpdateLineRenderer();

        _isFading = false;


        // var smokeEffectObject = _skillData.Pools.Instantiate("VFX_Smoke", _skillData.SkillPoisition, Quaternion.identity);
        var smokeEffectObject = _skillData.Pools.Instantiate("Effects/VFX_Smoke", _skillData.SkillPoisition, Quaternion.identity);
        var smokeEffect = smokeEffectObject.GetComponent<VfxSmokeEffect>();
        smokeEffect.Stop();
        smokeEffect.Initialize(_skillData.Pools);
        smokeEffect.Play();
    }

    /// <summary>
    /// LineRenderer 컴포넌트를 초기화하고 기본 설정을 적용
    /// </summary>
    private void InitializeLineRenderer()
    {
        _lineRenderer.material = _skillData.LineMaterial;
        _lineRenderer.startWidth = _skillData.LineWidth;
        _lineRenderer.endWidth = _skillData.LineWidth;
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

        for (int i = 0; i < _skillData.PointCount; i++)
        {
            float distance = Vector2.Distance(_points[i], _lastFramePoints[i]);
            float velocity = distance / Time.deltaTime;
            if (velocity > _skillData.StopVelocityThreshold)
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

        for (int i = 0; i < _skillData.PointCount * 2; i++)
        {
            int currentIndex = i % _skillData.PointCount;
            int prevIndex = (currentIndex - 1 + _skillData.PointCount) % _skillData.PointCount;

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
                    int nextPotentialIndex = (currentIndex + 1) % _skillData.PointCount;
                    var vec2 = Vector2.zero;

                    if (_isFixed[nextPotentialIndex])
                        vec2 = _points[nextPotentialIndex] - _points[currentIndex];

                    if (vec1.sqrMagnitude > 0.0001f && vec2.sqrMagnitude > 0.0001f)
                    {
                        float angle = Vector2.Angle(vec1, vec2);
                        if (angle < 180f - _skillData.AngleThresholdForStraighten)
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
            interpolationRanges.Add(new Tuple<int, int>(currentFixedSegmentStart, _skillData.PointCount - 1));
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
        int count = segEndIdx >= segStartIdx ? segEndIdx - segStartIdx + 1 : _skillData.PointCount - segStartIdx + segEndIdx + 1;

        for (int i = 0; i < count; i++)
        {
            int pointIndex = (segStartIdx + i) % _skillData.PointCount;
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
    private IEnumerator FadeOutAndInactive()
    {
        yield return new WaitForSeconds(_skillData.FadeDelay);

        float elapsedTime = 0f;
        var startColor = _lineRenderer.startColor;

        while (elapsedTime < _skillData.FadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsedTime / _skillData.FadeDuration);
            var newColor = new Color(startColor.r, startColor.g, startColor.b, alpha);
            _lineRenderer.startColor = newColor;
            _lineRenderer.endColor = newColor;
            yield return null;
        }

        _skillData.Pools.Destroy(gameObject);
    }
}