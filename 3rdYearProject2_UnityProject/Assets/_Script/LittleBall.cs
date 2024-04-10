using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LittleBall : MonoBehaviour
{
    public GameObject littleBall;
    public GameObject headlittleBall;
    public GameObject player;
    public float pickDistance=1f;

    // Update is called once per frame
    void Update()
    {
        //check if player is near littleball
        if (Vector3.Distance(player.transform.position, littleBall.transform.position) < pickDistance)
        {
            //check if left mousebotton click
            if (Input.GetMouseButtonDown(0))
            {
                //active headlittleBall
                headlittleBall.SetActive(true);
                Destroy(littleBall);
            }
        }
        else
        {
            headlittleBall.SetActive(false);
        }
    }
}
