using UnityEngine;

public class CardSceneArmController : MonoBehaviour
{
    [SerializeField] CardSceneCharacterRightArm right;
    [SerializeField] CardSceneCharacterLeftArm left;

    public void SelectCard(int num)
    {
        Debug.Log($"[ArmController] 팔 움직임 호출 index = {num}");
        right.SelectCard(num);
        left.SelectCard(num);
    }
}
