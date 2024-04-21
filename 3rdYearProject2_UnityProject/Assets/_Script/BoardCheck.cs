using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardCheck : MonoBehaviour
{
    public List<GameObject> Target = new List<GameObject>();

    void CheckLine()
    {
       bool clicked = Target[1].GetComponent<ChessBoard>().GetIsClicked();
    }
}
