// PlayerAnimation.cs
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void CheckAndStartDeathAnimation(bool isPlayerDead)
    {
        if (isPlayerDead)
        {
            StartDeathAnimation();
        }
    }

    public void StartDeathAnimation()
    {
        animator.SetBool("IsDead", true);
    }

    public void OnDeathAnimationComplete()
    {
        GetComponent<PlayerMove>().BackToIdle();
    }

    public void WalkAnimation(float verticalInput, float horizontalInput)
    {
        bool isMoving = (verticalInput != 0 || horizontalInput != 0);

        //animator.SetBool("isWalk", isMoving);
        animator.SetBool("isPickUp", isMoving);
    }
}
