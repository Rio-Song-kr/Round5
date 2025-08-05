using System.Collections;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class ShieldEffect : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private GameObject _shieldObject;

    public ShieldSkillDataSO SkillData;
    private PlayerStatus _status;
    private Transform _playerTransform;
    private PlayerController _myPlayer;

    private float _shieldActiveTime;
    private float _shieldTimeCount;
    private bool _shieldEffectActivated;

    private bool _isStarted;

    private Vector3 _networkPosition;

    private void Update()
    {
        if (!photonView.IsMine) return;

        if (_shieldEffectActivated)
        {
            _shieldTimeCount += Time.deltaTime;

            if (_shieldTimeCount >= _shieldActiveTime)
                photonView.RPC(nameof(DisableShieldEffect), RpcTarget.All);
        }
    }

    /// <summary>
    /// effect의 위치 동기화
    /// </summary>
    private void LateUpdate()
    {
        if (!_isStarted) return;

        transform.position = _playerTransform.position;
    }

    /// <summary>
    /// 효과 초기화 로직 수행
    /// </summary>
    /// <param name="skillData">사용할 쉴드 스킬 데이터</param>
    /// <param name="viewId">플레이어의 Photon View ID</param>
    public void Initialize(ShieldSkillDataSO skillData, int viewId)
    {
        SkillData = skillData;

        if (!photonView.IsMine) return;

        photonView.RPC(nameof(UseShieldEffect), RpcTarget.All, viewId);
    }

    /// <summary>
    /// Shield 효과 활성화 RPC 메서드
    /// </summary>
    /// <param name="viewId">활성화할 플레이어의 Photon View ID</param>
    [PunRPC]
    private void UseShieldEffect(int viewId)
    {
        _status = PhotonView.Find(viewId).GetComponent<PlayerStatus>();
        _playerTransform = _status.transform;
        _myPlayer = _status.GetComponent<PlayerController>();
        _isStarted = true;

        foreach (var status in SkillData.Status)
        {
            if (status.EffectType != StatusEffectType.Invincibility) continue;

            _status.ApplyStatusEffect(status.EffectType, status.EffectValue, status.Duration);

            _shieldActiveTime = status.Duration;
            _shieldEffectActivated = true;
        }
        _shieldObject.GetComponent<ShieldEffectController>().Init(SkillData.ShieldScaleMultiplier, SkillData.ShieldScaleDuration);
        _shieldObject.SetActive(true);
    }

    /// <summary>
    /// Shield 효과 비활성화 RPC 메서드 호출
    /// </summary>
    [PunRPC]
    private void DisableShieldEffect()
    {
        _status.RemoveStatusEffect(StatusEffectType.Invincibility);
        _shieldTimeCount = 0f;
        _shieldEffectActivated = false;

        if (photonView.IsMine)
        {
            StartCoroutine(FadeOutAndInactive());
        }
    }

    /// <summary>
    /// 활성화된 방패 효과를 점차적으로 비활성화하고 비활성 상태로 전환하는 코루틴 메서드
    /// </summary>
    private IEnumerator FadeOutAndInactive()
    {
        yield return null;

        float elapsedTime = 0f;
        var shieldImage = _shieldObject.GetComponent<Image>();
        var originalColor = shieldImage.color;

        while (elapsedTime < SkillData.FadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(shieldImage.color.a, 0f, elapsedTime / SkillData.FadeDuration);
            var newColor = new Color(shieldImage.color.r, shieldImage.color.g, shieldImage.color.b, alpha);
            shieldImage.color = newColor;
            yield return null;
        }

        _shieldObject.SetActive(false);
        shieldImage.color = originalColor;

        _isStarted = false;
        PhotonNetwork.Destroy(gameObject);
    }

    /// <summary>
    /// Photon 네트워크 뷰의 데이터 직렬화 및 전송 핸들러 메서드
    /// </summary>
    /// <param name="stream">데이터를 전송하거나 수신하기 위한 Photon 스트림</param>
    /// <param name="info">메시지 정보</param>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
            stream.SendNext(transform.position);
        else
            _networkPosition = (Vector3)stream.ReceiveNext();
    }
}