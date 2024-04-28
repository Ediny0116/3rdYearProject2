using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class balltest : MonoBehaviour
{
    void Update()
    {
        // 當滑鼠左鍵被點擊時
        if (Input.GetMouseButtonDown(0))
        {
            // 創建一條射線從相機發射到滑鼠位置
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // 檢測射線是否與3D地圖上的物體相交
            if (Physics.Raycast(ray, out hit))
            {
                Vector3 direction = hit.point - transform.position;

                // 施加力道
               // GetComponent<Rigidbody>().AddForce(direction.normalized * 10, ForceMode.Impulse);

                GetComponent<Rigidbody>().velocity = direction.normalized * 10;
            }
        }
    }
}
