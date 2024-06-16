using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigBall : MonoBehaviour
{
    public GameObject bigBall;
    public GameObject headbigBall;
    public GameObject player;
    public float pickDistance = 1f;

    // Update is called once per frame
    void Update()
    {
        //check if player is near bigball
        //The first parameter "transform.position" in the Distance function represents the position of "the object attached to the script", which is equivalent to the second parameter bigBall.transform.position, which means you put two identical things to compare the distance.
        if (Vector3.Distance(player.transform.position, bigBall.transform.position) < pickDistance)
        {
            //check if left mousebotton click
            if (Input.GetMouseButtonDown(0))
            {
                headbigBall.SetActive(true);
                Destroy(bigBall);
            }
        }
        else
        {
            headbigBall.SetActive(false);
        }
    }
}
