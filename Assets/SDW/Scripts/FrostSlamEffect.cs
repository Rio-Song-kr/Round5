using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostSlamEffect : MonoBehaviour
{
    [Header("Expansion Settings")]
    //# 초당 확장 속도
    [SerializeField] private float _expansionSpeed = 10f;
    //# 확장 시 최대 반지름
    [SerializeField] private float _maxRadius = 3f;
    //# 확장 전 시작 반지름
    [SerializeField] private float _initialRadius = 0.1f;
    //# Circle의 포인트 수
    [SerializeField] private int _pointCount = 64;
    //# 감지할 대상 레이어
    [SerializeField] private LayerMask _targetLayer;
    //# 감지 거리
    [SerializeField] private float _detectOffset = 0.01f;
    //# sphere 충돌 범위
    [SerializeField] private float _circleRadius = 0.1f;

    [Header("Line Settings")]
    //# Line Renderer에 사용할 Material
    [SerializeField] private Material _lineMaterial;
    //# Line의 두께
    [SerializeField] private float _lineWidth = 0.02f;

    [Header("Straighten Settings")]
    //# 직선화 적용 기준 각도 (이 각도 이상 꺾이면 포인트 유지, 단위: 도)
    [SerializeField] private float _angleThresholdForStraighten = 120f;

    //# 축별 변동 임계값 - 울툴붕퉁하게 포인트가 잡히는 것을 줄이기 위함
    [Header("Smoothing Settings")]
    [SerializeField] private float _xThreshold = 0.1f;
    [SerializeField] private float _yThreshold = 0.1f;

    //# Collider를 자유롭게 수정할 수 있도록 PolygonCollider 사용
    private PolygonCollider2D _polyCollider;
    //# Circle의 포인트
    private List<Vector2> _points;
    //#이전 프레임의 포인트 저장
    private List<Vector2> _previousPoints;
    //# 해당 Point가 움직이지 않고 고정되었는지 여부
    private List<bool> _isFixed;
    private float _currentRadius;

    private LineRenderer _lineRenderer;
    private float _angle;

    /// <summary>
    /// 컴포넌트 설정 및 초기 Circle 생성
    /// </summary>
    private void Start()
    {
        _polyCollider = GetComponent<PolygonCollider2D>();
        _points = new List<Vector2>(_pointCount);
        _isFixed = new List<bool>(_pointCount);
        _previousPoints = new List<Vector2>(_pointCount);

        //# 초기 반지름 설정
        _currentRadius = _initialRadius;

        //# LineRenderer 설정
        InitializeLineRenderer();

        //# 초기 원형 포인트 생성
        for (int i = 0; i < _pointCount; i++)
        {
            //# Unit 원의 둘레 / 포인트, 각 포인트의 각도 설정
            _angle = i * 2f * Mathf.PI / _pointCount;

            //# 극좌표계 => 직교 좌표계로 변환, x = cos(angle) * radius, y = sin(angle) * radius
            _points.Add(new Vector2(Mathf.Cos(_angle), Mathf.Sin(_angle)) * _currentRadius);
            _isFixed.Add(false);
            _previousPoints.Add(Vector2.zero);
        }

        //# PolygonCollider2D의 초기 포인트 설정
        _polyCollider.SetPath(0, _points);

        //# 시각적인 업데이트
        UpdateLineRenderer();
    }

    /// <summary>
    /// LineRenderer 컴포넌트 설정
    /// </summary>
    private void InitializeLineRenderer()
    {
        _lineRenderer = gameObject.AddComponent<LineRenderer>();

        //# LineRenderer의 material을 지정하지 않았을 때는 새로운 material 생성
        _lineRenderer.material = _lineMaterial != null ? _lineMaterial : new Material(Shader.Find("Sprites/Default"));

        //# 시작 line의 두께와 끝 Line의 두께 설정
        _lineRenderer.startWidth = _lineWidth;
        _lineRenderer.endWidth = _lineWidth;

        //# 원과 같이 닫혀야 하므로 Loop 설정
        _lineRenderer.loop = true;

        //# 로컬 좌표계 사용
        _lineRenderer.useWorldSpace = false;

        //# 시작 line의 색과, 끝 line의 색 설정
        _lineRenderer.startColor = Color.white;
        _lineRenderer.endColor = Color.white;
    }

    /// <summary>
    /// 매 프레임마다 원 확장
    /// </summary>
    private void Update()
    {
        //# 반지름 확장
        _currentRadius += _expansionSpeed * Time.deltaTime;

        //# 모든 지점이 고정되면 더 이상 업데이트할 필요 없음
        if (_currentRadius > _maxRadius)
        {
            _currentRadius = _maxRadius;
            if (AllPointsFixed()) return;
        }

        //# 임시로 다음 프레임의 _points와 _isFixed 상태를 저장할 리스트
        var nextPoints = new List<Vector2>(_pointCount);
        var nextIsFixed = new List<bool>(_pointCount);

        for (int i = 0; i < _pointCount; i++)
        {
            //# 이미 고정된 지점은 이전 값을 유지
            if (_isFixed[i])
            {
                nextPoints.Add(_points[i]);
                nextIsFixed.Add(true);
                continue;
            }

            //# Unit 원의 둘레 / 포인트로 각 포인트의 각도 설정
            _angle = i * 2f * Mathf.PI / _pointCount;
            var direction = new Vector2(Mathf.Cos(_angle), Mathf.Sin(_angle));

            //# 현재 확장하려는 위치
            float targetDistance = _currentRadius;
            var targetPosition = (Vector2)transform.position + direction * (targetDistance + _detectOffset);

            //# 충돌 검사
            var collider = Physics2D.OverlapCircle(targetPosition, _circleRadius, _targetLayer);

            if (collider != null)
            {
                float minDistance = _initialRadius;
                float maxDistance = targetDistance + _detectOffset;
                float stopDistance = minDistance;

                //# 이진 탐색을 통해 충졸 지점을 보다 정밀하게 탐색
                for (int j = 0; j < 5; j++)
                {
                    //# 장애물이 감지되면, 충돌 지점을 정밀하게 찾기 위해이진 탐색으로 정확한 정지 지점 찾기
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

                //# 감지 오프셋을 적용한 최종 정지 거리
                stopDistance = Mathf.Max(stopDistance - _detectOffset, _initialRadius);

                //# 목표 거리보다 정지 거리가 작거나 같으면 고정
                if (targetDistance >= stopDistance)
                {
                    nextPoints.Add(direction * stopDistance);
                    nextIsFixed.Add(true);
                }
                //# 아직 목표 거리까지 도달하지 않았지만 장애물이 감지되는 경우
                else
                {
                    nextPoints.Add(direction * stopDistance);
                    nextIsFixed.Add(false);
                }
            }
            //# 충돌이 없으면 정상적으로 확장
            else
            {
                nextPoints.Add(direction * targetDistance);
                nextIsFixed.Add(false);
            }
        }

        //# 이전 포인트와 비교하여 축별 변동 제한
        for (int i = 0; i < _pointCount; i++)
        {
            //# 이전 포인트가 초기화된 경우에만
            if (_previousPoints[i] != Vector2.zero)
            {
                float deltaX = Mathf.Abs(nextPoints[i].x - _previousPoints[i].x);
                float deltaY = Mathf.Abs(nextPoints[i].y - _previousPoints[i].y);

                //# x축 변동이 작고 y축 변동이 큰 경우, x축을 이전 위치로 고정
                if (deltaX <= _xThreshold && deltaY > _yThreshold)
                    nextPoints[i] = new Vector2(_previousPoints[i].x, nextPoints[i].y);
                //# y축 변동이 작고 x축 변동이 큰 경우, y축을 이전 위치로 고정
                else if (deltaY <= _yThreshold && deltaX > _xThreshold)
                    nextPoints[i] = new Vector2(nextPoints[i].x, _previousPoints[i].y);
            }
        }

        //# 임시 리스트의 값을 실제 _points와 _isFixed에 적용
        _points = nextPoints;
        _isFixed = nextIsFixed;
        _previousPoints = new List<Vector2>(_points); // 이전 포인트 업데이트

        //# 고정된 세그먼트를 직선으로 모간
        ProcessFixedSegments();

        _polyCollider.SetPath(0, _points);
        UpdateLineRenderer();
    }

    /// <summary>
    /// 모든 지점이 고정되었는지 확인
    /// </summary>
    private bool AllPointsFixed()
    {
        foreach (bool fixedStatus in _isFixed)
        {
            if (!fixedStatus) return false;
        }
        return true;
    }

    /// <summary>
    /// 고정된 세그먼트를 찾아 직선으로 보간합니다.
    /// 꺾이는 각도가 설정값 이상인 지점은 고정된 상태로 유지하고 직선화하지 않습니다.
    /// </summary>
    private void ProcessFixedSegments()
    {
        int currentFixedSegmentStart = -1;
        bool inFixedSegment = false;

        //# 임시로 보간할 세그먼트들을 저장할 리스트 (startIdx, endIdx)
        var interpolationRanges = new List<Tuple<int, int>>();

        //# 원형 구조의 마지막과 처음이 연결되는 경우를 처리하기 위해,
        //# _pointCount * 2 만큼 루프를 돌려 연속적인 세그먼트를 찾음.
        for (int i = 0; i < _pointCount * 2; i++)
        {
            int currentIndex = i % _pointCount;
            int prevIndex = (currentIndex - 1 + _pointCount) % _pointCount;

            //# 현재 지점이 고정되어 있다면
            if (_isFixed[currentIndex])
            {
                //# 새로운 고정 세그먼트의 시작이라면 기록
                if (!inFixedSegment)
                {
                    currentFixedSegmentStart = currentIndex;
                    inFixedSegment = true;
                }

                //# 현재 지점이 고정 세그먼트 내에 있고, 첫 지점이 아니라면 꺾임 각도 확인
                if (inFixedSegment && currentIndex != currentFixedSegmentStart)
                {
                    //# 이전 지점과 현재 지점 사이의 벡터
                    var vec1 = _points[currentIndex] - _points[prevIndex];

                    //# 현재 지점과 다음 지점 사이의 벡터 (다음 지점은 현재 세그먼트 내 고정된 지점이어야 함)
                    int nextPotentialIndex = (currentIndex + 1) % _pointCount;
                    var vec2 = Vector2.zero; // 초기화

                    //# 다음 지점도 고정되어 있는지 확인하여 각도 계산에 사용
                    if (_isFixed[nextPotentialIndex])
                        vec2 = _points[nextPotentialIndex] - _points[currentIndex];
                    //# 만약 다음 지점이 고정되어 있지 않다면, 현재 지점이 세그먼트의 끝임
                    //# 이 경우, 현재 지점까지를 하나의 세그먼트로 보고 처리 (다음 if 문에서)
                    //# 또는 각도 계산을 건너뜀
                    else
                        vec2 = Vector2.zero;

                    //# 벡터 크기가 너무 작으면 각도 계산 오류 방지 (거의 동일한 지점)
                    if (vec1.sqrMagnitude > 0.0001f && vec2.sqrMagnitude > 0.0001f)
                    {
                        float angle = Vector2.Angle(vec1, vec2);

                        //# 꺾임 각도가 임계값보다 크면 (180 - threshold 보다 작으면)
                        //# 현재까지의 세그먼트를 보간 대상으로 추가하고 새로운 세그먼트 시작
                        if (angle < 180f - _angleThresholdForStraighten)
                        {
                            //# 현재까지의 직선화 가능한 세그먼트를 추가
                            //# 시작은 currentFixedSegmentStart, 끝은 이전 지점 (prevIndex)
                            interpolationRanges.Add(new Tuple<int, int>(currentFixedSegmentStart, prevIndex));

                            //# 새로운 직선화 세그먼트의 시작은 현재 지점
                            currentFixedSegmentStart = currentIndex;
                        }
                    }
                }
            }
            //# 현재 지점이 고정되어 있지 않다면 (고정 세그먼트의 끝)
            else
            {
                if (inFixedSegment)
                {
                    //# 현재까지의 고정 세그먼트를 보간할 범위로 추가
                    //# 시작은 currentFixedSegmentStart, 끝은 이전 지점 (prevIndex)
                    interpolationRanges.Add(new Tuple<int, int>(currentFixedSegmentStart, prevIndex));
                    inFixedSegment = false;

                    //# 초기화
                    currentFixedSegmentStart = -1;
                }
            }
        }

        //# 루프가 끝난 후, 아직 열려있는 고정 세그먼트가 있다면 처리 (원형 연결의 마지막 부분)
        if (inFixedSegment && currentFixedSegmentStart != -1)
        {
            //# 세그먼트의 끝은 _pointCount - 1 (마지막 인덱스)
            //# ApplyLinearInterpolation에서 이 원형 연결을 처리하도록 함.
            interpolationRanges.Add(new Tuple<int, int>(currentFixedSegmentStart, _pointCount - 1));
        }

        //# 식별된 각 보간 범위에 대해 직선 보간을 적용
        foreach (var range in interpolationRanges)
        {
            //# 시작점과 끝점이 다르고, 유효한 범위인 경우에만 보간
            //# ApplyLinearInterpolation 내부에서 이 유효성 검사를 다시 수행
            ApplyLinearInterpolation(range.Item1, range.Item2);
        }
    }

    /// <summary>
    /// 주어진 시작 인덱스와 끝 인덱스 사이의 포인트를 직선으로 보간합니다.
    /// 이 함수는 `ProcessFixedSegments`에서 이미 보간할 세그먼트가 결정된 후에 호출됩니다.
    /// </summary>
    /// <param name="segStartIdx">세그먼트의 시작 인덱스</param>
    /// <param name="segEndIdx">세그먼트의 끝 인덱스</param>
    private void ApplyLinearInterpolation(int segStartIdx, int segEndIdx)
    {
        //# 시작점과 끝점이 같으면 보간할 필요 없음 (단일 지점 또는 세그먼트가 없음)
        if (segStartIdx == segEndIdx) return;

        var startPoint = _points[segStartIdx];
        var endPoint = _points[segEndIdx];

        int count = segEndIdx >= segStartIdx ? segEndIdx - segStartIdx + 1 : _pointCount - segStartIdx + segEndIdx + 1;

        for (int i = 0; i < count; i++)
        {
            int pointIndex = (segStartIdx + i) % _pointCount;

            //# 0에서 1 사이의 보간 비율
            float t = (float)i / (count - 1);
            _points[pointIndex] = Vector2.Lerp(startPoint, endPoint, t);
        }
    }

    /// <summary>
    /// LineRenderer 컴포넌트를 업데이트하여 원형의 포인트들을 시각적으로 표현
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
}