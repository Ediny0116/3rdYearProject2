using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerPickUpDrop : MonoBehaviour {


    [SerializeField] private Transform playerCameraTransform;
    [SerializeField] private Transform objectGrabPointTransform;
    private GameObject ball;
    private List<GameObject> ballList = new List<GameObject>();
    private ObjectGrabbable objectGrabbable;
    [SerializeField] private float throwForce = 20f;
    private void Update()
    {
        if (ballList.Count!=0)
        {
            ball = ballList[0];
            GrabKey();
            ShootKey();
        }
        else
        {
            ball = null;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
           ballList.Add(other.gameObject);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            ballList.Remove(other.gameObject);
        }
    }
    
    void GrabKey()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (objectGrabbable == null)
            {
                // Not carrying an object, try to grab
                objectGrabbable = ball.GetComponent<ObjectGrabbable>();
                objectGrabbable.Grab(objectGrabPointTransform);
            }
            else
            {
                // Currently carrying something, drop
                objectGrabbable.Drop();
                objectGrabbable = null;
            }
        }
    }
    void ShootKey()
    {
        
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (objectGrabbable != null)
            {
                // Currently carrying something, throw
                objectGrabbable.Drop();
                objectGrabbable = null;

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                // 檢測射線是否與3D地圖上的物體相交
                if (Physics.Raycast(ray, out hit))
                {
                    Vector3 direction = hit.point - ball.transform.position;

                    // 施加力道
                    ball.GetComponent<Rigidbody>().velocity = direction.normalized * throwForce;

                    ball = null;
                }
            }
        }
        
    }
}