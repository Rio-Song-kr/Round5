using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;

public abstract class BaseWeapon : MonoBehaviourPunCallbacks, IWeapon, IPunObservable
{
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected GameObject laserPrefab;
    [SerializeField] protected GameObject fragmentPrefab;
    [SerializeField] protected GameObject explosivePrefab;

    // private PhotonView photonView;
    protected GunControll gunController;

    [Header("탄환 정보")]
    public int maxAmmo; // 최대 탄약 수
    public int currentAmmo; // 남아 있는 탄약 수
    // public float reloadTime; // 재장전 애니메이션 및 동작에 걸리는 시간 (초)
    // public int useAmmo; // 공격 시 소모되는 탄약 수

    [Header("재장전 관련")]
    [SerializeField] protected Animator animator; // 재장전 애니메이션용 애니메이터
    [SerializeField] protected bool isReloading; // 재장전 여부
    // [SerializeField] protected float idleReloadDelay; // 공격하지 않았을 때 자동 재장전까지의 대기 시간
    [SerializeField] protected Coroutine idleCheckCoroutine; // 자동 재장전 감지를 위한 코루틴 핸들
    // [SerializeField] protected Coroutine autoReloadCoroutine; // 리로드 애니메이션과 함께 실행되는 자동 재장전 코루틴 핸들

    [Header("참조")]
    [SerializeField] protected AmmoDisplay ammoDisplay; // 탄약 UI를 표시하는 컴포넌트

    [Header("공격 정보")]
    // public float bulletSpeed;
    // public int attackDamage; // 

    // public float attackSpeed; // 
    [SerializeField] protected float lastAttackTime; // 마지막으로 공격한 시간 (탄창 남아있는데 공격하지 않았을 때 자동 재장전 감지용)

    private Vector3 _networkPosition;
    private Quaternion _networkRotation;

    protected PoolManager _poolManager;
    // protected CardManager cardManager;

    protected virtual void Start()
    {
        if (photonView.IsMine)
        {
            if (CardManager.Instance == null)
            {
                Debug.Log("Card Manager가 없습니다");
            }
            else
            {
                maxAmmo = (int)CardManager.Instance.GetCaculateCardStats().DefaultAmmo;
            }
        }

        _poolManager = FindFirstObjectByType<PoolManager>();

        _poolManager.InitializePool("Bullet", bulletPrefab, 1, 1);
        _poolManager.InitializePool("Fragment", fragmentPrefab, 1, 1);
        _poolManager.InitializePool("Laser", laserPrefab, 1, 1);
        _poolManager.InitializePool("Explosive", explosivePrefab, 1, 1);


        gunController = GetComponentInParent<GunControll>();
        // cardManager = FindFirstObjectByType<CardManager>();
        Initialize();
        StartCoroutine(DelayedReloadSpeed()); // 1프레임 후 clip 길이 확인
    }

    /// <summary>
    /// 기준 공격 속도 체크 메서드 추가
    /// </summary>
    /// <returns></returns>
    protected bool CanAttack()
        => Time.time - lastAttackTime >= CardManager.Instance.GetCaculateCardStats().DefaultAttackSpeed;

    public override void OnEnable()
    {
        base.OnEnable();
        // Initialize();
        StartCoroutine(DelayedReloadSpeed());
    }

    public override void OnDisable()
    {
        base.OnDisable();
        StopAllCoroutines();
        ammoDisplay.reloadIndicator.SetActive(false);
        isReloading = false;
    }

    protected IEnumerator DelayedReloadSpeed()
    {
        yield return null;
        ReloadSpeedFromAnimator();
    }

    /// <summary>
    /// 현재 Animator의 "Reload" 상태에서 재장전 속도를 자동으로 계산하여 설정합니다.
    /// </summary>
    protected void ReloadSpeedFromAnimator()
    {
        // float speed = 2f / reloadTime / 2; // 애니메이션 속도 계산
        Debug.Log(
            $"CardManager.Instance.GetCaculateCardStats().DefaultReloadSpeed : {CardManager.Instance.GetCaculateCardStats().DefaultReloadSpeed}");
        float speed = 2f / CardManager.Instance.GetCaculateCardStats().DefaultReloadSpeed / 2; // 애니메이션 속도 계산

        photonView.RPC(nameof(RPC_SetAnimatorSpeed), RpcTarget.All, speed);
    }

