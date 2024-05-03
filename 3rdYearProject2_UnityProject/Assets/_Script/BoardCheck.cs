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
    public Color[] LightColor=new Color[3];

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
        CheckIsLineFull(LineY, LineX);
    }

    void CheckIsLineFull(int lineY,int lineX)
    {
        bool[] isLineFull = new bool[4];
        if(ClickCountInLine[lineY] == 5)
        {
            ScoreUI.GetComponent<Score>().UpdateScore(5);
            AddLineCount();
            isLineFull[0]=true;
        }
        if (ClickCountInLine[lineX] == 5)
        {
            ScoreUI.GetComponent<Score>().UpdateScore(5);
            AddLineCount();
            isLineFull[1] = true;
        }
        if (ClickCountInLine[10] == 5)
        {
            ScoreUI.GetComponent<Score>().UpdateScore(5);
            AddLineCount();
            isLineFull[2] = true;
        }
        if (ClickCountInLine[11] == 5)
        {
            ScoreUI.GetComponent<Score>().UpdateScore(5);
            AddLineCount();
            isLineFull[3] = true;
        }

        if (isLineFull[0])
        {
            int ChessNum = (lineY % 5) * 5;
            for (int i = 0; i < 5; i++)
            {
                ChessList[ChessNum + i].GetComponent<ChessBoard>().SetIsClicked(false);
            }
            ClickCountInLine[lineY] = 0;
        }
        else if (isLineFull[1]|| isLineFull[2]||isLineFull[3])
            ClickCountInLine[lineY] -= 1;

        if (isLineFull[1])
        {
            int ChessNum = lineX;
            for (int i = 0; i < 5; i++)
            {
                ChessList[ChessNum + i*5].GetComponent<ChessBoard>().SetIsClicked(false);
            }
            ClickCountInLine[lineX] = 0;
        }
        else if (isLineFull[0] || isLineFull[2] || isLineFull[3])
            ClickCountInLine[lineX] -= 1;

        if (isLineFull[2])
        {
            for (int i = 0; i < 5; i++)
            {
                ChessList[i * 6].GetComponent<ChessBoard>().SetIsClicked(false);
            }
            ClickCountInLine[10] = 0;
        }
        else if (isLineFull[1] || isLineFull[0] || isLineFull[3])
            ClickCountInLine[10] -= 1;

        if (isLineFull[3])
        {
            int ChessNum = 4;
            for (int i = 0; i < 5; i++)
            {
                ChessList[ChessNum+(i * 4)].GetComponent<ChessBoard>().SetIsClicked(false);
            }
            ClickCountInLine[11] = 0;
        }
        else if (isLineFull[1] || isLineFull[2] || isLineFull[0])
            ClickCountInLine[11] -= 1;
    }
    void AddLineCount()
    {
        if(LineCount < 15)
        LightballList[LineCount%5].GetComponent<Renderer>().material.SetColor("_EmissionColor", LightColor[LineCount/5]);
        Debug.Log("Line" + LineCount);
        Debug.Log("LightLv "+LineCount/5);
        LineCount++;

    }
}
