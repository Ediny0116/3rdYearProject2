using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Transform RespawnPoint;
    public GameObject Lball,Bball;
    [SerializeField] GameObject endMenu;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    public void SpawnLBall()
    {
        Instantiate(Lball, RespawnPoint.position, Quaternion.identity);
    }
    public void SpawnBBall()
    {
        Instantiate(Bball, RespawnPoint.position, Quaternion.identity);
    }

    public void EndGame()
    {
        endMenu.SetActive(true);
        Time.timeScale = 0f;
        endMenu.transform.Find("Text (TMP)EndScore").GetComponent<TMPro.TextMeshProUGUI>().text = "Score:" + GameObject.Find("Text (TMP)Score").GetComponent<Score>().GetScore();
    }
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        GameObject.Find("Timer").GetComponent<Timer>().SetRemainingTime(60);
    }
}
