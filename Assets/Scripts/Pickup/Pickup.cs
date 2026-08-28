using EvolveGames;
using UnityEngine;
using UnityEngine.Events;

public class Pickup : MonoBehaviour
{
    [Header("Key Bindings")]
    public KeyCode PickupDropKey = KeyCode.Mouse0;
    public KeyCode RotateKey = KeyCode.Mouse1;
    public KeyCode ThrowKey = KeyCode.F;

    [Header("Raycast Settings (uses RayCaster on camera)")]
    public RayCaster rayCaster; // Assign in inspector or will be found on camera

    [Header("Hold Position (camera‑relative)")]
    public float HoldDistance = 2.0f;
    public float HoldVerticalOffset = -0.5f;
    public float HoldHorizontalOffset = 0f;

    [Header("Physics – Simple Force")]
    public float PickupForce = 150f;
    public float MaxForce = 300f;

    [Header("Rotation")]
    public float BaseRotationSpeed = 5f;
    public float MassEffectOnRotation = 0.5f;
    private bool isRotating = false;

    [Header("Auto Drop")]
    public float MaxHoldDistance = 15f;
    public bool EnableAutoDrop = true;

    [Header("Throw Settings")]
    public bool EnableThrow = true;
    [Range(1, 50)] public float MinThrowForce = 1f;
    [Range(1, 50)] public float MaxThrowForce = 10f;
    public float ChargeTime = 1.5f;
    public float ThrowForceMultiplier = 20f;

    [Header("Animation (optional)")]
    public Animator HandsAnimator;

    [Header("Events")]
    public UnityEvent OnPickup;
    public UnityEvent OnDrop;
    public UnityEvent OnThrow;

    private Pickupable currentPickupable;
    private Transform holdTarget;
    private Camera activeCamera;
    private bool isChargingThrow = false;
    private float currentForce = 1f;
    private PlayerController playerController;

    private bool originalGravityDuringRotation;

    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();

        activeCamera = Camera.main != null ? Camera.main : FindObjectOfType<Camera>();
        if (activeCamera == null)
        {
            Debug.LogError("Pickup: No camera found!");
            enabled = false;
            return;
        }

        // Find RayCaster if not assigned
        if (rayCaster == null)
        {
            rayCaster = activeCamera.GetComponent<RayCaster>();
            if (rayCaster == null)
                Debug.LogWarning("Pickup: No RayCaster component found on camera. Please assign one.");
        }

