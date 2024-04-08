using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float maxRotate = 5f; 
    public float maxSpeed = 3f; 
    public float lerpAmt = 0.1f; 

    private Vector3 rVec; 
    private Vector3 fVec; 

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
