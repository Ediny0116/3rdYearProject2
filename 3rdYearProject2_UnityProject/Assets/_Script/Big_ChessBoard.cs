using Newtonsoft.Json.Linq;
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
            ChangeColor(A, hoverColor);
            ChangeColor(B, hoverColor);
            ChangeColor(C, hoverColor);
            ChangeColor(D, hoverColor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
            ChangeColor(A, originalColors[0]);
            ChangeColor(B, originalColors[1]);
            ChangeColor(C, originalColors[2]);
            ChangeColor(D, originalColors[3]);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SetChessIsClicked(true);
        ChangeColor(A, bigClickColor);
        ChangeColor(B, bigClickColor);
        ChangeColor(C, bigClickColor);
        ChangeColor(D, bigClickColor);
    }

    private void ChangeColor(GameObject cell, Color color)
    {
        cell.GetComponent<ChessBoard>().ChangeColor(color);
    }

    //-----------------
    void SetChessIsClicked(bool value)
    {   //將大棋格的狀態傳給小棋格
        A.GetComponent<ChessBoard>().SetIsClicked(value);
        B.GetComponent<ChessBoard>().SetIsClicked(value);
        C.GetComponent<ChessBoard>().SetIsClicked(value);
        D.GetComponent<ChessBoard>().SetIsClicked(value);
    }
    //-----------------
}
