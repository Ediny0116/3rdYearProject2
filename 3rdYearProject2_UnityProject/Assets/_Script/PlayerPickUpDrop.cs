using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using Game;

public class PlayerPickUpDrop : NetworkBehaviour
{

    [SerializeField] private Transform objectGrabPointTransform;
    [SerializeField] private GameObject BigBallBox;
    [SerializeField] private LayerMask RayCastLayerMask;
    private GameObject ball;
    private List<GameObject> ballList = new List<GameObject>();
    public ObjectGrabbable objectGrabbable;
    [SerializeField] private float throwForce = 20f;

    [SerializeField] private Animator PinkAnimator;
    [SerializeField] private Animator BlueAnimator;
    private Animator animator;

    private void Start()
    {
        if (GameLobbyManager.Instance.IsHost)
        {
            animator = PinkAnimator;
        }
        else
        {
            animator = BlueAnimator;
        }
        //animator = GetComponent<Animator>();
        BigBallBox= GameObject.Find("BigBallBox");
        //BigBallBox.SetActive(false);
    }
    private void Update()
    {
        if (ballList.Count!=0)
        {
            ball = ballList[ballList.Count-1];

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
        if (other.CompareTag("BigBall")||other.CompareTag("LittleBall"))
        {
           ballList.Add(other.gameObject);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("BigBall") || other.CompareTag("LittleBall"))
        {
            ballList.Remove(other.gameObject);
            ballList.Remove(other.gameObject);
        }
    }
    
    void GrabKey()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (objectGrabbable == null)
            {
                if (ball != null)
                {
                    // Not carrying an object, try to grab
                    objectGrabbable = ball.GetComponent<ObjectGrabbable>();
                    objectGrabbable.Grab(objectGrabPointTransform);
                    if (GetIsBigBall())
                    {
                        BigBallBox.SetActive(true);
                    }
                    animator.SetBool("isPickUp", true);
                }
            }
            else
            {
                /*
                // Currently carrying something, drop
                objectGrabbable.Drop();
                objectGrabbable = null;
                BigBallBox.SetActive(false);
                */
            }
        }
    }
    void ShootKey()
    {
        
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            BigBallBox.SetActive(false);
            if (objectGrabbable != null)
            {
                // Currently carrying something, throw
                objectGrabbable.Drop();

                animator.SetBool("isThrow", true);
                animator.SetBool("isPickUp", false);

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                // 檢測射線是否與3D地圖上的物體相交
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, RayCastLayerMask))
                {
                    Vector3 direction = hit.point - ball.transform.position;

                    // 施加力道
                    objectGrabbable.gameObject.GetComponent<Rigidbody>().velocity = direction.normalized * throwForce;

                }
                objectGrabbable = null;
            }
        }
        
    }

    public bool GetIsGrabbing()
    {
        if (objectGrabbable != null)
        {
            return true;
        }
        return false;
    }
    public bool GetIsBigBall()
    {
        if (objectGrabbable.gameObject.CompareTag("BigBall"))
        {
            return true;
        }
        return false;
    }
}