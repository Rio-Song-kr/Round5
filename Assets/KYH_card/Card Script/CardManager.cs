using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSelectManager : MonoBehaviour
{
    [Header("전체 카드 프리팹 리스트")]
    public List<GameObject> allCardPrefabs;

    [Header("부모 레이아웃 그룹")]
    public Transform cardSpawnParent;

    [Header("출력할 카드 개수")]
    public int cardCountToShow = 3;

    private List<GameObject> currentCards = new();
    private bool hasSelected = false;

    void Start()
    {
        SpawnRandomCards();
    }

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

        // 중앙 기준 카드 간격 설정
        float spacing = 500f; // 카드 간 간격 (픽셀 단위, Canvas 기준)
        float centerX = 0f;
        float startX = centerX - (spacing * (cardCountToShow - 1) / 2f);

        for (int i = 0; i < cardCountToShow; i++)
        {
            GameObject card = Instantiate(allCardPrefabs[selectedIndexes[i]], cardSpawnParent);

            RectTransform rt = card.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = new Vector2(startX + spacing * i, -600f); // 시작 위치 아래
                rt.DOAnchorPosY(0f, 0.5f).SetEase(Ease.OutBack); // 위로 튀어오르는 애니메이션
            }

            FlipCard flip = card.GetComponent<FlipCard>();
            if (flip != null)
                flip.SetManager(this);

            currentCards.Add(card);
        }
    }

    public void OnCardSelected(GameObject selected)
    {
        if (hasSelected) return;
        hasSelected = true;

        foreach (GameObject card in currentCards)
        {
            if (card != selected)
                Destroy(card, 0.3f);
        }

        Debug.Log("선택된 카드: " + selected.name);
    }
}
