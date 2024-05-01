using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
   public GameObject Lball,Bball;
    
    public void SpawnLBall()
    {
        Instantiate(Lball, new Vector3(0, 8f, 0), Quaternion.identity);
    }
    public void SpawnBBall()
    {
        Instantiate(Bball, new Vector3(0, 8f, 0), Quaternion.identity);
    }
}
