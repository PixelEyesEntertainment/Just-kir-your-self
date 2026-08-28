using UnityEngine;

public class RayCaster2D : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float Distance = 10f;
    public LayerMask IgnoreLayers;

    [Header("Debug Information")]
    public GameObject HitObject;
    public GameObject TempCollider;

    private void Start()
    {
        HitObject = TempCollider;
    }

    void FixedUpdate()
    {
        int layerMask = ~IgnoreLayers;

        // 2D raycast
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            transform.right,          // use transform.right for 2D (change to transform.up if needed)
            Distance,
            layerMask
        );

        if (hit.collider != null)
        {
            Debug.DrawRay(transform.position, transform.right * hit.distance, Color.yellow);
            HitObject = hit.collider.gameObject;
        }
        else
        {
            Debug.DrawRay(transform.position, transform.right * Distance, Color.white);
            HitObject = TempCollider;
        }
    }
}