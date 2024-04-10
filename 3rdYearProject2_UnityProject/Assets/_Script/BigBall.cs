using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigBall : MonoBehaviour
{
    public GameObject bigBall;
    public GameObject headbigBall;

    // Update is called once per frame
    void Update()
    {
        //check if player is near bigball
        if (Vector3.Distance(transform.position, bigBall.transform.position) < 0.5f)
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
