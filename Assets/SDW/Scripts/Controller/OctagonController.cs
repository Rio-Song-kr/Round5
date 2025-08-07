using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 두 개의 팔각형 UI 이미지에 스케일링과 회전 애니메이션을 적용하는 컨트롤러
/// </summary>
public class OctagonController : MonoBehaviour
{
    [Header("Image UI")]
    //# 애니메이션을 적용할 첫 번째 팔각형 UI 이미지
    [SerializeField] private Image _octagon1;
    //# 애니메이션을 적용할 두 번째 팔각형 UI 이미지
    [SerializeField] private Image _octagon2;

    [Header("Scale Settings")]
    //# 팔각형이 확장될 때의 최대 scale
    [SerializeField] private float _maxScale = 1.1f;
    //# 0에서 최대 크기까지 확장되는 데 걸리는 시간
    [SerializeField] private float _expandDuration = 1f;
    //# 최대 크기에서 0으로 축소되는 데 걸리는 시간
    [SerializeField] private float _shrinkDuration = 1f;
    //# 두 번째 팔각형이 첫 번째 팔각형보다 늦게 시작되는 지연 시간
    [SerializeField] private float _delayBetweenOctagons = 0.1f;
    //# 최대 크기에 도달한 후 대기하는 시간
    [SerializeField] private float _pauseDuration = 3f;
    //# 확장 시에 적용할 easing 곡선(빠르게 시작해서 천천히 끝남)
    [SerializeField] private AnimationCurve _expandCurve;

    [Header("Rotation Settings")]
    //# 첫 번째 팔각형의 회전 속도(도/초 단위)
    [SerializeField] private float _octagon1RotationSpeed = 360f;
    //# 두 번째 팔각형의 회전 속도(도/초 단위, 음수면 반대 방향)
    [SerializeField] private float _octagon2RotationSpeed = -360f;
    //# 회전에 적용할 easing 곡선(부드러운 회전 변화를 위함)
    [SerializeField] private AnimationCurve _rotationCurve;

    //# 팔각형들의 초기 크기를 저장하는 변수
    private Vector3 _initialScale;
    //# 첫 번째 팔각형의 스케일 애니메이션 타이머
    private float _timer1;
    //# 두 번째 팔각형의 스케일 애니메이션 타이머
    private float _timer2;
    //# 첫 번째 팔각형이 현재 확장 중인지 여부
    private bool _isExpanding1;
    //# 두 번째 팔각형이 현재 확장 중인지 여부
    private bool _isExpanding2;
    //# 첫 번째 팔각형이 현재 최대 크기에서 대기 중인지 여부
    private bool _isPaused1;
    //# 두 번째 팔각형이 현재 최대 크기에서 대기 중인지 여부
    private bool _isPaused2;
    //# 첫 번째 팔각형의 애니메이션이 완료되었는지 여부
    private bool _isCompleted1;
    //# 두 번째 팔각형의 애니메이션이 완료되었는지 여부
    private bool _isCompleted2;

    //# 첫 번째 팔각형의 회전 애니메이션 타이머
    private float _rotationTimer1;
    //# 두 번째 팔각형의 회전 애니메이션 타이머
    private float _rotationTimer2;
    //# 첫 번째 팔각형의 초기 회전값(교차 배치를 위해 29.5도 회전)
    private float _initialRotation1 = 29.5f;
    //# 두 번째 팔각형의 초기 회전값
    private float _initialRotation2 = 0f;
    //# 전체 애니메이션이 완료되었는지 여부
    private bool _isAnimationComplete;

    /// <summary>
    /// 게임 오브젝트가 활성화될 때마다 애니메이션을 초기화하고 시작
    /// </summary>
    private void OnEnable() => Init();

    /// <summary>
    /// 팔각형들의 초기 상태를 설정하고 애니메이션을 시작하기 위한 준비 작업을 수행
    /// </summary>
    private void Init()
    {
        //# 첫 번째 팔각형의 현재 크기를 기준 크기로 저장
        if (_initialScale == Vector3.zero) _initialScale = _octagon1.transform.localScale;

        //# 두 팔각형 모두 크기를 0으로 설정하여 확장 애니메이션으로 시작
        _octagon1.transform.localScale = Vector3.zero;
        _octagon2.transform.localScale = Vector3.zero;

        //# 첫 번째 팔각형은 즉시 시작
        _timer1 = 0f;

        //# 두 번째 팔각형은 지연 시간만큼 늦게 시작
        _timer2 = -_delayBetweenOctagons;

        //# 두 팔각형 모두 확장 상태로 시작
        _isExpanding1 = true;
        _isExpanding2 = true;

        //# 두 팔각형 모두 대기 상태가 아님
        _isPaused1 = false;
        _isPaused2 = false;

        //# 두 팔각형 모두 완료 상태가 아님(초기화)
        _isCompleted1 = false;
        _isCompleted2 = false;

        //# 첫 번째 팔각형 회전은 즉시 시작
        _rotationTimer1 = 0f;

        //# 두 번째 팔각형 회전도 스케일과 동일한 지연 시간 적용
        _rotationTimer2 = -_delayBetweenOctagons;

        //# 두 팔각형이 시각적으로 교차하도록 서로 다른 초기 회전값 적용
        _octagon1.transform.rotation = Quaternion.Euler(0, 0, _initialRotation1);
        _octagon2.transform.rotation = Quaternion.Euler(0, 0, _initialRotation2);

        //# 전체 애니메이션 완료 상태 초기화
        _isAnimationComplete = false;

        _expandCurve = AnimationCurve.EaseInOut(0f, 0f, _expandDuration, 1f);
            _rotationCurve = AnimationCurve.EaseInOut(0f, 0f, _expandDuration, 1f);
    }

