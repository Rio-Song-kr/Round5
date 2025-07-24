using System.Collections;
using UnityEngine;

public abstract class BaseWeapon : MonoBehaviour, IWeapon
{
    public int maxAmmo; // 최대 탄약 수
    public Animator animator; // 재장전 애니메이션용 애니메이터

    public int currentAmmo; // 남아 있는 탄약 수
    public bool isReloading; // 재장전 여부
    public float lastAttackTime; // 마지막으로 공격한 시간 (탄창 남아있는데 공격하지 않았을 때 자동 재장전 감지용)
    public float idleReloadDelay; // 공격하지 않았을 때 자동 재장전까지의 대기 시간
    public Coroutine idleCheckCoroutine; // 자동 재장전 감지를 위한 코루틴 핸들
    public Coroutine autoReloadCoroutine; // 리로드 애니메이션과 함께 실행되는 자동 재장전 코루틴 핸들
    public float reloadTime; // 재장전 애니메이션 및 동작에 걸리는 시간 (초)
    public AmmoDisplay ammoDisplay; // 탄약 UI를 표시하는 컴포넌트

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
