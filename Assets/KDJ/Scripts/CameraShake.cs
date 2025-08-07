using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class CameraShake : MonoBehaviourPun
{
    //
    //NOTES:
    //This script is used for DEMONSTRATION porpuses of the Projectiles. I recommend everyone to create their own code for their own projects.
    //This is just a basic example.
    //

#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.

    [Header("진동 감소 시간 배수 (진동이 줄어드는 속도, 높을수록 진동이 빨리 줄어듭니다)")]
    [SerializeField] private float _shakeTimeMultiplier = 2f;
    [Header("다음 진동까지 지연 시간")]
    [SerializeField] private float _shakeDelay = 0.01f;
    private bool isRunning = false;
    public static CameraShake Instance { get; private set; }

    public Camera Cam { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Cam == null && Camera.main != null)
        {
            Cam = Camera.main;
        }
    }

    void Start()
    {

    }

    public void ShakeCamera()
    {
        ShakeCaller(0.2f, 0.05f);
    }

    // 호출용 함수
    /// <summary>
    /// 카메라 진동을 호출하는 함수입니다.
    /// </summary>
    /// <param name="amount">진동의 강도.</param>
    /// <param name="duration">진동 지속 시간.</param>
    public void ShakeCaller(float amount, float duration)
    {
        StartCoroutine(Shake(amount, duration));
    }

    // 핵심 카메라 진동 로직
    IEnumerator Shake(float amount, float duration)
    {
        isRunning = true;

        int counter = 0;

        while (duration > 0.01f)
        {
            counter++;
            // 첫 진동 이후 점점 진동이 줄어드는 로직
            var x = Random.Range(-1f, 1f) * (amount / counter);
            var y = Random.Range(-1f, 1f) * (amount / counter);

            Cam.transform.localPosition = Vector3.Lerp(Cam.transform.localPosition, new Vector3(x, y, -10), 0.5f);

            duration -= _shakeTimeMultiplier * Time.deltaTime;
            // 진동 간격을 두기 위한 지연 시간
            yield return new WaitForSeconds(_shakeDelay);
        }

        Cam.transform.localPosition = new Vector3(0, 0, -10f);

        isRunning = false;
    }
}
