using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerUI;
    [SerializeField] float remainingTime = 60;

    private void Start()
    {
        timerUI = GetComponent<TextMeshProUGUI>();
    }
    // Update is called once per frame
    void Update()
    {
        if (!(remainingTime- Time.deltaTime<=0))
        {
            remainingTime -= Time.deltaTime;
        }
        else
        {
            remainingTime = 0;
        }
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        timerUI.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        if(remainingTime <= 0)
        {
            FindAnyObjectByType<GameManager>().EndGame();
        }
    }

    public void SetRemainingTime(float Time)
    {
        remainingTime = Time;
    }
}
