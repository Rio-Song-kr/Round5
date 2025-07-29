using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;

public abstract class BaseWeapon : MonoBehaviourPunCallbacks, IWeapon, IPunObservable
{
    [Header("탄환 정보")]
    [SerializeField] protected int maxAmmo; // 최대 탄약 수
    [SerializeField] protected int currentAmmo; // 남아 있는 탄약 수
    [SerializeField] protected float reloadTime; // 재장전 애니메이션 및 동작에 걸리는 시간 (초)
    [SerializeField] public bool isBigBullet = false; // 큰 탄환 여부
    [SerializeField] public bool isExplosiveBullet = false; // 폭발성 탄환 여부
    
    [Header("재장전 관련")]
    [SerializeField] protected Animator animator; // 재장전 애니메이션용 애니메이터
    [SerializeField] protected bool isReloading; // 재장전 여부
    [SerializeField] protected float idleReloadDelay; // 공격하지 않았을 때 자동 재장전까지의 대기 시간
    [SerializeField] protected Coroutine idleCheckCoroutine; // 자동 재장전 감지를 위한 코루틴 핸들
    [SerializeField] protected Coroutine autoReloadCoroutine; // 리로드 애니메이션과 함께 실행되는 자동 재장전 코루틴 핸들
    
    [Header("참조")]
    [SerializeField] protected AmmoDisplay ammoDisplay; // 탄약 UI를 표시하는 컴포넌트
    
    [Header("공격 정보")]
    public float bulletSpeed;
    [SerializeField] protected int attackDamage; // 

    [SerializeField] protected float attackSpeed; // 
    [SerializeField] protected float lastAttackTime; // 마지막으로 공격한 시간 (탄창 남아있는데 공격하지 않았을 때 자동 재장전 감지용)
    
    [Header("스크립트")]
    protected GunControll gunController; // 총기 컨트롤러
    protected Bullet bullet; // 총기 컨트롤러
    protected Laser laser;

    private Vector3 _networkPosition;
    private Quaternion _networkRotation;

    protected virtual void Start()
    {
        ammoDisplay = FindObjectOfType<AmmoDisplay>();
        Initialize();
        StartCoroutine(DelayedReloadSpeed()); // 1프레임 후 clip 길이 확인
        gunController = FindObjectOfType<GunControll>();
        bullet = FindObjectOfType<Bullet>();
        laser = FindObjectOfType<Laser>();
    }
    
    /// <summary>
    /// 기준 공격 속도 체크 메서드 추가
    /// </summary>
    /// <returns></returns>
    protected bool CanAttack()
    {
        return Time.time - lastAttackTime >= 1f / attackSpeed;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        Initialize();
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
        // animator.speed = 1f; 
        float speed = 2f / reloadTime / 2; // 애니메이션 속도 계산
        // animator.speed = ; 
        
        photonView.RPC(nameof(RPC_SetAnimatorSpeed), RpcTarget.All, speed);
    }
    
    [PunRPC]
    protected void RPC_SetAnimatorSpeed(float speed)
    {
        if (animator != null)
        {
            animator.speed = speed;
            Debug.Log($"[RPC_SetAnimatorSpeed] 애니메이터 속도 설정: {speed}");
        }
    }

    protected virtual void Update()
    {
        if (isReloading && animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Reload") && stateInfo.normalizedTime >= 1f)
            {
                FinishReload();
            }
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
        Invoke(nameof(FinishReload), reloadTime);
    }

    [PunRPC]
    protected void RPC_FinishReload()
    {
        currentAmmo = maxAmmo;
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
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            if (!isReloading && currentAmmo < maxAmmo && Time.time - lastAttackTime >= idleReloadDelay)
            {
                currentAmmo = maxAmmo;
                isReloading = false;
                ammoDisplay?.UpdateAmmoIcons(currentAmmo, maxAmmo);
                lastAttackTime = Time.time;
            }
        }
    }

    protected void UpdateAmmoUI()
    {
        ammoDisplay?.UpdateAmmoIcons(currentAmmo, maxAmmo);
    }
    
    public virtual void Initialize()
    {
        currentAmmo = maxAmmo;
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
            stream.SendNext(currentAmmo);
        }
        else
        {
            Vector3 position = (Vector3)stream.ReceiveNext();
            Quaternion rotation = (Quaternion)stream.ReceiveNext();
            currentAmmo = (int)stream.ReceiveNext();
        
            transform.position = position;
            transform.rotation = rotation;

            UpdateAmmoUI();
            
        }
    }
}
