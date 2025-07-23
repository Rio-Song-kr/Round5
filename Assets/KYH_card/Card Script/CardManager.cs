using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSelectManager : MonoBehaviour
{
    [Header("전체 카드 프리팹 리스트")]
    public List<GameObject> allCardPrefabs; // 게임에서 사용할 전체 카드 프리팹 목록

    [Header("부모 레이아웃 그룹")]
    public Transform cardSpawnParent; // 생성된 카드가 붙을 부모(캔버스 내 위치 컨테이너)

    [Header("출력할 카드 개수")]
    [SerializeField] public int cardCountToShow = 3; // 한 번에 보여줄 카드 개수

    private List<GameObject> currentCards = new(); // 현재 화면에 표시 중인 카드 목록
    private bool hasSelected = false; // 플레이어가 카드를 선택했는지 여부

    void Start()
    {
        SpawnRandomCards(); // 시작 시 카드들을 랜덤하게 생성
    }

    // 랜덤 카드 생성 및 화면에 출력
    public void SpawnRandomCards()
    {
        if (allCardPrefabs.Count < cardCountToShow)
        {
            Debug.LogError("카드 프리팹이 부족합니다.");
            return;
        }

        List<int> selectedIndexes = new();
        while (selectedIndexes.Count < cardCountToShow)
        {
            int rand = Random.Range(0, allCardPrefabs.Count);
            if (!selectedIndexes.Contains(rand))
                selectedIndexes.Add(rand);
        }

        float radiusX = 550f;   // 좌우 퍼짐 정도 (클수록 더 넓게)
        float radiusY = 300f;   // 세로 위치의 벌어짐 정도 (낮을수록 덜 위로 올라감)
        float totalAngle = 100f;
        float yOffset = 200f;  // 원하는 만큼 위로 올릴 값 ( 원하는 값으로 조절 가능)
        Vector2 center = Vector2.zero;

        for (int i = 0; i < cardCountToShow; i++)
        {
            GameObject card = Instantiate(allCardPrefabs[selectedIndexes[i]], cardSpawnParent);
            RectTransform rt = card.GetComponent<RectTransform>();
            CanvasGroup cg = card.GetComponent<CanvasGroup>();
            if (cg == null) cg = card.AddComponent<CanvasGroup>();

            float angle = -totalAngle / 2f + (totalAngle / (cardCountToShow - 1)) * i;
            float rad = angle * Mathf.Deg2Rad;

            // ㅅ 형태 위치 계산
            float targetX = Mathf.Sin(rad) * radiusX;
            float targetY = -Mathf.Abs(Mathf.Sin(rad)) * radiusY + yOffset;  // 아래쪽으로 퍼지도록

            float startY = -600f;

            if (rt != null)
            {
                rt.anchoredPosition = new Vector2(targetX, startY);
                cg.alpha = 0f;

                Sequence seq = DOTween.Sequence();
                seq.Append(rt.DOAnchorPosY(targetY, 0.6f).SetEase(Ease.OutCubic));
                seq.Join(cg.DOFade(1f, 0.6f));
                seq.SetAutoKill(true);
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
        hasSelected = true; // 선택 플래그 설정

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
                // 선택되지 않은 카드 → 아래로 이동하며 사라짐
                RectTransform rt = card.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.DOAnchorPosY(rt.anchoredPosition.y - 300f, 0.5f)
                      .SetEase(Ease.InBack)
                      .OnComplete(() => Destroy(card));
                }
                else
                {
                    // UI 오브젝트가 아니라면 일반 Transform 이동으로 대체
                    card.transform.DOMoveY(card.transform.position.y - 3f, 0.5f)
                        .SetEase(Ease.InBack)
                        .OnComplete(() => Destroy(card));
                }
            }
        }

        Debug.Log("선택된 카드: " + selected.name);
    }
}
