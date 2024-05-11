// PlayerAnimation.cs
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
}
