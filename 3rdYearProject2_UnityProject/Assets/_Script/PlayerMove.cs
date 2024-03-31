using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float maxRotate = 5f; // 最大旋?角度
    public float maxSpeed = 3f; // 最大移?速度
    public float lerpAmt = 0.1f; // 插值系?

    private Vector3 rVec; // 相机的右向量（用于旋?）
    private Vector3 fVec; // 相机的前向量（用于移?）

    void Start()
    {
        rVec = Camera.main.transform.right;
        Vector3 tempV = Camera.main.transform.forward;
        tempV.y = 0;
        tempV.Normalize();
        fVec = tempV;
    }

    void Update()
    {
        // Get Input
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        if (verticalInput != 0 || horizontalInput != 0)
        {
            float transAmt = verticalInput;
            float rotAmt = horizontalInput;
            MoveAndRotate(transAmt, rotAmt);
        }
    }

    void MoveAndRotate(float transAmt, float rotAmt)
    {
        Vector3 dir = (rVec * rotAmt) + (fVec * transAmt);

        transform.forward = Vector3.Slerp(transform.forward, dir, maxRotate * Time.deltaTime);

        float moveDist = dir.magnitude;
        Vector3 moveAmt = transform.forward * moveDist * maxSpeed;

        transform.position += moveAmt * Time.deltaTime;
    }
}
