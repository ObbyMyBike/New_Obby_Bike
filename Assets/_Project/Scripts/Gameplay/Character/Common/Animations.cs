using UnityEngine;

public class Animations
{
    private static readonly int HashSpeed = Animator.StringToHash("Speed");
    private static readonly int HashInAir = Animator.StringToHash("InAir");
    private static readonly int HashTouchLeft = Animator.StringToHash("TouchLeft");
    private static readonly int HashTouchRight = Animator.StringToHash("TouchRight");

    private readonly Animator animator;

    public Animations(Animator animator)
    {
        this.animator = animator;
    }
    
    public void UpdateAnimator(Rigidbody rigidbody)
    {
        float speedParam = rigidbody.velocity.magnitude / 3f;

        animator.SetFloat(HashSpeed, speedParam);
    }
    
    public void SetAirborne(bool inAir) => animator.SetBool(HashInAir, inAir);
    
    public void PlayPush(PushSide side)
    {
        if (side == PushSide.Left)
            animator.SetTrigger(HashTouchLeft);
        else
            animator.SetTrigger(HashTouchRight);
    }
}