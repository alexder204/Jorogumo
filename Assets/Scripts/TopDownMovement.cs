using UnityEngine;

public class TopDownMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;

    private Rigidbody2D rb;
    private Vector2 movement;

    private Animator animator;
    private string lastDirection = "Down";

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement.Normalize();

        bool isMoving = movement.magnitude > 0.01f;
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);

        if (!isSprinting && isMoving)
        {
            // Play walk animation based on direction
            if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
            {
                if (movement.x > 0)
                {
                    animator.Play("WalkRight");
                    lastDirection = "Right";
                }
                else
                {
                    animator.Play("WalkLeft");
                    lastDirection = "Left";
                }
            }
            else
            {
                if (movement.y > 0)
                {
                    animator.Play("WalkUp");
                    lastDirection = "Up";
                }
                else
                {
                    animator.Play("WalkDown");
                    lastDirection = "Down";
                }
            }
        }
        else
        {
            // Idle based on last direction faced
            animator.Play("Idle" + lastDirection);
        }
    }

    void FixedUpdate()
    {
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;
        rb.linearVelocity = movement * currentSpeed;
    }
}
