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

    public void OnPickUpAnimationEnd()
    {
        animator.SetBool("isPickUp", false);
        animator.SetBool("afterPickUp", true); // 触?PickUp_stay??
    }

    // 添加在??事件中?用的方法
    public void AfterPickUpAnimation()
    {
        animator.SetBool("afterPickUp", false);
    }
}
