using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2d : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 15f;
    public float stopThreshold = 0.05f;
    public float groundCheckDistance = 0.2f;
    public float stickForce = 20f;

    [Header("Layers")]
    public LayerMask groundMask;
    public LayerMask wallMask;

    [Header("References")]
    public Animator animator;

    private Rigidbody2D rb;
    private float targetX;
    private bool hasTarget = false;
    private bool isPaused = false; // <--- pause flag

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (isPaused) return; // ignore input when paused

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetX = mouseWorld.x;
            hasTarget = true;
        }

        // Update walking animation based on actual velocity
        if (animator != null)
        {
            bool walking = Mathf.Abs(rb.linearVelocity.x) > 0.01f;
            animator.SetBool("isWalking", walking);
        }
    }

    void FixedUpdate()
    {
        if (isPaused)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        float diffX = targetX - rb.position.x;
        float moveDir = 0f;

        if (hasTarget && Mathf.Abs(diffX) > stopThreshold)
        {
            moveDir = Mathf.Sign(diffX);

            // Short ray just in front of player to detect wall directly ahead
            Vector2 rayOrigin = rb.position + Vector2.up * 0.1f;
            Vector2 rayDir = new Vector2(moveDir, 0);
            float rayDist = 0.2f; // just enough to catch a wall immediately in front

            RaycastHit2D hitWall = Physics2D.Raycast(rayOrigin, rayDir, rayDist, wallMask);
            if (hitWall.collider != null)
            {
                moveDir = 0f;
                hasTarget = false; // stop moving when we hit the wall
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
        else
        {
            hasTarget = false;
        }

        float targetVelocityX = moveDir * maxSpeed;
        float velocityX = rb.linearVelocity.x;

        if (Mathf.Abs(targetVelocityX) > Mathf.Abs(velocityX))
            velocityX = Mathf.MoveTowards(velocityX, targetVelocityX, acceleration * Time.fixedDeltaTime);
        else
            velocityX = Mathf.MoveTowards(velocityX, targetVelocityX, deceleration * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(velocityX, rb.linearVelocity.y);

        // Flip sprite
        if (moveDir > 0) transform.localScale = new Vector3(1, 1, 1);
        if (moveDir < 0) transform.localScale = new Vector3(-1, 1, 1);

        // Stick to ground
        RaycastHit2D hitGround = Physics2D.Raycast(rb.position + Vector2.up * 0.1f, Vector2.down, groundCheckDistance, groundMask);
        if (hitGround.collider != null)
        {
            rb.AddForce(Vector2.down * stickForce);
        }
    }


    public void Pause()
    {
        isPaused = true;
        rb.linearVelocity = Vector2.zero;
        hasTarget = false;
        if (animator != null)
            animator.SetBool("isWalking", false);
    }

    public void UnPause()
    {
        isPaused = false;
    }

    void OnDrawGizmosSelected()
    {
        if (rb != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(rb.position + Vector2.up * 0.1f, rb.position + Vector2.up * 0.1f + Vector2.down * groundCheckDistance);

            if (hasTarget)
            {
                Gizmos.color = Color.red;
                float dir = Mathf.Sign(targetX - rb.position.x);
                Vector2 rayOrigin = rb.position + Vector2.up * 0.1f;
                Gizmos.DrawLine(rayOrigin, rayOrigin + new Vector2(dir * Mathf.Abs(targetX - rb.position.x), 0));
            }
        }
    }
}
