using EvolveGames;
using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    [Header("⚠️ HELP ⚠️")]
    [Header("Makes this object smoothly rotate to face a target.")]
    [Header("")]
    [Header("Set lookAtTarget to any Transform (enemy, waypoint, etc).")]
    [Header("OR check lookAtPlayer to auto-detect and follow the player.")]
    [Header("")]
    [Header("Call SetTarget(Transform) to change target at runtime.")]
    [Header("Call SetLookAtPlayer(bool) to toggle player mode at runtime.")]
    [Header("------------------------------------------------------------------------")]

    [Header("Target")]
    [Tooltip("The transform to look at (drag any object here).")]
    public Transform lookAtTarget;

    [Tooltip("If checked, ignores lookAtTarget and automatically finds the player.")]
    public bool lookAtPlayer = false;

    [Header("Look Settings")]
    [Tooltip("If true, the object will also look up/down at the target.")]
    public bool lookOnAllAxes = true;

    [Header("Offset (Euler angles)")]
    [Tooltip("Additional rotation applied after looking at the target. " +
             "X = tilt up/down, Y = turn left/right, Z = roll.")]
    public Vector3 rotationOffset = Vector3.zero;

    [Header("Smoothing")]
    [Tooltip("How fast the object rotates toward the target. Higher = faster.")]
    public float rotationSpeed = 5f;

    [Tooltip("If true, uses Slerp (spherical) for smoother rotation. If false, uses Lerp.")]
    public bool useSlerp = true;

    private Transform currentTarget;

    void Start()
    {
        if (lookAtPlayer)
        {
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                currentTarget = playerController.transform;
                Debug.Log($"[LookAtTarget] Player found via PlayerController.");
            }
            else
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    currentTarget = playerObj.transform;
                    Debug.Log($"[LookAtTarget] Player found via 'Player' tag.");
                }
                else
                {
                    Debug.LogWarning($"[LookAtTarget] No player found! Set lookAtPlayer to false and assign a target manually.");
                }
            }
        }
        else
        {
            currentTarget = lookAtTarget;
        }
    }

    void Update()
    {
        if (lookAtPlayer)
        {
            if (currentTarget == null)
            {
                PlayerController playerController = FindObjectOfType<PlayerController>();
                if (playerController != null)
                    currentTarget = playerController.transform;
                else
                {
                    GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                    if (playerObj != null)
                        currentTarget = playerObj.transform;
                }
                return;
            }
        }
        else
        {
            currentTarget = lookAtTarget;
        }

        if (currentTarget == null)
            return;

        Vector3 direction;

        if (lookOnAllAxes)
        {
            direction = currentTarget.position - transform.position;
        }
        else
        {
            Vector3 targetPos = new Vector3(currentTarget.position.x, transform.position.y, currentTarget.position.z);
            direction = targetPos - transform.position;
        }

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            Quaternion targetRotation = lookRotation * Quaternion.Euler(rotationOffset);

            if (useSlerp)
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            else
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    // === Public Runtime Methods ===

    /// <summary>
    /// Manually set a target at runtime. Disables lookAtPlayer.
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        lookAtPlayer = false;
        lookAtTarget = newTarget;
        currentTarget = newTarget;
    }

    /// <summary>
    /// Toggle player tracking on/off at runtime.
    /// </summary>
    public void SetLookAtPlayer(bool value)
    {
        lookAtPlayer = value;
        if (value)
        {
            currentTarget = null;
        }
        else
        {
            currentTarget = lookAtTarget;
        }
    }
}