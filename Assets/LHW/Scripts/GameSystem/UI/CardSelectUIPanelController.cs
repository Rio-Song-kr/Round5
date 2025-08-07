using Photon.Pun;

public class CardSelectUIPanelController : MonoBehaviourPun
{
    [PunRPC]
    public void CardSelectUIActivate(bool activation)
    {
        gameObject.SetActive(activation);
    }
}