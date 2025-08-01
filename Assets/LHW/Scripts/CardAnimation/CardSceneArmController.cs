using UnityEngine;

public class CardSceneArmController : MonoBehaviour
{
    [SerializeField] CardSceneCharacterRightArm right;
    [SerializeField] CardSceneCharacterLeftArm left;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SelectCard(1);
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            SelectCard(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SelectCard(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SelectCard(4);
        }
        else if(Input.GetKeyDown (KeyCode.Alpha5))
        {
            SelectCard(5);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            SelectCard(6);
        }
    }

    public void SelectCard(int num)
    {
        right.SelectCard(num);
        left.SelectCard(num);
    }
}
