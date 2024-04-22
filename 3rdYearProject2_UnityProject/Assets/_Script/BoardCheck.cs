using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardCheck : MonoBehaviour
{
    public List<GameObject> ChessList=new List<GameObject>();
    public int[] ClickCountInLine=new int[12];
    public GameObject ScoreUI;
    public List<GameObject> LightballList = new List<GameObject>();

    int LineCount = 0;

    public void AddToLine(GameObject chess)
    {
        ScoreUI.GetComponent<Score>().UpdateScore(1);
        int ChessNum=ChessList.IndexOf(chess);
        int LineY = (ChessNum / 5)+5;
        int LineX = ChessNum % 5;

        ClickCountInLine[LineY]++;
        //Debug.Log("Line " + LineY + " +1");
        ClickCountInLine[LineX]++;
        //Debug.Log("Line " + LineX + " +1");

        if (ChessNum % 6 == 0)
        {
            ClickCountInLine[10]++;
            //Debug.Log("Line " + 10 + " +1");
        }
        if (ChessNum%4==0&&ChessNum!=24&& ChessNum !=0)
        {
            ClickCountInLine[11]++;
            //Debug.Log("Line " + 11 + " +1");
        }

        if (CheckIsLineFull(ClickCountInLine[LineY]))
        {
            ScoreUI.GetComponent<Score>().UpdateScore(5);
            AddLineCount();
            ClickCountInLine[LineY] = 0;
            //Debug.Log("Line "+LineY+" Full");
        }
        if (CheckIsLineFull(ClickCountInLine[LineX]))
        {
            ScoreUI.GetComponent<Score>().UpdateScore(5);
            AddLineCount();
            ClickCountInLine[LineX] = 0;
            //Debug.Log("Line " + LineX + " Full");
        }
        if (CheckIsLineFull(ClickCountInLine[10]))
        {
            ScoreUI.GetComponent<Score>().UpdateScore(5);
            AddLineCount();
            ClickCountInLine[10] = 0;
            //Debug.Log("Line 10 Full");
        }
        if (CheckIsLineFull(ClickCountInLine[11]))
        {
            ScoreUI.GetComponent<Score>().UpdateScore(5);
            AddLineCount();
            ClickCountInLine[11] = 0;
            //Debug.Log("Line 11 Full");
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

    void AddLineCount()
    {
        if(LineCount<5)
        LightballList[LineCount].GetComponent<Renderer> ().material.EnableKeyword("_EMISSION"); ;
        LineCount++;

    }
}
