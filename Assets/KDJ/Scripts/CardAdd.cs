using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CardAdd : MonoBehaviourPunCallbacks

{
    [SerializeField] private AttackCard _barrageCard;
    [SerializeField] private AttackCard _laserCard;
    [SerializeField] private AttackCard _explosiveCard;
    [SerializeField] private AttackCard _bigCard;

    void Start()
    {
        InGameManager.Instance.SetStartedOffline(true);
    }

    private void Update()
    {
        AddCardInList();
    }

    private void AddCardInList()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            CardManager.Instance.ClearLists();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            CardManager.Instance.ClearLists();
            CardManager.Instance.AddCard(_barrageCard);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            CardManager.Instance.ClearLists();
            CardManager.Instance.AddCard(_laserCard);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            CardManager.Instance.ClearLists();
            CardManager.Instance.AddCard(_explosiveCard);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            CardManager.Instance.ClearLists();
            CardManager.Instance.AddCard(_bigCard);
        }

    }
}
