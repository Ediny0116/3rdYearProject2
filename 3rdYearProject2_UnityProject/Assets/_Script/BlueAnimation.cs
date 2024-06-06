using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueAnimation : MonoBehaviour
{
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    public void WalkAnimation(float verticalInput, float horizontalInput)
    {
        bool isMoving = (verticalInput != 0 || horizontalInput != 0);

        animator.SetBool("isWalk", isMoving);
    }
    public void PickUpRunAnimation(float verticalInput, float horizontalInput)
    {
        bool isMoving = (verticalInput != 0 || horizontalInput != 0);

        animator.SetBool("isPickUpRun", isMoving);
    }
    public void PickUpTurnToPickUpStay()
    {
        animator.SetBool("afterPickUp", true);
    }
    public void AfterPickUpAnimation()
    {
        animator.SetBool("afterPickUp", false);
    }
    public void AfterThrow()
    {
        animator.SetBool("isThrow", false);
    }

    private void OnTriggerEnter(Collider other)
    {
        //when touch water, Player speed slow
        if (other.CompareTag("Water"))
        {
            animator.SetBool("isDead", true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            animator.SetBool("isDead", false);
        }
    }
}
