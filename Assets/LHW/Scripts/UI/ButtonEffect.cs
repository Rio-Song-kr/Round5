using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Button button;

    [SerializeField] private TMP_Text buttonText;

    private void Start()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(button.IsInteractable()) buttonText.fontSize = 90;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(button.IsInteractable()) buttonText.fontSize = 80;
    }
}