    /// <summary>
    /// 매 프레임마다 두 팔각형의 스케일과 회전 애니메이션을 업데이트
    /// 애니메이션이 완료되면 업데이트를 중단
    /// </summary>
    private void Update()
    {
        //# 애니메이션이 완료되었으면 더 이상 업데이트하지 않음
        if (_isAnimationComplete) return;

        //# 각 팔각형의 스케일 애니메이션을 독립적으로 업데이트
        if (!_isCompleted1) UpdateOctagon(_octagon1, ref _timer1, ref _isExpanding1, ref _isPaused1, ref _isCompleted1);
        if (!_isCompleted2) UpdateOctagon(_octagon2, ref _timer2, ref _isExpanding2, ref _isPaused2, ref _isCompleted2);

        //# 각 팔각형의 회전 애니메이션을 독립적으로 업데이트 (완료되지 않은 경우에만)
        if (!_isCompleted1) UpdateRotation(_octagon1, ref _rotationTimer1, _octagon1RotationSpeed, _initialRotation1);
        if (!_isCompleted2) UpdateRotation(_octagon2, ref _rotationTimer2, _octagon2RotationSpeed, _initialRotation2);

        //# 두 팔각형 모두 완료되었는지 확인
        if (_isCompleted1 && _isCompleted2) _isAnimationComplete = true;
    }

    /// <summary>
    /// 개별 팔각형의 스케일 애니메이션을 처리하는 메서드
    /// 확장-대기-축소를 한 번만 수행하고 완료 상태로 변경
    /// </summary>
    /// <param name="octagon">애니메이션을 적용할 팔각형 이미지</param>
    /// <param name="timer">해당 팔각형의 애니메이션 타이머</param>
    /// <param name="isExpanding">현재 확장 중인지 여부</param>
    /// <param name="isPaused">현재 대기 중인지 여부</param>
    /// <param name="isCompleted">애니메이션이 완료되었는지 여부</param>
    private void UpdateOctagon(Image octagon, ref float timer, ref bool isExpanding, ref bool isPaused, ref bool isCompleted)
    {
        //# 매 프레임마다 타이머 증가
        timer += Time.deltaTime;

        //# 대기 상태일 때의 처리
        if (isPaused)
        {
            //# 대기 시간이 끝나면 축소 단계로 전환
            if (timer >= _pauseDuration)
            {
                isPaused = false;
                isExpanding = false;
                timer = 0f;
            }
            return;
        }

        //# 확장 단계일 때의 처리
        if (isExpanding)
        {
            //# 0에서 1까지의 진행률 계산
            float t = Mathf.Clamp01(timer / _expandDuration);

            //# easing 곡선을 적용하여 자연스러운 확장 효과 구현
            float scaleFactor = Mathf.Lerp(0f, _maxScale, _expandCurve.Evaluate(t));
            octagon.transform.localScale = _initialScale * scaleFactor;

            //# 확장이 완료되면 대기 단계로 전환
            if (t >= 1f)
            {
                isPaused = true;
                timer = 0f;
            }
        }
        //# 축소 단계일 때의 처리
        else
        {
            //# 최대 크기에서 0으로 일정한 속도로 축소
            float t = Mathf.Clamp01(timer / _shrinkDuration);
            float scaleFactor = Mathf.Lerp(_maxScale, 0f, t);
            octagon.transform.localScale = _initialScale * scaleFactor;

            //# 축소가 완료되면 애니메이션 완료 처리(사이클 반복하지 않음)
            if (t >= 1f)
            {
                //# 크기를 완전히 0으로 고정
                octagon.transform.localScale = Vector3.zero;

                //# 이 팔각형의 애니메이션 완료 표시
                isCompleted = true;
            }
        }
    }

    /// <summary>
    /// 개별 팔각형의 회전 애니메이션을 처리하는 메서드
    /// 지속적인 회전에 easing 효과를 적용하여 부드러운 회전 변화를 구현
    /// </summary>
    /// <param name="octagon">회전 애니메이션을 적용할 팔각형 이미지</param>
    /// <param name="rotationTimer">해당 팔각형의 회전 타이머</param>
    /// <param name="rotationSpeed">회전 속도 (도/초 단위)</param>
    /// <param name="initialRotation">초기 회전값</param>
    private void UpdateRotation(Image octagon, ref float rotationTimer, float rotationSpeed, float initialRotation)
    {
        //# 지연 시간이 아직 남아있으면 대기
        if (rotationTimer < 0)
        {
            rotationTimer += Time.deltaTime;
            return;
        }

        //# 회전 타이머 증가
        rotationTimer += Time.deltaTime;

        //# 전체 스케일 애니메이션 사이클 시간을 기준으로 easing 계산
        float totalCycleDuration = _expandDuration + _pauseDuration + _shrinkDuration;

        //# 현재 사이클에서의 진행률 계산 (0 ~ 1 범위로 반복)
        float cycleProgress = rotationTimer % totalCycleDuration / totalCycleDuration;

        //# easing 곡선을 적용하여 부드러운 회전 변화 효과
        float easedProgress = _rotationCurve.Evaluate(cycleProgress);

        //# 기본 회전값 계산 (시간에 비례한 누적 회전)
        float baseRotation = rotationTimer * rotationSpeed;
        //# easing 효과를 위한 추가 오프셋 계산 (강도 조절 가능)
        float easingOffset = (easedProgress - cycleProgress) * 30f;

        //# 최종 회전값 적용 (초기 회전 + 기본 회전 + easing 오프셋)
        float currentRotation = initialRotation + baseRotation + easingOffset;
        octagon.transform.rotation = Quaternion.Euler(0, 0, currentRotation);
    }
}