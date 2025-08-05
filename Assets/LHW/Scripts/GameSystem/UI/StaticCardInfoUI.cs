using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticCardInfoUI : MonoBehaviour
{
    [SerializeField] GameObject UIImagePrefab;

    [SerializeField] Transform leftPanelContent;
    [SerializeField] Transform rightPanelContent;
    [SerializeField] List<GameObject> leftUIImage = new List<GameObject>();
    [SerializeField] List<GameObject> rightUIImage = new List<GameObject>();

    string[] leftCardList;
    string[] rightCardList;

    PhotonView leftPanelView;
    PhotonView rightPanelView;

    int leftPanelViewID;
    int rightPanelViewID;

    private void Awake()
    {
        leftPanelView = leftPanelContent.GetComponent<PhotonView>();
        rightPanelView = rightPanelContent.GetComponent<PhotonView>();

        leftPanelViewID = leftPanelView.ViewID;
        rightPanelViewID = rightPanelView.ViewID;
    }

    private void OnEnable()
    {
        TestIngameManager.OnSkillObtained += ShowCardList;
    }

    private void OnDisable()
    {
        TestIngameManager.OnSkillObtained -= ShowCardList;
    }

    private void ShowCardList()
    {
        leftCardList = TestIngameManager.Instance.GetSkillInfo("Left");
        rightCardList = TestIngameManager.Instance.GetSkillInfo("Right");

        if(leftCardList.Length > leftUIImage.Count)
        {
            for(int i = leftUIImage.Count; i < leftCardList.Length; i++)
            {
                //GameObject skillInfo = Instantiate(UIImagePrefab);
                //skillInfo.transform.SetParent(leftPanelContent);
                GameObject skillInfo = PhotonNetwork.Instantiate(UIImagePrefab.name, transform.position, Quaternion.identity);
                PhotonView skillInfoView = skillInfo.GetComponent<PhotonView>();
                skillInfoView.RPC(nameof(SkillInfoUI.SetParentToPanel), RpcTarget.All, leftPanelViewID);
                // TODO : 카드 정보 입력
                leftUIImage.Add(skillInfo);
            }
        }

        if(rightCardList.Length > rightUIImage.Count)
        {
            for (int i = rightUIImage.Count; i < rightCardList.Length; i++)
            {
                //GameObject skillInfo = Instantiate(UIImagePrefab);
                //skillInfo.transform.SetParent(rightPanelContent);
                GameObject skillInfo = PhotonNetwork.Instantiate(UIImagePrefab.name, transform.position, Quaternion.identity);
                PhotonView skillInfoView = skillInfo.GetComponent<PhotonView>();
                skillInfoView.RPC(nameof(SkillInfoUI.SetParentToPanel), RpcTarget.All, rightPanelViewID);
                // TODO : 카드 정보 입력
                rightUIImage.Add(skillInfo);
            }
        }
    }
}