using UnityEngine;

public class CardSelectByHandManager : MonoBehaviour
{
    [SerializeField] GameObject[] cards;
    [SerializeField] CardSceneArmController armController;
    
    // 스크립트 충돌 문제로 임시 주석처리했습니다. 용호님의 플립카드 스크립트와 연결 필요
    /*
    private int selectedIndex = -1;
    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.D))
        {
            SelectRightCard();
            if (selectedIndex >= 0 && selectedIndex < cards.Length)
            {
                cards[selectedIndex].GetComponent<LHWFlipCard>().PlayFlipAnimation();
                armController.SelectCard(selectedIndex);
                CardAnimaitonPlay();
            }
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            SelectLeftCard();
            if (selectedIndex >= 0 && selectedIndex < cards.Length)
            {
                cards[selectedIndex].GetComponent<LHWFlipCard>().PlayFlipAnimation();
                armController.SelectCard(selectedIndex);
                if (cards[selectedIndex].GetComponent<LHWFlipCard>().IsFlipped)
                {
                    CardAnimaitonPlay();
                }
            }
        }
    }

    private void SelectRightCard()
    {
        if (selectedIndex >= cards.Length - 1) return;

        selectedIndex++;
    }

    private void SelectLeftCard()
    {
        if (selectedIndex <= 0) return;

        selectedIndex--;
    }

    private void CardAnimaitonPlay()
    {
        for (int i = 0; i < cards.Length; i++)
        {
            if (i == selectedIndex)
            {
                cards[selectedIndex].GetComponentInChildren<CardAnimator>().RestartAnimation();
                continue;
            }
            else if (cards[i].GetComponent<LHWFlipCard>().IsFlipped)
            {
                cards[i].GetComponentInChildren<CardAnimator>().StopAnimation();
            }
        }
    }
    */
}