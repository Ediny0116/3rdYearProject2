using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndMenuScoreDisplay : MonoBehaviour
{
    void Start()
    {
        int finalScore = PlayerPrefs.GetInt("FinalScore", 0);
        TextMeshProUGUI scoreUI = GameObject.Find("Text (TMP)EndScore").GetComponent<TextMeshProUGUI>();
        scoreUI.text = "Score: " + finalScore;
    }
}
