using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Big_ChessBoard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    //建議:物件名稱用ABCD排序，腳本裡又用ABCD，拉物件時會混亂
    //建議:可以用List或陣列
    public GameObject A;
    public GameObject B;
    public GameObject C;
    public GameObject D;

    public Color hoverColor;
    public Color bigClickColor;
    private Color[] originalColors;
    private bool isClicked = false;
    //問題:
    //每個大格子都有自己的腳本，
    //你用if(!isClicked)判斷要不要變色，
    //但大格子影響的小格是有重疊的，
    //假設A1大格的isClicked是true，它隔壁的A2和B1仍然是false，
    //所以不受A2和B1影響的最角落的小格子會保持紅色，而其它三個小格會變色。
    //解法:判斷isClicked一律以小格為準，大格不需要這個變數。

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
        SetChessIsClicked(true);
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
