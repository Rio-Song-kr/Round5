using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class CircleFillController : MonoBehaviourPun, IPunObservable
{
    [Header("Circle UI")]
    //# 카운트다운 게이지의 채워지는 원형 이미지
    [SerializeField] private Image _filledCircleImg;
    //# 카운트다운 게이지의 움직이는 원형 이미지
    [SerializeField] private Image _movingCircleImg;
    //# 카운트다운 게이지의 움직이는 라인 이미지
    [SerializeField] private Image _movingLineImg;

    //# 현재 게이지의 채움 정도 (0~1)
    public float CurrentFillAmount = 0f;
    //# 목표로 하는 게이지의 채움 정도 (0~1)
    private float _targetFillAmount = 1f;

    //# 현재 게이지가 증가 모드인지 여부 (true: 증가, false: 감소)
    public bool CanIncrese = true;
    public bool StartEffect = false;
    private bool _applyEffect = false;

    private float _activateTime;
    private float _chargeTime;

    public bool PlayerMoved;

    public void Initialize(float activateTime, float chargeTime)
    {
        _activateTime = activateTime;
        _chargeTime = chargeTime;
    }

    private void Update()
    {
        //# 증가 중에 플레이어 이동 시 감소로 전환
        if (!_applyEffect && CanIncrese && PlayerMoved)
        {
            CanIncrese = false;
            _targetFillAmount = 0f;
            if (photonView.IsMine)
                photonView.RPC(nameof(SetFillAmount), RpcTarget.All, CurrentFillAmount);
        }
        //todo 증가 -> 감소 변환 + 플레이어가 이동 X 일 때도 계속 적용되는 문제
        else if (!_applyEffect && !CanIncrese && !PlayerMoved)
        {
            CanIncrese = true;
            _targetFillAmount = 1f;
            photonView.RPC(nameof(SetFillAmount), RpcTarget.All, CurrentFillAmount);
        }

        //# 현재 채움량을 목표값으로 부드럽게 이동
        if (CurrentFillAmount > _targetFillAmount) CurrentFillAmount -= Time.deltaTime / _activateTime;
        else if (CurrentFillAmount < _targetFillAmount) CurrentFillAmount += Time.deltaTime / _chargeTime;

        //# 증가 모드에서 게이지가 거의 완전히 채워졌을 때의 처리
        if (CanIncrese && Mathf.Abs(CurrentFillAmount - 1f) < 0.01f)
        {
            //# 게이지를 완전히 채우고 감소 모드로 전환
            CurrentFillAmount = 1f;
            _targetFillAmount = 0f;
            CanIncrese = false;
            StartEffect = true;
            _applyEffect = true;
        }
        //# 감소 모드에서 게이지가 거의 완전히 비워졌을 때의 처리
        else if (!CanIncrese && Mathf.Abs(CurrentFillAmount) < 0.01f)
        {
            //# 게이지를 완전히 비우고 증가 모드로 전환
            CurrentFillAmount = 0f;
            _targetFillAmount = 1f;
            CanIncrese = true;
            StartEffect = true;
            _applyEffect = false;
        }

        //# 계산된 채움량을 UI 요소들에 적용
        SetFillAmount(CurrentFillAmount);
    }

    /// <summary>
    /// 계산된 채움량을 각 UI 이미지 요소에 적용하여 카운트다운 효과를 시각화
    /// </summary>
    /// <param name="fillAmount">적용할 채움량 (0~1 범위)</param>
    [PunRPC]
    public void SetFillAmount(float fillAmount)
    {
        //# 채워지는 원형 이미지와 움직이는 원형 이미지의 fillAmount 설정
        _filledCircleImg.fillAmount = fillAmount;
        _movingCircleImg.fillAmount = fillAmount;

        //# 움직이는 라인 이미지를 채움량에 따라 회전 (360도 기준)
        _movingLineImg.transform.rotation = Quaternion.Euler(0f, 0f, -fillAmount * 360f);
    }

    //# 테스트용
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
            stream.SendNext(PlayerMoved);
        else
            PlayerMoved = (bool)stream.ReceiveNext();
    }

    public void OnPlayerMoveChanged(bool value) => PlayerMoved = value;
}