
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private NavClickToMove clickToMove;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        clickToMove = GetComponent<NavClickToMove>();
    }

    void Update()
    {
        if (animator == null || clickToMove == null)
            return;

        animator.SetFloat(
            "Speed",
            clickToMove.IsMoving ? 1f : 0f
        );
    }
}
