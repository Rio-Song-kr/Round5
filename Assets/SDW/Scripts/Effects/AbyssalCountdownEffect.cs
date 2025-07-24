using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 원형 카운트다운 이펙트를 제어하는 컨트롤러
/// </summary>
public class AbyssalCountdownEffect : MonoBehaviour
{
    [Header("Image UI")]
    // # 카운트다운 게이지의 채워지는 원형 이미지
    [SerializeField] private Image _filledCircleImg;
    // # 카운트다운 게이지의 움직이는 원형 이미지
    [SerializeField] private Image _movingCircleImg;
    // # 카운트다운 게이지의 움직이는 라인 이미지
    [SerializeField] private Image _movingLineImg;

    [Header("Effects Prefabs")]
    // # 카운트다운 완료 시 생성할 VFX 파티클 프리팹
    [SerializeField] private GameObject _vfxPrefab;
    // # 카운트다운 완료 시 활성화할 octagon 이펙트 오브젝트
    [SerializeField] private GameObject _octagonObject;
    // # 생성된 VFX 오브젝트의 파티클 시스템 컴포넌트
    private ParticleSystem _particle;
    // # 생성된 VFX 오브젝트의 참조
    private GameObject _vfxObject;

    [SerializeField]
    // # 카운트다운 게이지가 변화하는 속도
    private float _speed = 0.1f;
    // # 현재 게이지의 채움 정도 (0~1)
    private float _currentFillAmount = 0f;
    // # 목표로 하는 게이지의 채움 정도 (0~1)
    private float _targetFillAmount = 1f;
    // # 현재 게이지가 증가 모드인지 여부 (true: 증가, false: 감소)
    private bool _canIncrease = true;

    /// <summary>
    /// VFX 오브젝트를 생성하고 초기 상태를 설정
    /// </summary>
    private void Start()
    {
        // # VFX 프리팹을 현재 위치에 생성하여 자식으로 설정
        _vfxObject = Instantiate(_vfxPrefab, transform.position, transform.rotation, transform);

        // # 생성된 VFX 오브젝트에서 파티클 시스템 컴포넌트를 가져옴
        _particle = _vfxObject.GetComponentInChildren<ParticleSystem>();

        // # 파티클을 중지하고 기존 파티클들을 제거
        _particle.Stop();
        _particle.Clear();

        // # octagon 이펙트는 초기에 비활성화
        _octagonObject.SetActive(false);
    }

    /// <summary>
    /// 카운트다운 게이지를 업데이트하고 이펙트 상태를 관리
    /// </summary>
    private void Update()
    {
        // # 현재 채움량을 목표값으로 부드럽게 이동
        if (_currentFillAmount > _targetFillAmount) _currentFillAmount -= Time.deltaTime * _speed;
        else if (_currentFillAmount < _targetFillAmount) _currentFillAmount += Time.deltaTime * _speed;

        // # 증가 모드에서 게이지가 거의 완전히 채워졌을 때의 처리
        if (_canIncrease && Mathf.Abs(_currentFillAmount - 1f) < 0.01f)
        {
            // # 게이지를 완전히 채우고 감소 모드로 전환
            _currentFillAmount = 1f;
            _targetFillAmount = 0f;
            _canIncrease = false;

            StartEffect();
        }
        // # 감소 모드에서 게이지가 거의 완전히 비워졌을 때의 처리
        else if (!_canIncrease && Mathf.Abs(_currentFillAmount) < 0.01f)
        {
            // # 게이지를 완전히 비우고 증가 모드로 전환
            _currentFillAmount = 0f;
            _targetFillAmount = 1f;
            _canIncrease = true;

            EndEffect();
        }

        // # 계산된 채움량을 UI 요소들에 적용
        SetFillAmount(_currentFillAmount);
    }

    /// <summary>
    /// 계산된 채움량을 각 UI 이미지 요소에 적용하여 카운트다운 효과를 시각화
    /// </summary>
    /// <param name="fillAmount">적용할 채움량 (0~1 범위)</param>
    public void SetFillAmount(float fillAmount)
    {
        // # 채워지는 원형 이미지와 움직이는 원형 이미지의 fillAmount 설정
        _filledCircleImg.fillAmount = fillAmount;
        _movingCircleImg.fillAmount = fillAmount;

        // # 움직이는 라인 이미지를 채움량에 따라 회전 (360도 기준)
        _movingLineImg.transform.rotation = Quaternion.Euler(0f, 0f, -fillAmount * 360f);
    }

    public void StartEffect()
    {
            // # 파티클 효과 시작 및 octagon 이펙트 활성화
            _particle.Clear();
            _particle.Play();
            _octagonObject.SetActive(true);
    }

    public void EndEffect()
    {
            // # 파티클 효과 중지 및 octagon 이펙트 비활성화
            _particle.Stop();
            _octagonObject.SetActive(false);
    }
}