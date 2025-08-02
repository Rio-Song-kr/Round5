using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using LHWtestScript;
using UnityEngine;

public class CardSelectManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private CanvasController canvasController;

    [SerializeField] private CardSelectCheckManager cardSelectCheckManager;

    [Header("��ü ī�� ������ ����Ʈ")]
    public List<GameObject> allCardPrefabs; // ���ӿ��� ����� ��ü ī�� ������ ���

    [Header("�θ� ���̾ƿ� �׷�")]
    public Transform cardSpawnParent1; // ������ ī�尡 ���� �θ�(ĵ���� �� ��ġ �����̳�)
    public Transform cardSpawnParent2;

    [Header("����� ī�� ����")]
    public int cardCountToShow = 3; // �� ���� ������ ī�� ����

    [Header("��ä�� ��ġ ����")]
    public float xSpacing = 300f; // ī�� �� X ���� ������
    public float curveHeight = 150f; // Y ��ġ�� �ó�� �ֱ� ���� ��
    public float maxAngle = 60f; // ȸ�� �ð� ����
    public float appearYOffset = -600f;


    private List<GameObject> currentCards = new(); // ���� ȭ�鿡 ǥ�� ���� ī�� ���
    [SerializeField] private bool hasSelect = false; // �÷��̾ ī�带 �����ߴ��� ����

    void Start()
    {

        cardSelectCheckManager.cardSelectPanelSpawn();
        cardSelectCheckManager.CardSelectPanelSpawn(PhotonNetwork.LocalPlayer);
        // SceneLoadingManager.Instance.LoadSceneAsync("Game Scene");
        // Debug.Log("���� �� ���� �Ѿ�� ���� �ε� ����");

        if (PhotonNetwork.IsMasterClient)
        {
            List<int> selectedMasterIndexes = GetRandomCardIndexes();
            photonView.RPC(nameof(RPC_SpawnCardsWithIndexes), RpcTarget.All, selectedMasterIndexes.ToArray());
        }
        
    }

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // ���� ī�� ���� �� ȭ�鿡 ���
    public List<int> GetRandomCardIndexes()
    {
        List<int> indexes = new();
        while (indexes.Count < cardCountToShow)
        {
            int rand = Random.Range(0, allCardPrefabs.Count);
            if (!indexes.Contains(rand))
                indexes.Add(rand);
        }
        return indexes;
    }

    [PunRPC]
    public void RPC_SpawnCardsWithIndexes(int[] indexes)
    {
        SpawnCardsFromIndexes(indexes, PhotonNetwork.IsMasterClient);
    }

    public void SpawnCardsFromIndexes(int[] indexes, bool canInteract)
    {
        Debug.Log("ī�� ���� ����");
        currentCards.Clear();

        float centerIndex = (indexes.Length - 1) / 2f;

        for (int i = 0; i < indexes.Length; i++)
        {
            Debug.Log("�� �� ������ ������");
            GameObject card = Instantiate(allCardPrefabs[indexes[i]], cardSpawnParent1);
            RectTransform rt = card.GetComponent<RectTransform>();
            CanvasGroup cg = card.GetComponent<CanvasGroup>();
            if (cg == null) cg = card.AddComponent<CanvasGroup>();

            float offset = i - centerIndex;
            float x = offset * xSpacing;
            float y = -Mathf.Abs(offset) * curveHeight + curveHeight;
            float rotZ = offset * 5f;

            if (rt != null)
            {
                rt.anchoredPosition = new Vector2(x, appearYOffset);
                rt.localRotation = Quaternion.Euler(0, 0, rotZ);
                cg.alpha = 0f;

                Sequence seq = DOTween.Sequence();
                seq.Append(rt.DOAnchorPos(new Vector2(x, y), 0.6f).SetEase(Ease.OutCubic));
                seq.Join(cg.DOFade(1f, 0.6f));
            }

            LHWFlipCard flip = card.GetComponent<LHWFlipCard>();
            if (flip != null)
            {
                flip.SetManager(this);
                flip.SetCardIndex(i); // �ε��� ��� ����ȭ��
                flip.SetInteractable(canInteract);
            }

            currentCards.Add(card);
        }

    }

    public void SpawnClientCardsFromIndexes(int[] indexes, bool canInteract)
    {
        Debug.Log("ī�� ���� ����");
        currentCards.Clear();

        float centerIndex = (indexes.Length - 1) / 2f;

        for (int i = 0; i < indexes.Length; i++)
        {
            Debug.Log("�� �� ������ ������");
            GameObject card = Instantiate(allCardPrefabs[indexes[i]], cardSpawnParent2);
            RectTransform rt = card.GetComponent<RectTransform>();
            CanvasGroup cg = card.GetComponent<CanvasGroup>();
            if (cg == null) cg = card.AddComponent<CanvasGroup>();

            float offset = i - centerIndex;
            float x = offset * xSpacing;
            float y = -Mathf.Abs(offset) * curveHeight + curveHeight;
            float rotZ = offset * 5f;

            if (rt != null)
            {
                rt.anchoredPosition = new Vector2(x, appearYOffset);
                rt.localRotation = Quaternion.Euler(0, 0, rotZ);
                cg.alpha = 0f;

                Sequence seq = DOTween.Sequence();
                seq.Append(rt.DOAnchorPos(new Vector2(x, y), 0.6f).SetEase(Ease.OutCubic));
                seq.Join(cg.DOFade(1f, 0.6f));
            }

            LHWFlipCard flip = card.GetComponent<LHWFlipCard>();
            if (flip != null)
            {
                flip.SetManager(this);
                flip.SetCardIndex(i); // �ε��� ��� ����ȭ��
                flip.SetInteractable(canInteract);
            }

            currentCards.Add(card);
        }

    }

    public List<GameObject> GetCurrentCards() => currentCards;

    [PunRPC]
    public void RPC_FlipCardByIndex(int index)
    {
        if (index >= 0 && index < currentCards.Count)
        {
            LHWFlipCard flip = currentCards[index].GetComponent<LHWFlipCard>();
            if (flip != null)
            {
                flip.PlayFlipAnimation();
            }
        }
    }
   

    // ī�� �ϳ��� ���õǾ��� �� ȣ���
    public void OnCardSelected(GameObject selected)
    {

        if (hasSelect == true)
        {
            return;
        }


        hasSelect = true;

        Debug.Log("�� ī�� ���� �Ϸ��");

        if (cardSelectCheckManager.cardSelectPanels.TryGetValue(PhotonNetwork.LocalPlayer.ActorNumber, out CardSelectPanelItem panel))
        {
            panel.OnCardSelected();

            panel.SelectCheck(PhotonNetwork.LocalPlayer);
        }

        //  // ī�� ȿ�� ����
        //  CardEffect effect = selected.GetComponent<CardEffect>();
        //  if ( effect != null)
        //  {
        //      Player player = GameObject.FindWithTag("Player");
        //
        //      if (player != null)
        //      {
        //          effect.ApplyEffect(player);
        //      }
        //  }

        foreach (GameObject card in currentCards)
        {
            if (card == null) continue;

            if (card == selected)
            {
                // ���õ� ī�� �� ���̵�ƿ�
                CanvasGroup cg = card.GetComponent<CanvasGroup>();
                if (cg == null) cg = card.AddComponent<CanvasGroup>();

                cg.DOFade(0f, 0.5f)
                  .SetEase(Ease.InOutSine)
                  .OnComplete(() => Destroy(card)); // �ִϸ��̼� ������ ����
            }
            else
            {
                // ���õ��� ���� ī�� �� ȸ�� �������� �̵� + ��� + ���̵�ƿ�
                RectTransform rt = card.GetComponent<RectTransform>();
                CanvasGroup cg = card.GetComponent<CanvasGroup>();
                if (cg == null) cg = card.AddComponent<CanvasGroup>();

                float angleZ = rt.localEulerAngles.z; // rotZ
                Vector2 direction = Quaternion.Euler(0, 0, angleZ) * Vector2.up; // ȸ�� ���� ���� ����
                Vector2 targetPos = rt.anchoredPosition + direction * 400f;

                Sequence seq = DOTween.Sequence();
                seq.Join(rt.DOAnchorPos(targetPos, 2f).SetEase(Ease.InCubic)); // ���ư��� �̵�
                seq.Join(rt.DOScale(0.1f, 2f).SetEase(Ease.InCubic));           // �۾�����
                seq.Join(cg.DOFade(0f, 2f));
                seq.Join(rt.DOLocalRotate(new Vector3(180f, 180f, angleZ), 2f, RotateMode.FastBeyond360));
                seq.OnComplete(() => Destroy(card));
            }
        }

        Debug.Log("���õ� ī��: " + selected.name);
        Debug.Log("���� �� ���� �Ѿ�� ���� �ε� ����");

        if (PhotonNetwork.IsMasterClient)
        {
            canvasController.photonView.RPC("RPC_SwitchToClientCanvas", RpcTarget.All);
        }



        // DOVirtual.DelayedCall(2f, () => SceneLoadingManager.Instance.AllowSceneActivation());
        // DOVirtual.DelayedCall(2f, () => PhotonNetwork.LoadLevel("Game Scene"));
    }

    public override void OnPlayerPropertiesUpdate(Player target, ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        base.OnPlayerPropertiesUpdate(target, propertiesThatChanged);

        if (propertiesThatChanged.ContainsKey("Select"))
        {
            cardSelectCheckManager.cardSelectPanels[target.ActorNumber].SelectCheck(target);

            if (PhotonNetwork.IsMasterClient && cardSelectCheckManager.AllPlayerCardSelectCheck() == true)
            {
                Debug.Log(" ��� �÷��̾� ī�� ���� �Ϸ� �� Game Scene �ε�");
                PhotonNetwork.LoadLevel("Game Scene");
                // �� ��ȯ �ʿ��� �κ�
            }
        }
    }




}
