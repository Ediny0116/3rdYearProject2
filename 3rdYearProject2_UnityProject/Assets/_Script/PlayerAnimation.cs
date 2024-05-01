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
}
