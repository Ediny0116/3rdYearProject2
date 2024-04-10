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
        //原版: if (Vector3.Distance(transform.position, bigBall.transform.position) < 0.5f). Distance函數中第一個參數transform.position代表"該腳本附著的物件"的位置，等同第二個參數bigBall.transform.position，也就是你放了兩個一樣的東西去比較距離。
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
