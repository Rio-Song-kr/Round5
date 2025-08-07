using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SandBoxScene : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private float animationLength = 1f;

    public void OnMainButtonClick()
    {
        StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
    {
        anim.Play("Welcome Out");
        
        yield return new WaitForSeconds(animationLength);


        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
        SceneManager.LoadScene("KDJ_TestScene");
    }
}
