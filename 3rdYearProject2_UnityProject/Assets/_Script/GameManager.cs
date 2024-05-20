using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Transform RespawnPoint;
    public GameObject Lball,Bball;
    
    public void SpawnLBall()
    {
        Instantiate(Lball, RespawnPoint.position, Quaternion.identity);
    }
    public void SpawnBBall()
    {
        Instantiate(Bball, RespawnPoint.position, Quaternion.identity);
    }
}
