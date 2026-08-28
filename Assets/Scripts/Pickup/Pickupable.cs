using UnityEngine;
using UnityEngine.Events;

public class Pickupable : MonoBehaviour
{
    [Header("⚠️ Help ⚠️")]
    [Header("You have to add a layer for when you holding an object so player won't be able to jump on it")]
    [Header("After u added Select it in Pickupable scripts and add it as No Jump Layer in PlayerController")]
    [Header("------------------------------------------------------------------------")]

    [Header("Layer Management")]
    public LayerMask heldLayer = 6;
    public bool restoreOriginalLayer = true;

    [Header("Weightless")]
    public bool weightless = false;   // When held, movement/rotation ignores actual mass

    [Header("Throwable")]
    public bool throwable = true;     // Can this object be thrown?

    [Header("Events")]
    public UnityEvent OnPickedUp;
    public UnityEvent OnDropped;
    public UnityEvent<float> OnThrown;

    [HideInInspector] public Rigidbody rb;

    private int originalLayer;
    private bool originalGravityState;
    private float originalLinearDamping;
    private float originalAngularDamping;
    private CollisionDetectionMode originalCollisionMode;
    private float originalMass;
    private bool wasKinematic;        // store if it was kinematic before pickup

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) Debug.LogError($"Pickupable on {gameObject.name} needs a Rigidbody!");

        originalLayer = gameObject.layer;
        originalGravityState = rb.useGravity;
        originalLinearDamping = rb.linearDamping;
        originalAngularDamping = rb.angularDamping;
        originalCollisionMode = rb.collisionDetectionMode;
        originalMass = rb.mass;
        wasKinematic = rb.isKinematic;
    }

    public void PickUp()
    {
        // If the object was kinematic, we make it non-kinematic so physics can move it.
        // This change is permanent – we will NOT restore kinematic on drop/throw.
        if (rb.isKinematic)
        {
            rb.isKinematic = false;
        }

        if (restoreOriginalLayer)
        {
            int layerIndex = GetLayerIndexFromMask(heldLayer);
            gameObject.layer = layerIndex;
        }

        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.linearDamping = 10f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Weightless: ignore real mass while held
        if (weightless)
            rb.mass = 1f;

        OnPickedUp?.Invoke();
    }

    public void Drop()
    {
        Restore();
        OnDropped?.Invoke();
    }

    public void Throw(Vector3 force)
    {
        // Restore original mass BEFORE applying force so throw feels realistic
        rb.mass = originalMass;

        // Apply force while constraints are still frozen (original behaviour)
        rb.AddForce(force, ForceMode.Impulse);

        // Restore everything else (gravity, damping, collision mode, layer, etc.)
        Restore();

        OnThrown?.Invoke(force.magnitude);
    }

    private void Restore()
    {
        if (restoreOriginalLayer) gameObject.layer = originalLayer;
        rb.useGravity = originalGravityState;
        rb.constraints = RigidbodyConstraints.None;
        rb.linearDamping = originalLinearDamping;
        rb.angularDamping = originalAngularDamping;
        rb.collisionDetectionMode = originalCollisionMode;
        rb.mass = originalMass;

        // NOTE: We do NOT restore isKinematic. Once we disable kinematic during pickup,
        // it stays non-kinematic forever (or until you manually change it elsewhere).
    }

    private int GetLayerIndexFromMask(LayerMask mask)
    {
        int maskValue = mask.value;
        for (int i = 0; i < 32; i++)
        {
            if ((maskValue & (1 << i)) != 0)
                return i;
        }
        Debug.LogError($"Invalid LayerMask {mask} – no layers set!");
        return 0;
    }
}