using Photon.Pun;
using UnityEngine;

/// <summary>
/// 개별 Arc 오브젝트의 이동, 충돌 감지 및 생명 주기를 완벽하게 독립적으로 관리
/// </summary>
public class ArcController : MonoBehaviourPun
{
    [Header("Collision Settings")]
    //# 충돌 시 생성될 이펙트 프리팹
    [SerializeField] private GameObject _hitEffectPrefab;
    //# 충돌을 감지할 대상의 레이어

    //# EMPEffect로부터 초기화받는 설정값들
    private float _initialExpansionSpeed;
    private float _minExpansionSpeed;
    private float _fastExpansionRadius;
    private float _decelerationDuration;
    private Vector3 _centerPoint;
    private Vector3 _direction;

    //# 내부 상태 변수
    private float _currentSpeed;
    private float _decelerationTimer;
    private Camera _mainCamera;

    private GameObject _hitEffectObject;
    private VfxArcEffect _hitEffect;
    private bool _isReleased;

    public EmpEffectSkillDataSO SkillData;

    /// <summary>
    /// 매 프레임마다 자신의 상태를 판단하여 속도를 결정하고 이동하며, 화면 밖으로 나가면 Pool에 반환
    /// </summary>
    private void Update()
    {
        if (!photonView.IsMine) return;

        //# 속도 결정 로직
        float distanceFromCenter = Vector3.Distance(transform.position, _centerPoint);

        if (distanceFromCenter < _fastExpansionRadius)
            _currentSpeed = _initialExpansionSpeed;
        else
        {
            if (_decelerationTimer < _decelerationDuration)
            {
                _decelerationTimer += Time.deltaTime;
                _currentSpeed = Mathf.Lerp(_initialExpansionSpeed, _minExpansionSpeed,
                    _decelerationTimer / _decelerationDuration);
            }
            else
                _currentSpeed = _minExpansionSpeed;
        }

        //# 이동 로직 (저장된 방향 사용)
        transform.position += _currentSpeed * Time.deltaTime * _direction;

        //# 상태 확인 로직
        if (IsOffScreen())
        {
            if (_isReleased) return;

            PhotonNetwork.Destroy(gameObject);
        }
    }

    /// <summary>
    /// EMPEffect에 의해 호출되어 Arc의 모든 동작 설정을 초기화
    /// </summary>
    public void Initialize(
        EmpEffectSkillDataSO skillData,
        int viewId,
        Vector3 direction,
        float initialSpeed,
        float minSpeed,
        float fastRadius,
        float decelerationDuration)
    {
        if (!photonView.IsMine) return;

        SkillData = skillData;


        photonView.RPC(
            nameof(InitializeArc),
            RpcTarget.All,
            viewId,
            direction,
            initialSpeed,
            minSpeed,
            fastRadius,
            decelerationDuration);
    }

    /// <summary>
    /// 초기화 매개변수를 통해 Arc 오브젝트의 동작 설정을 원격으로 수행
    /// </summary>
    /// <param name="viewId">Arc 오브젝트의 Photon View ID</param>
    /// <param name="direction">Arc 오브젝트의 초기 이동 방향</param>
    /// <param name="initialSpeed">초기 확장 속도</param>
    /// <param name="minSpeed">최소 유지 속도</param>
    /// <param name="fastRadius">빠른 확장 반경 조건</param>
    /// <param name="decelerationDuration">감속 시간</param>
    [PunRPC]
    private void InitializeArc(
        int viewId,
        Vector3 direction,
        float initialSpeed,
        float minSpeed,
        float fastRadius,
        float decelerationDuration)
    {
        var empTransform = PhotonView.Find(viewId).transform;

        var effect = empTransform.GetComponent<EmpEffect>();

        if (SkillData == null)
            SkillData = effect.SkillData;

        _centerPoint = empTransform.position;
        _direction = direction;
        _initialExpansionSpeed = initialSpeed;
        _minExpansionSpeed = minSpeed;
        _fastExpansionRadius = fastRadius;
        _decelerationDuration = decelerationDuration;

        _currentSpeed = _initialExpansionSpeed;
        _mainCamera = Camera.main;

        _decelerationTimer = 0f;
        _isReleased = false;
    }

    /// <summary>
    /// 트리거 충돌이 발생했을 때 호출되어, 대상 확인 후 이펙트 재생 및 Pool에 반환
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isReleased || !photonView.IsMine || photonView == null) return;
        if (SkillData == null)
        {
            Debug.Log("_skillData = null");
            return;
        }

        //# 충돌한 오브젝트가 지정된 타겟 레이어에 속하는지 확인함
        if ((SkillData.TargetMask.value & 1 << other.gameObject.layer) > 0)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                int targetViewId = other.GetComponent<PhotonView>().ViewID;
                photonView.RPC(nameof(ApplyReduceEffect), RpcTarget.All, targetViewId);
            }

            //# Pool에서 VFX_Arc를 꺼냄
            _hitEffectObject = PhotonNetwork.Instantiate("VFX_Arc", transform.position, transform.rotation);
            _hitEffect = _hitEffectObject.GetComponent<VfxArcEffect>();
            _hitEffect.Stop();
            _hitEffect.Initialize();

            _hitEffectObject.transform.SetPositionAndRotation(transform.position, transform.rotation);

            photonView.RPC(nameof(PlayHitEffect), RpcTarget.All, transform.position, transform.rotation);
            _isReleased = true;
        }
    }

    /// <summary>
    /// 적용된 방어 효과를 감소시키는 원격 호출 메서드
    /// 특정 뷰 ID의 대상 오브젝트에 속도 감소 효과를 적용
    /// </summary>
    /// <param name="viewId">효과를 적용할 대상 오브젝트의 Photon View ID</param>
    [PunRPC]
    private void ApplyReduceEffect(int viewId)
    {
        var targetView = PhotonView.Find(viewId);
        var targetStatus = targetView.gameObject.GetComponent<IStatusEffectable>();

        foreach (var status in SkillData.Status)
        {
            if (status.EffectType != StatusEffectType.ReduceSpeed) continue;

            targetStatus.ApplyStatusEffect(
                status.EffectType,
                status.EffectValue,
                status.Duration
            );
        }
    }

    /// <summary>
    /// 개별 Arc 오브젝트의 상태를 업데이트하여 화면 내에서 움직이도록 제어
    /// 화면 밖으로 나가면 Pool 시스템에 반환하는 함수
    /// </summary>
    /// <param name="position">Arc 오브젝트의 새로운 위치 벡터</param>
    /// <param name="rotation">Arc 오브젝트의 새로운 회전 쿼터니언</param>
    [PunRPC]
    private void PlayHitEffect(Vector3 position, Quaternion rotation)
    {
        if (_hitEffectObject != null && _hitEffect != null || photonView != null || !_isReleased)
        {
            if (photonView.IsMine)
            {
                _hitEffectObject?.transform.SetPositionAndRotation(position, rotation);
                _hitEffect?.Play();
            }
        }

        //# Pool에 ArcController 반환
        if (photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
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