using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LittleBall : MonoBehaviour
{
    public GameObject littleBall;
    public GameObject headlittleBall;

    // Update is called once per frame
    void Update()
    {
        //check if player is near littleball
        if (Vector3.Distance(transform.position, littleBall.transform.position) < 0.5f)
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
