using UnityEngine;

public class TopDownMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;

    [Header("Footstep Settings")]
    public AudioSource footstepSource;
    public float footstepInterval = 0.4f;
    public float footstepIntervalRun = 0.3f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator animator;
    private string lastDirection = "Down";
    private string currentSurface = "Default";
    private float footstepTimer;

    public static bool isInDialogue = false;
    public static bool isFading = false;
    public static bool isSceneLoading = false;

    private bool lockedDirection = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (PauseManager.isGamePaused || isInDialogue || isFading || isSceneLoading)
        {
            if (!lockedDirection)
            {
                animator.Play("Idle" + lastDirection);
                lockedDirection = true;
            }

            rb.linearVelocity = Vector2.zero;
            footstepSource.Stop();
            return;
        }

        lockedDirection = false;

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement.Normalize();

        HandleAnimation();
        HandleFootsteps();
    }

    void FixedUpdate()
    {
        if (PauseManager.isGamePaused || isInDialogue || isFading || isSceneLoading)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;
        rb.linearVelocity = movement * currentSpeed;
    }

    void HandleAnimation()
    {
        bool isMoving = movement.magnitude > 0.01f;
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);

        if (isSprinting && isMoving)
            PlayDirectionAnimation("Run");
        else if (isMoving)
            PlayDirectionAnimation("Walk");
        else
            animator.Play("Idle" + lastDirection);
    }

    void PlayDirectionAnimation(string prefix)
    {
        if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
        {
            if (movement.x > 0)
                lastDirection = "Right";
            else
                lastDirection = "Left";
        }
        else
        {
            if (movement.y > 0)
                lastDirection = "Up";
            else
                lastDirection = "Down";
        }

        animator.Play(prefix + lastDirection);
    }

    void HandleFootsteps()
    {
        if (movement.magnitude <= 0.01f)
        {
            footstepSource.Stop();
            footstepTimer = 0f;
            return;
        }

        footstepTimer -= Time.deltaTime;
        float interval = Input.GetKey(KeyCode.LeftShift) ? footstepIntervalRun : footstepInterval;

        if (footstepTimer <= 0f)
        {
            PlayFootstep();
            footstepTimer = interval;
        }
    }

    void PlayFootstep()
    {
        FootstepZone zone = GetCurrentFootstepZone();
        if (zone != null)
        {
            AudioClip clip = zone.GetRandomClip(currentSurface);
            if (clip != null)
                footstepSource.PlayOneShot(clip);
        }
    }

    FootstepZone GetCurrentFootstepZone()
    {
        Collider2D[] colliders = Physics2D.OverlapPointAll(transform.position);
        foreach (var col in colliders)
        {
            FootstepZone zone = col.GetComponent<FootstepZone>();
            if (zone != null && zone.surfaceType == currentSurface)
                return zone;
        }
        return null;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        FootstepZone zone = other.GetComponent<FootstepZone>();
        if (zone != null)
            currentSurface = zone.surfaceType;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        FootstepZone zone = other.GetComponent<FootstepZone>();
        if (zone != null && currentSurface == zone.surfaceType)
            currentSurface = "Default";
    }
}
