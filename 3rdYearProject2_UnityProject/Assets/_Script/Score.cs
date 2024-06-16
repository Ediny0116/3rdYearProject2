using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Score : MonoBehaviour
{
    TextMeshProUGUI scoreUI;
    string scoreString= "Score:";
    int ScoreCount = 0;
    // Start is called before the first frame update
    void Start()
    {
        scoreUI = GetComponent<TextMeshProUGUI>();
        scoreUI.text = scoreString + "0";
    }

    public void UpdateScore(int score)
    {
        ScoreCount += score;
        scoreUI.text = scoreString + ScoreCount;
    }

    public int GetScore()
    {
        return ScoreCount;
    }

    public void SaveScore()
    {
        PlayerPrefs.SetInt("FinalScore", ScoreCount);
        PlayerPrefs.Save();
    }
}
