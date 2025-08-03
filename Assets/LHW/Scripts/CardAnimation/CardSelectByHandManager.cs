using UnityEngine;

public class CardSelectByHandManager : MonoBehaviour
{
    [SerializeField] GameObject[] cards;
    [SerializeField] CardSceneArmController armController;
    
   
    private int selectedIndex = -1;
    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.D))
        {
            SelectRightCard();
            if (selectedIndex >= 0 && selectedIndex < cards.Length)
            {
                cards[selectedIndex].GetComponent<FlipCard>().RPC_Flip();
                armController.SelectCard(selectedIndex);
                CardAnimaitonPlay();
            }
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            SelectLeftCard();
            if (selectedIndex >= 0 && selectedIndex < cards.Length)
            {
                cards[selectedIndex].GetComponent<FlipCard>().RPC_Flip();
                armController.SelectCard(selectedIndex);
                if (cards[selectedIndex].GetComponent<FlipCard>().IsFlipped)
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
            else if (cards[i].GetComponent<FlipCard>().IsFlipped)
            {
                cards[i].GetComponentInChildren<CardAnimator>().StopAnimation();
            }
        }
    }
   
}