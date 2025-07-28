using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CardSelectManager : MonoBehaviourPunCallbacks
{
    [Header("전체 카드 프리팹 리스트")]
    public List<GameObject> allCardPrefabs; // 게임에서 사용할 전체 카드 프리팹 목록

    [Header("부모 레이아웃 그룹")]
    public Transform cardSpawnParent; // 생성된 카드가 붙을 부모(캔버스 내 위치 컨테이너)

    [Header("출력할 카드 개수")]
    public int cardCountToShow = 3; // 한 번에 보여줄 카드 개수

    [Header("부채꼴 배치 설정")]
    public float xSpacing = 300f; // 카드 간 X 간격 고정값
    public float curveHeight = 150f; // Y 위치를 곡선처럼 주기 위한 값
    public float maxAngle = 60f; // 회전 시각 연출
    public float appearYOffset = -600f;


    private List<GameObject> currentCards = new(); // 현재 화면에 표시 중인 카드 목록
    private bool hasSelected = false; // 플레이어가 카드를 선택했는지 여부

    void Start()
    {
        SpawnRandomCards(); // 시작 시 카드들을 랜덤하게 생성
        SceneLoadingManager.Instance.LoadSceneAsync("Game Scene");
        Debug.Log("게임 씬 으로 넘어가기 위해 로딩 진행");
    }

    // 랜덤 카드 생성 및 화면에 출력
    public void SpawnRandomCards()
    {
        if (allCardPrefabs.Count < cardCountToShow)
        {
            Debug.LogError("카드 프리팹이 부족합니다.");
            return;
        }

        // 랜덤 카드 선택
        List<int> selectedIndexes = new();
        while (selectedIndexes.Count < cardCountToShow)
        {
            int rand = Random.Range(0, allCardPrefabs.Count);
            if (!selectedIndexes.Contains(rand))
                selectedIndexes.Add(rand);
        }

        float centerIndex = (cardCountToShow - 1) / 2f;

        for (int i = 0; i < cardCountToShow; i++)
        {
            GameObject card = Instantiate(allCardPrefabs[selectedIndexes[i]], cardSpawnParent);
            RectTransform rt = card.GetComponent<RectTransform>();
            CanvasGroup cg = card.GetComponent<CanvasGroup>();
            if (cg == null) cg = card.AddComponent<CanvasGroup>();

            float offset = i - centerIndex;

            //  X 좌표는 등간격으로 고정
            float x = offset * xSpacing;

            //  Y는 부드러운 곡선을 따라 위로 살짝
            float y = -Mathf.Abs(offset) * curveHeight + curveHeight;

            //  회전도 각도만큼 부채꼴처럼 부여
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
                flip.SetManager(this);

            currentCards.Add(card);
        }
    }

    // 카드 하나가 선택되었을 때 호출됨
    public void OnCardSelected(GameObject selected)
    {
        if (hasSelected) return; // 이미 선택했다면 무시
        hasSelected = true;

        

        // 카드 효과 적용
        CardEffect effect = selected.GetComponent<CardEffect>();
        if ( effect != null)
        {
            GameObject player = GameObject.FindWithTag("Player");

            if (player != null)
            {
                effect.ApplyEffect(player);
            }
        }

        foreach (GameObject card in currentCards)
        {
            if (card == null) continue;

            if (card == selected)
            {
                // 선택된 카드 → 페이드아웃
                CanvasGroup cg = card.GetComponent<CanvasGroup>();
                if (cg == null) cg = card.AddComponent<CanvasGroup>();

                cg.DOFade(0f, 0.5f)
                  .SetEase(Ease.InOutSine)
                  .OnComplete(() => Destroy(card)); // 애니메이션 끝나면 삭제
            }
            else
            {
                // 선택되지 않은 카드 → 회전 방향으로 이동 + 축소 + 페이드아웃
                RectTransform rt = card.GetComponent<RectTransform>();
                CanvasGroup cg = card.GetComponent<CanvasGroup>();
                if (cg == null) cg = card.AddComponent<CanvasGroup>();

                float angleZ = rt.localEulerAngles.z; // rotZ
                Vector2 direction = Quaternion.Euler(0, 0, angleZ) * Vector2.up; // 회전 방향 기준 위쪽
                Vector2 targetPos = rt.anchoredPosition + direction * 400f;

                Sequence seq = DOTween.Sequence();
                seq.Join(rt.DOAnchorPos(targetPos, 2f).SetEase(Ease.InCubic)); // 날아가듯 이동
                seq.Join(rt.DOScale(0.1f, 2f).SetEase(Ease.InCubic));           // 작아지기
                seq.Join(cg.DOFade(0f, 2f));
                seq.Join(rt.DOLocalRotate(new Vector3(180f, 180f, angleZ), 2f, RotateMode.FastBeyond360));
                seq.OnComplete(() => Destroy(card));
            }
        }

        Debug.Log("선택된 카드: " + selected.name);
        Debug.Log("게임 씬 으로 넘어가기 위해 로딩 진행");


        // DOVirtual.DelayedCall(2f, () => SceneLoadingManager.Instance.AllowSceneActivation());

    }

    
}