        holdTarget = new GameObject("HoldTarget").transform;
        holdTarget.SetParent(activeCamera.transform);
        UpdateHoldTargetPosition();
    }

    void UpdateHoldTargetPosition()
    {
        if (holdTarget != null)
            holdTarget.localPosition = new Vector3(HoldHorizontalOffset, HoldVerticalOffset, HoldDistance);
    }

    void FixedUpdate()
    {
        if (currentPickupable != null && !isRotating && holdTarget != null)
            MoveObject();
    }

    void Update()
    {
        if (rayCaster == null) return;

        if (holdTarget != null && (holdTarget.localPosition.x != HoldHorizontalOffset ||
                                   holdTarget.localPosition.y != HoldVerticalOffset ||
                                   holdTarget.localPosition.z != HoldDistance))
            UpdateHoldTargetPosition();

        if (EnableAutoDrop && currentPickupable != null)
        {
            float dist = Vector3.Distance(holdTarget.position, currentPickupable.transform.position);
            if (dist > MaxHoldDistance)
            {
                DropObject();
                return;
            }
        }

        // Use RayCaster's HitObject instead of doing our own raycast
        GameObject hitGameObject = rayCaster.HitObject;
        bool validHit = (hitGameObject != null && hitGameObject != rayCaster.TempCollider);

        if (Input.GetKeyDown(PickupDropKey))
        {
            if (currentPickupable == null)
            {
                if (validHit) PickUpObject(hitGameObject);
            }
            else DropObject();
        }

        // ---------- THROW CHECK WITH throwable ----------
        if (EnableThrow && currentPickupable != null && currentPickupable.throwable)
        {
            if (Input.GetKey(ThrowKey))
            {
                if (!isChargingThrow)
                {
                    isChargingThrow = true;
                    currentForce = MinThrowForce;
                }
                else
                {
                    currentForce += (MaxThrowForce - MinThrowForce) * (Time.deltaTime / ChargeTime);
                    currentForce = Mathf.Clamp(currentForce, MinThrowForce, MaxThrowForce);
                }
            }
            else if (isChargingThrow && Input.GetKeyUp(ThrowKey))
            {
                ThrowObject(currentForce);
                isChargingThrow = false;
            }
        }
        else if (isChargingThrow) isChargingThrow = false;

        if (currentPickupable != null)
        {
            if (Input.GetKeyDown(RotateKey) && !isRotating) StartRotation();
            if (Input.GetKeyUp(RotateKey) && isRotating) StopRotation();
        }

        if (isRotating && currentPickupable != null)
        {
            float mass = currentPickupable.rb.mass;
            float factor = 1f / (1f + mass * MassEffectOnRotation);
            float speed = BaseRotationSpeed * factor;
            float mouseX = Input.GetAxis("Mouse X") * speed;
            float mouseY = Input.GetAxis("Mouse Y") * speed;

            Quaternion deltaRot = Quaternion.Euler(-mouseY, mouseX, 0);
            currentPickupable.rb.MoveRotation(currentPickupable.rb.rotation * deltaRot);
        }

        if (HandsAnimator != null)
            HandsAnimator.SetBool("Hold", currentPickupable != null);
    }

    void StartRotation()
    {
        isRotating = true;
        if (currentPickupable.rb != null)
        {
            originalGravityDuringRotation = currentPickupable.rb.useGravity;
            currentPickupable.rb.useGravity = false;
        }
        playerController?.Pause();
    }

    void StopRotation()
    {
        isRotating = false;
        if (currentPickupable?.rb != null)
        {
            currentPickupable.rb.useGravity = originalGravityDuringRotation;
        }
        playerController?.UnPause();
    }

    void MoveObject()
    {
        Rigidbody rb = currentPickupable.rb;
        if (rb == null || holdTarget == null) return;

        Vector3 currentPos = currentPickupable.transform.position;
        Vector3 targetPos = holdTarget.position;

        Vector3 moveDir = targetPos - currentPos;

        float distance = moveDir.magnitude;
        if (distance < 0.001f) return;

        Vector3 dir = moveDir / distance;

        // ONLY BLOCK DESTRUCTIVE COLLISION
        if (Physics.Raycast(
            currentPos,
            dir,
            out RaycastHit hit,
            distance,
            ~0,
            QueryTriggerInteraction.Ignore
        ))
        {
            float dot = Vector3.Dot(dir, hit.normal);
            if (dot < -0.2f)
            {
                moveDir = Vector3.ProjectOnPlane(moveDir, hit.normal);
            }
        }

        Vector3 force = moveDir * PickupForce;
        force = Vector3.ClampMagnitude(force, MaxForce);

        rb.AddForce(force, ForceMode.Force);
    }

    bool TryGetSafeTargetPosition(Rigidbody rb, Vector3 desiredTarget, out Vector3 safeTarget)
    {
        safeTarget = desiredTarget;
        Vector3 direction = desiredTarget - rb.position;
        float distance = direction.magnitude;
        if (distance < 0.01f) return true;

        RaycastHit hit;
        if (rb.SweepTest(direction.normalized, out hit, distance, QueryTriggerInteraction.Ignore))
        {
            safeTarget = rb.position + direction.normalized * (hit.distance - 0.05f);
            return false;
        }
        return true;
    }

    void PickUpObject(GameObject pickObj)
    {
        Pickupable p = pickObj.GetComponent<Pickupable>();
        if (p == null) return;

        if (pickObj.GetComponent<InvokerTimer>() != null)
            pickObj.GetComponent<InvokerTimer>().StartTimer();

        OnPickup?.Invoke();

        currentPickupable = p;
        currentPickupable.PickUp();
    }

    void DropObject()
    {
        if (currentPickupable == null) return;
        if (isRotating) StopRotation();
        if (isChargingThrow) isChargingThrow = false;

        PopOutOfGround(currentPickupable);

        currentPickupable.Drop();
        currentPickupable = null;

        OnDrop?.Invoke();
    }

    void ThrowObject(float forceValue)
    {
        if (currentPickupable == null) return;
        if (isRotating) StopRotation();

        float actualForce = forceValue * ThrowForceMultiplier;
        Vector3 direction = activeCamera.transform.forward;

        PopOutOfGround(currentPickupable);

        currentPickupable.Throw(direction * actualForce);
        currentPickupable = null;
        isChargingThrow = false;

        OnThrow?.Invoke();
    }

    void PopOutOfGround(Pickupable p)
    {
        float checkDistance = 0.3f;
        if (Physics.Raycast(p.transform.position, Vector3.down, out RaycastHit hit, checkDistance))
        {
            p.transform.position += Vector3.up * (checkDistance - hit.distance + 0.02f);
        }
    }

    public void CanRayFalse() => CanRay = false;
    public void CanRayTrue() => CanRay = true;

    // Obsolete: kept for compatibility but no longer used
    [System.Obsolete("Raycasting is now handled by RayCaster component")]
    public bool CanRay = true;
}
