using UnityEngine;

public class StrategyCamera : MonoBehaviour
{
    [Header("⚠️ HELP ⚠️")]
    [Header("Arrow keys  – pan the camera")]
    [Header("Middle mouse – drag to rotate")]
    [Header("Scroll      – zoom")]
    [Header("Smooth movement, terrain‑following pivot")]
    [Header("No obstacle avoidance – camera clips through objects")]
    [Header("------------------------------------------------------------------------")]

    [Header("Movement")]
    public float panSpeed = 10f;
    public float rotationSensitivity = 5f;
    public float zoomSpeed = 5f;

    [Header("Smoothing")]
    public float panSmoothTime = 0.15f;
    public float rotationSmoothTime = 0.15f;
    public float zoomSmoothTime = 0.15f;
    public float heightSmoothTime = 0.15f;

    [Header("Terrain")]
    public bool followTerrain = true;
    public LayerMask groundMask = -1;
    public float pivotHeightOffset = 0f;

    [Header("Limits")]
    public float minPitch = 10f;
    public float maxPitch = 80f;
    public float minDistance = 2f;
    public float maxDistance = 50f;

    [Header("Optional Target (follow)")]
    public Transform target;

    // ─── state ────────────────────────────────────────────────────────────
    private Vector3 pivotPoint;
    private float yaw, pitch, distance;

    private Vector3 targetPivot;
    private float targetYaw, targetPitch, targetDistance, targetHeight;

    private Vector3 pivotVel;
    private float yawVel, pitchVel, zoomVel, heightVel;

    void Start()
    {
        // Initialize pivot from camera's current focus
        Ray ray = new Ray(transform.position, transform.forward);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        if (groundPlane.Raycast(ray, out float enter))
            pivotPoint = ray.GetPoint(enter);
        else
            pivotPoint = transform.position + transform.forward * 10f;

        Vector3 dir = (transform.position - pivotPoint).normalized;
        distance = Vector3.Distance(transform.position, pivotPoint);
        yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        pitch = Mathf.Asin(dir.y) * Mathf.Rad2Deg;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        targetPivot = pivotPoint;
        targetYaw = yaw;
        targetPitch = pitch;
        targetDistance = distance;
        targetHeight = pivotPoint.y;
    }

    void Update()
    {
        // ─── Pan (Arrow keys only) ──────────────────────────────────────
        float h = 0f, v = 0f;
        if (Input.GetKey(KeyCode.LeftArrow)) h = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) h = 1f;
        if (Input.GetKey(KeyCode.UpArrow)) v = 1f;
        if (Input.GetKey(KeyCode.DownArrow)) v = -1f;

        Vector3 move = (transform.forward * v + transform.right * h) * panSpeed * Time.deltaTime;
        move.y = 0f;
        if (move.magnitude > 0.001f)
            targetPivot += move;

        // ─── Rotation (Middle mouse drag) ──────────────────────────────
        if (Input.GetMouseButton(2))
        {
            targetYaw += Input.GetAxis("Mouse X") * rotationSensitivity;
            targetPitch -= Input.GetAxis("Mouse Y") * rotationSensitivity;
            targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
        }

        // ─── Zoom ────────────────────────────────────────────────────────
        targetDistance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);

        // ─── Follow target ──────────────────────────────────────────────
        if (target != null)
            targetPivot = target.position;

        // ─── Terrain height ──────────────────────────────────────────────
        if (followTerrain)
        {
            Vector3 origin = targetPivot + Vector3.up * 100f;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 200f, groundMask))
                targetHeight = hit.point.y + pivotHeightOffset;
            else
                targetHeight = targetPivot.y;
        }
        else
        {
            targetHeight = targetPivot.y;
        }

        // ─── Smooth pivot ────────────────────────────────────────────────
        Vector3 targetH = targetPivot;
        targetH.y = 0f;
        Vector3 currentH = pivotPoint;
        currentH.y = 0f;

        Vector3 smoothH = Vector3.SmoothDamp(currentH, targetH, ref pivotVel, panSmoothTime);
        float smoothHeight = Mathf.SmoothDamp(pivotPoint.y, targetHeight, ref heightVel, heightSmoothTime);

        pivotPoint = new Vector3(smoothH.x, smoothHeight, smoothH.z);

        // ─── Smooth angles & distance ────────────────────────────────────
        yaw = Mathf.SmoothDamp(yaw, targetYaw, ref yawVel, rotationSmoothTime);
        pitch = Mathf.SmoothDamp(pitch, targetPitch, ref pitchVel, rotationSmoothTime);
        distance = Mathf.SmoothDamp(distance, targetDistance, ref zoomVel, zoomSmoothTime);

        // ─── Direction ───────────────────────────────────────────────────
        float yawRad = yaw * Mathf.Deg2Rad;
        float pitchRad = pitch * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(
            Mathf.Sin(yawRad) * Mathf.Cos(pitchRad),
            Mathf.Sin(pitchRad),
            Mathf.Cos(yawRad) * Mathf.Cos(pitchRad)
        );

        // ─── Apply ────────────────────────────────────────────────────────
        transform.position = pivotPoint + dir * distance;
        transform.LookAt(pivotPoint);
    }

    // ─── Public methods ──────────────────────────────────────────────────
    public void SetPivot(Vector3 pos) => targetPivot = pos;
    public void SetTarget(Transform t) => target = t;
    public Vector3 GetPivot() => pivotPoint;

    public void ResetCamera(Vector3 position)
    {
        Vector3 dir = (position - pivotPoint).normalized;
        targetDistance = Vector3.Distance(position, pivotPoint);
        targetYaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        targetPitch = Mathf.Asin(dir.y) * Mathf.Rad2Deg;
        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
    }
}