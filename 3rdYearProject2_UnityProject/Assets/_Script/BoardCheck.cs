using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardCheck : MonoBehaviour
{
    public List<GameObject> ChessList=new List<GameObject>();
    public int[] LineCount=new int[12];
    public GameObject ScoreUI;

    public void AddToLine(GameObject chess)
    {
        ScoreUI.GetComponent<Score>().UpdateScore(1);
        int ChessNum=ChessList.IndexOf(chess);
        int posY = (ChessNum / 5)+5;
        int posX = ChessNum % 5;

        LineCount[posY]++;
        LineCount[posX]++;

        if (ChessNum % 6 == 0)
        {
            LineCount[10]++;
        }
        if (ChessNum%4==0)
        {
            LineCount[11]++;
        }

        if (CheckIsLineFull(LineCount[posY]))
        {
            ScoreUI.GetComponent<Score>().UpdateScore(5);
        }
        if (CheckIsLineFull(LineCount[posX]))
        {
            ScoreUI.GetComponent<Score>().UpdateScore(5);
        }
        if (CheckIsLineFull(LineCount[10]))
        {
            ScoreUI.GetComponent<Score>().UpdateScore(5);
        }
        if (CheckIsLineFull(LineCount[11]))
        {
            ScoreUI.GetComponent<Score>().UpdateScore(5);
        }
    }

    bool CheckIsLineFull(int lineCount)
    {
        if(lineCount==5)
        {
            return true;
        }
        return false;
    }
}
