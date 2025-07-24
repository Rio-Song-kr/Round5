using System.Collections;
using UnityEngine;

public abstract class BaseWeapon : MonoBehaviour, IWeapon
{
    [SerializeField] protected int maxAmmo; // 최대 탄약 수
    [SerializeField] protected Animator animator; // 재장전 애니메이션용 애니메이터

    protected int currentAmmo; // 남아 있는 탄약 수
    protected bool isReloading; // 재장전 여부
    protected float lastAttackTime; // 마지막으로 공격한 시간 (탄창 남아있는데 공격하지 않았을 때 자동 재장전 감지용)
    protected float idleReloadDelay = 3f; // 공격하지 않았을 때 자동 재장전까지의 대기 시간
    protected Coroutine idleCheckCoroutine; // 자동 재장전 감지를 위한 코루틴 핸들
    private Coroutine autoReloadCoroutine; // 리로드 애니메이션과 함께 실행되는 자동 재장전 코루틴 핸들
    protected float reloadTime = 3f; // 재장전 애니메이션 및 동작에 걸리는 시간 (초)
    protected AmmoDisplay ammoDisplay; // 탄약 UI를 표시하는 컴포넌트

    protected virtual void Start()
    {
        ammoDisplay = FindObjectOfType<AmmoDisplay>();
        Initialize();
    }

    protected virtual void OnEnable()
    {
        Initialize();
    }

    protected virtual void OnDisable()
    {
        if (idleCheckCoroutine != null)
            StopCoroutine(idleCheckCoroutine);
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
        if (autoReloadCoroutine != null)
            StopCoroutine(autoReloadCoroutine);

        autoReloadCoroutine = StartCoroutine(AutoReloadRoutine());
        // if (isReloading) return;
        // isReloading = true;
        // animator?.SetTrigger("Reload");
        // ammoDisplay?.SetReloading(true);
    }
    
    private IEnumerator AutoReloadRoutine()
    {
        isReloading = true;
        ammoDisplay?.SetReloading(true);
        animator?.SetTrigger("Reload");

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
        UpdateAmmoUI();
        ammoDisplay?.SetReloading(false);
        lastAttackTime = Time.time;
    }

    protected void FinishReload()
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
            // if (!isReloading && currentAmmo < maxAmmo && Time.time - lastAttackTime >= idleReloadDelay)
            // {
            //     StartAutoReload();
            // }
        }
    }

    protected void UpdateAmmoUI()
    {
        if (ammoDisplay != null)
        {
            ammoDisplay.UpdateAmmoIcons(currentAmmo, maxAmmo);
            ammoDisplay.SetReloading(currentAmmo == 0 && !isReloading);
        }
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
}
