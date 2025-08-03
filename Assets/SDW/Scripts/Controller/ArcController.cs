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

        //# 이동 로직 (저장된 방향 사용)
        transform.position += _currentSpeed * Time.deltaTime * _direction;

        //# 상태 확인 로직
        if (IsOffScreen())
        {
            if (_isReleased) return;
            PhotonNetwork.Destroy(_hitEffectObject);
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

        //# Pool에서 VFX_Arc를 꺼냄
        _hitEffectObject = PhotonNetwork.Instantiate("VFX_Arc", transform.position, transform.rotation);
        _hitEffect = _hitEffectObject.GetComponent<VfxArcEffect>();
        _hitEffect.Stop();
        _hitEffect.Initialize();
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

            _hitEffectObject.transform.SetPositionAndRotation(transform.position, transform.rotation);

            photonView.RPC(nameof(PlayHitEffect), RpcTarget.All, transform.position, transform.rotation);

            //# Pool에 ArcController 반환
            PhotonNetwork.Destroy(gameObject);
            _isReleased = true;
        }
    }

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
    /// Hit Effect 재생 RPC
    /// </summary>
    [PunRPC]
    private void PlayHitEffect(Vector3 position, Quaternion rotation)
    {
        if (_hitEffectObject != null && _hitEffect != null || photonView != null || !_isReleased)
        {
            _hitEffectObject.transform.SetPositionAndRotation(position, rotation);
            _hitEffect.Play();
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