    [PunRPC]
    protected void RPC_SetAnimatorSpeed(float speed)
    {
        if (animator != null)
        {
            animator.speed = speed;
            // Debug.Log($"[RPC_SetAnimatorSpeed] 애니메이터 속도 설정: {speed}");
        }
    }

    protected virtual void Update()
    {
        if (isReloading && animator != null)
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Reload") && stateInfo.normalizedTime >= 1f)
            {
                FinishReload();
            }
        }

        if (photonView.IsMine)
        {
            maxAmmo = (int)CardManager.Instance.GetCaculateCardStats().DefaultAmmo;
        }
    }

    protected void StartAutoReload()
    {
        photonView.RPC(nameof(RPC_StartAutoReload), RpcTarget.All);
    }

    protected void FinishReload()
    {
        photonView.RPC(nameof(RPC_FinishReload), RpcTarget.All);
    }

    [PunRPC]
    protected void RPC_StartAutoReload()
    {
        isReloading = true;
        ammoDisplay?.SetReloading(true);

        if (animator != null)
        {
            animator.SetTrigger("Reload");
            ReloadSpeedFromAnimator();
        }

        // 리로드 시간 후 자동 완료 호출
        // Invoke(nameof(FinishReload), reloadTime);
        Invoke(nameof(FinishReload), CardManager.Instance.GetCaculateCardStats().DefaultReloadSpeed);
    }

    [PunRPC]
    protected void RPC_FinishReload()
    {
        currentAmmo = maxAmmo;
        // currentAmmo = (int)CardManager.Instance.GetCaculateCardStats().DefaultAmmo;
        isReloading = false;
        UpdateAmmoUI();
        ammoDisplay?.SetReloading(false);
    }

    protected void NowIdleCheck()
    {
        if (idleCheckCoroutine != null)
            StopCoroutine(idleCheckCoroutine);
        idleCheckCoroutine = StartCoroutine(IdleCheckRoutine());
    }

    private IEnumerator IdleCheckRoutine()
    {
        lastAttackTime = Time.time;
        Debug.Log($"----- lastAttackTime : {lastAttackTime}");

        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            // if (!isReloading && currentAmmo < maxAmmo && Time.time - lastAttackTime >= idleReloadDelay)
            // if (!isReloading && currentAmmo < playerStatusDataSO.DefaultAmmo && Time.time - lastAttackTime >= idleReloadDelay)
            // if (!isReloading && currentAmmo < CardManager.Instance.GetCaculateCardStats().DefaultAmmo && Time.time - lastAttackTime >= CardManager.Instance.GetCaculateCardStats().DefaultReloadSpeed)
            if (!isReloading && currentAmmo < maxAmmo &&
                Time.time - lastAttackTime >= CardManager.Instance.GetCaculateCardStats().DefaultReloadSpeed)
            {
                // currentAmmo = (int)CardManager.Instance.GetCaculateCardStats().DefaultAmmo;
                currentAmmo = maxAmmo;
                isReloading = false;
                // ammoDisplay?.UpdateAmmoIcons((int)currentAmmo, (int)CardManager.Instance.GetCaculateCardStats().DefaultAmmo);
                ammoDisplay?.UpdateAmmoIcons(currentAmmo, maxAmmo);
                lastAttackTime = Time.time;
            }
        }
    }

    protected void UpdateAmmoUI()
    {
        Debug.Log($"currentAmmo: {currentAmmo}, maxAmmo: {maxAmmo}");

        ammoDisplay?.UpdateAmmoIcons(currentAmmo, maxAmmo);
        // ammoDisplay?.UpdateAmmoIcons(currentAmmo, (int)CardManager.Instance.GetCaculateCardStats().DefaultAmmo);
    }

    public virtual void Initialize()
    {
        currentAmmo = maxAmmo;
        // currentAmmo = (int)CardManager.Instance.GetCaculateCardStats().DefaultAmmo;
        isReloading = false;
        UpdateAmmoUI();
        NowIdleCheck();
    }

    public abstract void Attack(Transform firingPoint);
    public abstract WeaponType GetWeaponType();

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(maxAmmo);
            stream.SendNext(currentAmmo);
        }
        else
        {
            var position = (Vector3)stream.ReceiveNext();
            var rotation = (Quaternion)stream.ReceiveNext();
            maxAmmo = (int)stream.ReceiveNext();
            currentAmmo = (int)stream.ReceiveNext();

            if (!photonView.IsMine)
            {
                transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * 10f);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 10f);
            }

            UpdateAmmoUI();
        }
    }
}