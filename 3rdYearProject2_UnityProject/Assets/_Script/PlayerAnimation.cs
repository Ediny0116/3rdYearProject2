using Unity.Netcode;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

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
}
