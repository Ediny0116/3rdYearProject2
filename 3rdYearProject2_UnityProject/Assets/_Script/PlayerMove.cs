// PlayerMove.cs
using GameFramework.Network.Movement;
using Unity.Netcode;
using UnityEngine;

public class PlayerMove : NetworkBehaviour
{
    public float originalMaxSpeed = 3f;

    private Vector3 rVec;
    private Vector3 fVec;

    [SerializeField] private NetworkMovementComponent _playerMovement;

    private Animator animator;
    private PlayerPickUpDrop playerPickUpDrop;

    public override void OnNetworkSpawn()
    {
        
    }

    void Start()
    {
        /*
        rVec = Camera.main.transform.right;
        Vector3 tempV = Camera.main.transform.forward;
        tempV.y = 0;
        tempV.Normalize();
        fVec = tempV;*/

        animator = GetComponent<Animator>();
        playerPickUpDrop = GetComponent<PlayerPickUpDrop>();
    }

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        float transAmt = verticalInput;
        float rotAmt = horizontalInput;
        if (IsClient && IsLocalPlayer)
        {
            _playerMovement.ProcessLocalPlayerMovement(transAmt, rotAmt);
        }
        else
        {
            _playerMovement.ProcessSimulatedPlayerMovement();
        }
    }
    /*
    void MoveAndRotate(float transAmt, float rotAmt)
    {
        
        //Debug.Log("ClientMove");
        Vector3 dir = (rVec * rotAmt) + (fVec * transAmt);

        transform.forward = Vector3.Slerp(transform.forward, dir, maxRotate * Time.deltaTime);

        float moveDist = dir.magnitude;
        Vector3 moveAmt = transform.forward * moveDist * maxSpeed;

        transform.position += moveAmt * Time.deltaTime;
        

        // Call WalkAnimation based on user input
        if (playerPickUpDrop.objectGrabbable == null)
        {
            GetComponent<PlayerAnimation>().WalkAnimation(transAmt, rotAmt);
        }

        if(playerPickUpDrop.objectGrabbable != null)
        {
            GetComponent<PlayerAnimation>().PickUpRunAnimation(transAmt, rotAmt);
        }
    }*/
    
    private void OnTriggerEnter(Collider other)
    {
        //when touch water, Player speed slow
        if (other.CompareTag("Water"))
        {
            maxSpeed = maxSpeed / 2f;
            animator.SetBool("isDead", true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            //back to normal speed
            maxSpeed = originalMaxSpeed;
            animator.SetBool("isDead", false);
        }
    }
    
}
