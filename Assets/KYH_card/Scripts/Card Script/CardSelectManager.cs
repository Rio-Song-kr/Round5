using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class CardSelectManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private CanvasController canvasController;
    [SerializeField] private GameObject canvasActivation;
    [SerializeField] private CardSelectCheckManager cardSelectCheckManager;
    [SerializeField] private CardSelectPanelItem cardSelectPanelItem;
    FlipCard flipCard;

    [Header("전체 카드 프리팹 리스트")]
    public List<GameObject> allCardPrefabs; // 게임에서 사용할 전체 카드 프리팹 목록

    [Header("부모 레이아웃 그룹")]
    public Transform cardSpawnParent1; // 생성된 카드가 붙을 부모(캔버스 내 위치 컨테이너)
    public Transform cardSpawnParent2;

    [Header("출력할 카드 개수")]
    public int cardCountToShow = 3; // 한 번에 보여줄 카드 개수

    [Header("부채꼴 배치 설정")]
    public float xSpacing = 300f; // 카드 간 X 간격 고정값
    public float curveHeight = 150f; // Y 위치를 곡선처럼 주기 위한 값
    public float maxAngle = 60f; // 회전 시각 연출
    public float appearYOffset = -600f;

    private CardSceneArmController armController;

    [SerializeField] private GameObject masterCharacter;
    [SerializeField] private GameObject clientCharacter;

    [SerializeField] private CardSceneArmController masterArmController;
    [SerializeField] private CardSceneArmController clientArmController;

    [SerializeField] private CharacterShrinkEffect masterShrinkEffect;
    [SerializeField] private CharacterShrinkEffect clientShrinkEffect;

    [Header("케릭터 크기 조절")]
    [SerializeField] private CharacterShrinkEffect shrinkEffect;


    private List<GameObject> currentCards = new(); // 현재 화면에 표시 중인 카드 목록
    [SerializeField] private bool hasSelect = false; // 플레이어가 카드를 선택했는지 여부


    void Start()
    {
        

        cardSelectCheckManager.cardSelectPanelSpawn();
        cardSelectCheckManager.CardSelectPanelSpawn(PhotonNetwork.LocalPlayer);

        UpdateCharacterVisibility();
        // SceneLoadingManager.Instance.LoadSceneAsync("Game Scene");
        // Debug.Log("게임 씬 으로 넘어가기 위해 로딩 진행");

        // if (PhotonNetwork.IsMasterClient)
        // {
        //     List<int> selectedMasterIndexes = GetRandomCardIndexes();
        //     photonView.RPC(nameof(RPC_SpawnCardsWithIndexes), RpcTarget.All, selectedMasterIndexes.ToArray());
        // }

    }
    public void UpdateCharacterVisibility()
    {
        bool isMaster = PhotonNetwork.IsMasterClient;
        bool masterCanvasActive = canvasController.IsMasterCanvasActive();
        bool clientCanvasActive = canvasController.IsClientCanvasActive();

        if (masterCanvasActive)
        {
            if (canvasController.IsMyTurn())
            {
                if (isMaster) ActivateMasterCharacter();     // 마스터 → 선택자
                else ActivateClientCharacter();              // 참가자 → 선택자
            }
            else
            {
                if (isMaster) ActivateClientCharacter();     // 마스터 → 관전자
                else ActivateMasterCharacter();              // 참가자 → 관전자
            }
        }
        else if (clientCanvasActive)
        {
            if (canvasController.IsMyTurn())
            {
                if (isMaster) ActivateClientCharacter();     // 마스터 → 선택자
                else ActivateMasterCharacter();              // 참가자 → 선택자
            }
            else
            {
                if (isMaster) ActivateMasterCharacter();     // 마스터 → 관전자
                else ActivateClientCharacter();              // 참가자 → 관전자
            }
        }
        else
        {
            masterCharacter.SetActive(false);
            clientCharacter.SetActive(false);
        }
    }
      public void ActivateMasterCharacter()
      {
          masterCharacter.SetActive(true);
          clientCharacter.SetActive(false);
    
          armController = masterArmController;
          shrinkEffect = masterShrinkEffect;
      }
    
      public void ActivateClientCharacter()
      {
          masterCharacter.SetActive(false);
          clientCharacter.SetActive(true);
    
          armController = clientArmController;
          shrinkEffect = clientShrinkEffect;
      }

    // private void Awake()
    // {
    //     PhotonNetwork.AutomaticallySyncScene = true;
    // }

    public CardSceneArmController GetArmController()
    {
        return armController;
    }

    [PunRPC]
    public void RPC_SelectCardArm(int index)
    {
        Debug.Log($"[CardSelectManager] 셀렉트 카드 암 index = {index} 호출됨");
        armController.SelectCard(index);
    }

    // 랜덤 카드 생성 및 화면에 출력
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

   // [PunRPC]
   // public void RPC_SpawnCardsWithIndexes(int[] indexes)
   // {
   //     SpawnCardsFromIndexes(indexes, canvasController.IsMyTurn());
   // }

    public void SpawnCardsFromIndexes(int[] indexes, bool canInteract)
    {
      //  if (hasSelect)
      //  {
      //      Debug.Log("이미 카드 선택 완료 상태 → 카드 생성 스킵");
      //      return; // 선택이 끝났다면 카드 다시 띄우지 않음
      //  }

        Debug.Log("카드 생성 시작");
        currentCards.Clear();

        float centerIndex = (indexes.Length - 1) / 2f;

        for (int i = 0; i < indexes.Length; i++)
        {
            Debug.Log("포 문 안으로 들어왔음");
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

            FlipCard flip = card.GetComponent<FlipCard>();
            if (flip != null)
            {
                flip.SetManager(this);
                flip.SetCardIndex(i); // 인덱스 기반 동기화용
                flip.SetInteractable(canInteract);
            }

            currentCards.Add(card);
        }

    }

    public void SpawnClientCardsFromIndexes(int[] indexes, bool canInteract)
    {
       // if (hasSelect)
       // {
       //     Debug.Log("이미 카드 선택 완료 상태 → 카드 생성 스킵");
       //     return; // 선택이 끝났다면 카드 다시 띄우지 않음
       // }

        Debug.Log("카드 생성 시작");
        currentCards.Clear();

        float centerIndex = (indexes.Length - 1) / 2f;

        for (int i = 0; i < indexes.Length; i++)
        {
            Debug.Log("포 문 안으로 들어왔음");
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

            FlipCard flip = card.GetComponent<FlipCard>();
            if (flip != null)
            {
                flip.SetManager(this);
                flip.SetCardIndex(i); // 인덱스 기반 동기화용
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
            FlipCard flip = currentCards[index].GetComponent<FlipCard>();
            if (flip != null)
            {
                Debug.Log("카드 뒤집힘 애니메이션 실행");
                flip.PlayFlipAnimation();
            }
        }
    }

    [PunRPC]
    public void RPC_HighlightCardByIndex(int index)
    {
        if (index < 0 || index >= currentCards.Count) return;

        GameObject card = currentCards[index];
        if (card == null) return;

        FlipCard flip = card.GetComponent<FlipCard>();
        if (flip == null) return;

        flip.PlayHighlight(); // 확장된 연출용 메서드
    }

    [PunRPC]
    public void RPC_UnhighlightCardByIndex(int index)
    {
        if (index < 0 || index >= currentCards.Count) return;

        GameObject card = currentCards[index];
        if (card == null) return;

        FlipCard flip = card.GetComponent<FlipCard>();
        if (flip == null) return;

        flip.PlayUnhighlight();
    }


    // 카드 하나가 선택되었을 때 호출됨
    public void OnCardSelected(GameObject selected)
    {
        Debug.Log($"[OnCardSelected] called | hasSelect: {hasSelect}");
        if (hasSelect) return;

        hasSelect = true;

        Debug.Log("내 카드 선택 완료됨");

        PhotonNetwork.AutomaticallySyncScene = true;

        if (cardSelectCheckManager.cardSelectPanels.TryGetValue(PhotonNetwork.LocalPlayer.ActorNumber, out CardSelectPanelItem panel))
        {
            panel.OnCardSelected();
            panel.SelectCheck(PhotonNetwork.LocalPlayer);
        }

        // 카드 효과 적용
        CardEffect effect = selected.GetComponent<CardEffect>();
        if (effect != null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                PlayerStats playerStats = playerObj.GetComponent<PlayerStats>();
                if (playerStats != null)
                {
                    effect.ApplyShotEffect(playerStats);
                    effect.ApplyStatusEffect(playerStats);
                    Debug.Log($"[카드 적용] {effect.cardName} 효과가 적용되었습니다.");
                }
            }
        }

        // 선택된 카드 인덱스를 구해서 RPC 호출
        int selectedIndex = currentCards.IndexOf(selected);
        photonView.RPC(nameof(RPC_PlayCardSelectionAnimation), RpcTarget.All, selectedIndex);

        Debug.Log("선택된 카드: " + selected.name);
       // Debug.Log("게임 씬으로 넘어가기 위해 로딩 진행");

        if (canvasController.IsMyTurn())
        {
            DOVirtual.DelayedCall(1f, () =>
            {
                if (cardSelectCheckManager.AllPlayerCardSelectCheck())
                {
                    if (PhotonNetwork.IsMasterClient) // 마스터만 씬 전환
                    {
                        Debug.Log("모든 플레이어 선택 완료 → Game Scene 전환");
                        PhotonNetwork.LoadLevel("Game Scene");
                    }
                }
                else
                {
                    canvasController.photonView.RPC("RPC_SwitchTurnToOther", RpcTarget.All);
                }
            });
        }
    }

    [PunRPC]
    public void RPC_PlayCardSelectionAnimation(int selectedIndex)
    {
        for (int i = 0; i < currentCards.Count; i++)
        {
            GameObject card = currentCards[i];
            if (card == null) continue;

            RectTransform rt = card.GetComponent<RectTransform>();
            CanvasGroup cg = card.GetComponent<CanvasGroup>();
            if (cg == null) cg = card.AddComponent<CanvasGroup>();

            if (i == selectedIndex)
            {
                // 선택된 카드 → 페이드아웃 후 삭제
                cg.DOFade(0f, 0.5f)
                  .SetEase(Ease.InOutSine)
                  .OnComplete(() => Destroy(card));
            }
            else
            {
                // 나머지 카드 → 멀어지면서 축소 후 삭제
                float angleZ = rt.localEulerAngles.z;
                Vector2 direction = Quaternion.Euler(0, 0, angleZ) * Vector2.up;
                Vector2 targetPos = rt.anchoredPosition + direction * 400f;

                Sequence seq = DOTween.Sequence();
                seq.Join(rt.DOAnchorPos(targetPos, 1f).SetEase(Ease.InCubic));
                seq.Join(rt.DOScale(0.1f, 1f).SetEase(Ease.InCubic));
                seq.Join(cg.DOFade(0f, 1f));
                seq.Join(rt.DOLocalRotate(new Vector3(180f, 180f, angleZ), 1f, RotateMode.FastBeyond360));
                seq.OnComplete(() => Destroy(card));
            }
        }

        shrinkEffect.RequestShrinkEffect();
    }

     public override void OnPlayerPropertiesUpdate(Player target, ExitGames.Client.Photon.Hashtable propertiesThatChanged)
     {
         base.OnPlayerPropertiesUpdate(target, propertiesThatChanged);
    
         if (propertiesThatChanged.ContainsKey("Select"))
         {
             cardSelectCheckManager.cardSelectPanels[target.ActorNumber].SelectCheck(target);
    
             // 모든 플레이어 카드 선택 완료 → 게임 씬 전환 (2초 후)
             if (cardSelectCheckManager.AllPlayerCardSelectCheck())
             {
                 Debug.Log("모든 플레이어 카드 선택 완료 ");
    
                 DOVirtual.DelayedCall(1f, () =>
                 {
                     Debug.Log("라운드 종료됨 → 다음 카드 선택 준비");

                     ResetCardSelectionState();

                     

                     // 1. CanvasController 상태 리셋
                     canvasController.ResetCardSelectionState();

                     DOVirtual.DelayedCall(0.2f, () => {
                         canvasController.DecideNextSelector();
                     });


                 });
             }
         }
     }

    public bool HasSelected()
    {
        return hasSelect;
    }


    public void ResetCardSelectionState()
    {
        Debug.Log("카드선택상황 초기화");
        hasSelect = false;

        // 양쪽 Canvas의 자식 카드 오브젝트 제거
        foreach (Transform t in cardSpawnParent1)
            Destroy(t.gameObject);

        foreach (Transform t in cardSpawnParent2)
            Destroy(t.gameObject);

        ExitGames.Client.Photon.Hashtable props = new();
        props["Select"] = false;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        // 캐릭터 꺼두기
        masterCharacter.SetActive(false);
        clientCharacter.SetActive(false);

        foreach (var panel in cardSelectCheckManager.cardSelectPanels.Values)
        {
            panel.ResethasSelected();
        }
    }

}
