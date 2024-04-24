using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Big_ChessBoard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public GameObject[] chessBoard = new GameObject[4];

    public Color hoverColor;
    public Color bigClickColor;
    private Color[] originalColors;

    void Start()
    {
        
        originalColors = new Color[4];
        for (int i = 0; i < 4; i++)
        {
            originalColors[i] = chessBoard[i].GetComponent<Renderer>().material.color;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ChangeColor(hoverColor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ChangeColor(originalColors);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SetChessIsClicked(true);
        ChangeColor(bigClickColor);
    }

    private void ChangeColor(Color color)
    {
        // 將顏色應用到每個棋盤格子
        foreach (GameObject cell in chessBoard)
        {
            cell.GetComponent<ChessBoard>().ChangeColor(color);
        }
    }

    private void ChangeColor(Color[] colors)
    {
        // 將每個棋盤格子的顏色恢復到原始顏色
        for (int i = 0; i < 4; i++)
        {
            chessBoard[i].GetComponent<ChessBoard>().ChangeColor(colors[i]);
        }
    }

    //-----------------
    void SetChessIsClicked(bool value)
    {   //將大棋格的狀態傳給小棋格
        foreach (GameObject cell in chessBoard)
        {
            cell.GetComponent<ChessBoard>().SetIsClicked(value);
        }
    }
    //-----------------
}
