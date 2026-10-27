using UnityEngine;

public class FatGhostAnimation : MonoBehaviour
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
}