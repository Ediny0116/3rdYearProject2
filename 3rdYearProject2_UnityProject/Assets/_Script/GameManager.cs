using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Transform RespawnPoint;
    public GameObject Lball, Bball;
    [SerializeField] GameObject endMenu;

    private void Awake()
    {
        /*
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        */
    }
    private void Start()
    {
        endMenu = GameObject.Find("EndMenu");
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
        endMenu = GameObject.Find("EndMenu");
        endMenu.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
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
