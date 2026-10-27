using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class TextHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text text;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.red;

    public void OnPointerEnter(PointerEventData eventData)
    {
        text.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        text.color = normalColor;
    }
}