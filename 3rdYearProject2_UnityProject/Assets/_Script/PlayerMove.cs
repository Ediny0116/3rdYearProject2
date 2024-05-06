// PlayerMove.cs
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float maxRotate = 5f;
    public float maxSpeed = 3f;

    private Vector3 rVec;
    private Vector3 fVec;

    private bool canMove = true;
    public bool isDead = false;

    private Animator animator;

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
        if (canMove && !isDead)
        {
            // Get Input
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            float transAmt = verticalInput;
            float rotAmt = horizontalInput;
            MoveAndRotate(transAmt, rotAmt);

            // No need to call WalkAnimation here
        }

        // Check if the player is at position (0, 8, 0)
        if (transform.position == new Vector3(0f, 8f, 0f))
        {
            isDead = false;
            canMove = true;
        }
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
        if (other.CompareTag("Water"))
        {
            isDead = true;
            canMove = false;
            GetComponent<PlayerAnimation>().StartDeathAnimation();
        }
    }

    // Animation Event method called when death animation is complete
    public void BackToIdle()
    {
        isDead = false;
        canMove = true;
        animator.SetBool("IsDead", false);
    }
}
