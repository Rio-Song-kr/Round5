using UnityEngine;

/// <summary>
/// 개별 Arc 오브젝트의 이동, 충돌 감지 및 생명 주기를 완벽하게 독립적으로 관리
/// </summary>
public class ArcController : MonoBehaviour
{
    [Header("Collision Settings")]
    // # 충돌 시 대상에게 입힐 데미지
    [SerializeField] private int _damage = 10;
    // # 충돌 시 생성될 이펙트 프리팹
    [SerializeField] private GameObject _hitEffectPrefab;
    // # 충돌을 감지할 대상의 레이어
    [SerializeField] private LayerMask _targetLayer;

    // # EMPEffect로부터 초기화받는 설정값들
    private float _initialExpansionSpeed;
    private float _minExpansionSpeed;
    private float _fastExpansionRadius;
    private float _decelerationDuration;
    private Vector3 _centerPoint;
    private Vector3 _direction;

    // # 내부 상태 변수
    private float _currentSpeed;
    private float _decelerationTimer = 0f;
    private Camera _mainCamera;

    /// <summary>
    /// 매 프레임마다 자신의 상태를 판단하여 속도를 결정하고 이동하며, 화면 밖으로 나가면 비활성화
    /// </summary>
    private void Update()
    {
        // # 속도 결정 로직
        float distanceFromCenter = Vector3.Distance(transform.position, _centerPoint);

        if (distanceFromCenter < _fastExpansionRadius)
        {
            _currentSpeed = _initialExpansionSpeed;
        }
        else
        {
            if (_decelerationTimer < _decelerationDuration)
            {
                _decelerationTimer += Time.deltaTime;
                _currentSpeed = Mathf.Lerp(_initialExpansionSpeed, _minExpansionSpeed,
                    _decelerationTimer / _decelerationDuration);
            }
            else
            {
                _currentSpeed = _minExpansionSpeed;
            }
        }

        // # 이동 로직 (저장된 방향 사용)
        transform.position += _currentSpeed * Time.deltaTime * _direction;

        // # 상태 확인 로직
        if (IsOffScreen()) gameObject.SetActive(false);
    }

    /// <summary>
    /// EMPEffect에 의해 호출되어 Arc의 모든 동작 설정을 초기화
    /// </summary>
    public void Initialize(Vector3 centerPoint, Vector3 direction, float initialSpeed, float minSpeed, float fastRadius,
        float decelDuration)
    {
        _centerPoint = centerPoint;
        _direction = direction;
        _initialExpansionSpeed = initialSpeed;
        _minExpansionSpeed = minSpeed;
        _fastExpansionRadius = fastRadius;
        _decelerationDuration = decelDuration;

        _currentSpeed = _initialExpansionSpeed;
        _mainCamera = Camera.main;
    }

    /// <summary>
    /// 트리거 충돌이 발생했을 때 호출되어, 대상 확인 후 이펙트 생성 및 자신을 비활성화
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // # 충돌한 오브젝트가 지정된 타겟 레이어에 속하는지 확인함
        if ((_targetLayer.value & 1 << other.gameObject.layer) > 0)
        {
            // # 충돌 이펙트가 지정되어 있다면 생성함
            if (_hitEffectPrefab != null)
                Instantiate(_hitEffectPrefab, transform.position, transform.rotation);

            // # 충돌 후 Arc 오브젝트를 비활성화함
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 자신의 위치가 메인 카메라의 화면 밖에 있는지 확인
    /// </summary>
    private bool IsOffScreen()
    {
        if (_mainCamera == null) return false;

        var screenPoint = _mainCamera.WorldToViewportPoint(transform.position);
        return screenPoint.x < 0 || screenPoint.x > 1 || screenPoint.y < 0 || screenPoint.y > 1;
    }
}