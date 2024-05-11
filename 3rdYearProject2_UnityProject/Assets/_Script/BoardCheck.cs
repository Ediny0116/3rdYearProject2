using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BoardCheck : MonoBehaviour
{
    public List<GameObject> ChessList=new List<GameObject>();
    public int[] ClickCountInLine=new int[12];
    public GameObject ScoreUI;
    public List<GameObject> LightballList = new List<GameObject>();
    public Color[] LightColor=new Color[3];

    public List<TextMeshPro> DebugText = new List<TextMeshPro>();

    int LineCount = 0;

    private void Start()
    {
        foreach (GameObject LB in LightballList)
        {
            LB.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
            LB.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
        }
    }
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

        #region 斜線
        /*
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
        */
        #endregion

        CheckIsLineFull(LineY, LineX);
        DebugText[LineY].text = ClickCountInLine[LineY] + "";
        DebugText[LineX].text = ClickCountInLine[LineX] + "";
    }

    void CheckIsLineFull(int lineY,int lineX)
    {
        bool[] isLineFull = new bool[2];
        isLineFull[0] = false;
        isLineFull[1] = false;
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
        //兩條線都沒滿
        if (!isLineFull[0] && !isLineFull[1])
        {
            return;
        }
        //只有Y軸線滿
        else if (isLineFull[0] && !isLineFull[1])
        {
            int ChessNum = (lineY % 5) * 5;
            for (int i = 0; i < 5; i++)
            {
                ClickCountInLine[i] -= 1;
                ChessList[ChessNum + i].GetComponent<ChessBoard>().SetIsClicked(false);
            }
            ClickCountInLine[lineY] = 0;
        }
        //只有X軸線滿
        else if (!isLineFull[0] && isLineFull[1])
        {
            int ChessNum = lineX;
            for (int i = 0; i < 5; i++)
            {
                ClickCountInLine[i + 5] -= 1;
                ChessList[ChessNum + i * 5].GetComponent<ChessBoard>().SetIsClicked(false);
            }
            ClickCountInLine[lineX] = 0;
        }
        //Y軸線與X軸線都滿
        else if (isLineFull[0] && isLineFull[1])
        {
            int ChessNum = (lineY % 5) * 5;
            for (int i = 0; i < 5; i++)
            {
                ClickCountInLine[i] -= 1;
                ChessList[ChessNum + i].GetComponent<ChessBoard>().SetIsClicked(false);
            }
            ChessNum = lineX;
            for (int i = 0; i < 5; i++)
            {
                ClickCountInLine[i + 5] -= 1;
                ChessList[ChessNum + i * 5].GetComponent<ChessBoard>().SetIsClicked(false);
            }
            ClickCountInLine[lineY] = 0;
            ClickCountInLine[lineX] = 0;
        }
    }
    void AddLineCount()
    {
        if(LineCount < 15)
        LightballList[LineCount%5].GetComponent<Renderer>().material.SetColor("_EmissionColor", LightColor[LineCount/5]);
        /*
        Debug.Log("LB " + LightballList[LineCount % 5].gameObject);
        Debug.Log("Line" + LineCount);
        Debug.Log("LightLv "+LineCount/5);
        */
        LineCount++;
    }
}
