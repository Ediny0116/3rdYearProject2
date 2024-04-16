using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChessBoard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Color hoverColor;//Color on mouseover
    public Color clickColor;//Click color

    private Color originalColor;//Original
    private bool isClicked = false;
    //private bool HeadLittleBallActivated = false;

    private void Start()
    {
        originalColor=GetComponent<Renderer>().material.color;//get the original color.
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isClicked)
        {
            ChangeColor(hoverColor);//when mouseover, change color.
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isClicked)
        {
            ChangeColor(originalColor);//when mouse move over, change color back.
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        isClicked = true;
        ChangeColor(clickColor);
    }

    private void ChangeColor(Color color)
    {
        GetComponent<Renderer>().material.color = color;//change cube color.
    }
}
