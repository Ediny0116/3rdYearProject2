// PlayerMove.cs
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float maxRotate = 5f;
    public float maxSpeed;
    public float originalMaxSpeed = 3f;

    private Vector3 rVec;
    private Vector3 fVec;

    private Animator animator;
    private ObjectGrabbable objectGrabbable;

    void Start()
    {
        rVec = Camera.main.transform.right;
        Vector3 tempV = Camera.main.transform.forward;
        tempV.y = 0;
        tempV.Normalize();
        fVec = tempV;

        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Get Input
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        float transAmt = verticalInput;
        float rotAmt = horizontalInput;
        MoveAndRotate(transAmt, rotAmt);

        // Check if the player is picking up the ball
        if (Input.GetKeyDown(KeyCode.E))
        {
            animator.SetBool("isPickUp", true);
            animator.SetBool("afterPickUp",true);
        }


        //can not working IDK why-------------
        /*if (Input.GetKeyDown(KeyCode.Mouse0)&&objectGrabbable!=null)
        {
            animator.SetBool("isPickUp", false);
            animator.SetBool("afterPickUp", false);
            animator.SetBool("isThrow", true);
        }*/
        //------------------------
    }

    void MoveAndRotate(float transAmt, float rotAmt)
    {
        Vector3 dir = (rVec * rotAmt) + (fVec * transAmt);

        transform.forward = Vector3.Slerp(transform.forward, dir, maxRotate * Time.deltaTime);

        float moveDist = dir.magnitude;
        Vector3 moveAmt = transform.forward * moveDist * maxSpeed;

        transform.position += moveAmt * Time.deltaTime;

        // Call WalkAnimation based on user input
        GetComponent<PlayerAnimation>().WalkAnimation(transAmt, rotAmt);
    }

    
    private void OnTriggerEnter(Collider other)
    {
        /*//when player pick up: -------------------
        if (Input.GetKeyDown(KeyCode.E)&& objectGrabbable != null)
        {
            if(other.CompareTag("BigBall") || other.CompareTag("LittleBall"))
            {
                animator.SetBool("isPickUp", true);
            }
        }
        else
        {
            //animator.SetBool("isPickUp",false);
        }*/

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
