using UnityEngine;

public class ThinGhostAnimation : MonoBehaviour
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void SetChasing(bool chasing)
    {
        if (animator != null)
        {
            animator.SetBool("IsChasing", chasing);
        }
    }

    public void PlayAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack_fat");
        }
    }
}