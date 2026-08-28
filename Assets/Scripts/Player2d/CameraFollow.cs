using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("⚠️ Help ⚠️")]
    [Header("Smoothly follows a target (usually the player) with a configurable offset.")]
    [Header("Supports clamping within specified world bounds to restrict camera movement.")]
    [Header("Ideal for 2D games to keep the camera focused on the player while respecting level limits.")]
    [Header("Call StartFollow() or StopFollow() to handle camera.")]
    [Header("------------------------------------------------------------------------")]

    public Transform target;
    public Vector3 offset;

    [Tooltip("Time it takes to smooth to target. Smaller = faster response.")]
    public float smoothTime = 0.25f;

    public float minX = -10f;
    public float maxX = 10f;
    public float minY = -5f;
    public float maxY = 5f;

    private bool isFollowing = true;
    private Vector3 stopTargetPosition;
    private Vector3 velocity = Vector3.zero; // used by SmoothDamp

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition;

        if (isFollowing)
        {
            // Update last known position
            stopTargetPosition = target.position + offset;
            stopTargetPosition.z = transform.position.z;

            stopTargetPosition.x = Mathf.Clamp(stopTargetPosition.x, minX, maxX);
            stopTargetPosition.y = Mathf.Clamp(stopTargetPosition.y, minY, maxY);

            desiredPosition = stopTargetPosition;
        }
        else
        {
            desiredPosition = stopTargetPosition;
        }

        // SmoothDamp is smoother and avoids jitter
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
    }

    public void StartFollow() => isFollowing = true;
    public void StopFollow() => isFollowing = false;
}
