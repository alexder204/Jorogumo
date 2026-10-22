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
    private float footstepTimer;

    [Tooltip("Optional point at the player's feet. Uses the player position if empty.")]
    [SerializeField] private Transform footstepPoint;

    [Tooltip("Sounds used outside any footstep zone.")]
    [SerializeField] private AudioClip[] defaultFootstepClips;

    public static bool isInDialogue = false;
    public static bool isFading = false;
    public static bool isSceneLoading = false;

    private bool lockedDirection = false;

    [Header("Interaction")]
    [SerializeField] private BoxCollider2D interactCollider;  // the trigger collider on the player
                                                              // Distance from player per direction
    [SerializeField] private float distUp = 0.5f;
    [SerializeField] private float distDown = 0.5f;
    [SerializeField] private float distLeft = 0.5f;
    [SerializeField] private float distRight = 0.5f;

    // Collider size per direction
    [SerializeField] private Vector2 sizeUp = new Vector2(0.6f, 0.4f);
    [SerializeField] private Vector2 sizeDown = new Vector2(0.6f, 0.4f);
    [SerializeField] private Vector2 sizeLeft = new Vector2(0.4f, 0.6f);
    [SerializeField] private Vector2 sizeRight = new Vector2(0.4f, 0.6f);

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (GameUIController.IsGameplayBlocked)
        {
            movement = Vector2.zero;
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
        if (GameUIController.IsGameplayBlocked)
        {
            movement = Vector2.zero;
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

        UpdateInteractionCollider(lastDirection);
        animator.Play(prefix + lastDirection);
    }

    private void UpdateInteractionCollider(string direction)
    {
        if (interactCollider == null) return;

        switch (direction)
        {
            case "Up":
                interactCollider.offset = new Vector2(0f, distUp);
                interactCollider.size = sizeUp;
                break;

            case "Down":
                interactCollider.offset = new Vector2(0f, -distDown);
                interactCollider.size = sizeDown;
                break;

            case "Left":
                interactCollider.offset = new Vector2(-distLeft, 0f);
                interactCollider.size = sizeLeft;
                break;

            case "Right":
                interactCollider.offset = new Vector2(distRight, 0f);
                interactCollider.size = sizeRight;
                break;
        }
    }

    void HandleFootsteps()
    {
        if (footstepSource == null)
            return;

        bool hasDefaultClips =
            defaultFootstepClips != null &&
            defaultFootstepClips.Length > 0;

        if (!hasDefaultClips && GetCurrentFootstepZone() == null)
        {
            footstepSource.Stop();
            footstepTimer = 0f;
            return;
        }
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
        if (footstepSource == null)
            return;

        FootstepZone zone = GetCurrentFootstepZone();

        AudioClip clip = zone != null
            ? zone.GetRandomClip(zone.surfaceType)
            : null;

        if (clip == null &&
            defaultFootstepClips != null &&
            defaultFootstepClips.Length > 0)
        {
            clip = defaultFootstepClips[
                Random.Range(0, defaultFootstepClips.Length)];
        }

        if (clip != null)
            footstepSource.PlayOneShot(clip);
    }

    FootstepZone GetCurrentFootstepZone()
    {
        Vector2 point = footstepPoint != null
            ? (Vector2)footstepPoint.position
            : (Vector2)transform.position;

        Collider2D[] colliders = Physics2D.OverlapPointAll(point);
        FootstepZone selectedZone = null;

        foreach (Collider2D area in colliders)
        {
            if (!area.isTrigger)
                continue;

            FootstepZone zone =
                area.GetComponentInParent<FootstepZone>();

            if (zone == null || !zone.isActiveAndEnabled)
                continue;

            if (selectedZone == null ||
                zone.priority > selectedZone.priority)
            {
                selectedZone = zone;
            }
        }

        return selectedZone;
    }
}
