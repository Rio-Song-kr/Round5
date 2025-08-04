using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class WinnerImageFillingUI : MonoBehaviourPun
{
    private Image fillImage;

    private void Awake()
    {
        fillImage = GetComponent<Image>();
        fillImage.fillAmount = 0;
    }

    [PunRPC]
    public void FillAmountInit(float value)
    {
        fillImage.fillAmount = value;
    }
}