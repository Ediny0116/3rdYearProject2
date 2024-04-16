using UnityEngine;
using UnityEngine.EventSystems;

public class Big_ChessBoard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public GameObject A;
    public GameObject B;
    public GameObject C;
    public GameObject D;

    public Color hoverColor;
    public Color bigClickColor;
    private Color[] originalColors;
    private bool isClicked = false;

    void Start()
    {
        originalColors = new Color[4];
        originalColors[0] = A.GetComponent<Renderer>().material.color;
        originalColors[1] = B.GetComponent<Renderer>().material.color;
        originalColors[2] = C.GetComponent<Renderer>().material.color;
        originalColors[3] = D.GetComponent<Renderer>().material.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isClicked)
        {
            ChangeColor(A, hoverColor);
            ChangeColor(B, hoverColor);
            ChangeColor(C, hoverColor);
            ChangeColor(D, hoverColor);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isClicked)
        {
            ChangeColor(A, originalColors[0]);
            ChangeColor(B, originalColors[1]);
            ChangeColor(C, originalColors[2]);
            ChangeColor(D, originalColors[3]);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        isClicked = true;
        ChangeColor(A, bigClickColor);
        ChangeColor(B, bigClickColor);
        ChangeColor(C, bigClickColor);
        ChangeColor(D, bigClickColor);
    }

    private void ChangeColor(GameObject cell, Color color)
    {
        cell.GetComponent<Renderer>().material.color = color;
    }
}
