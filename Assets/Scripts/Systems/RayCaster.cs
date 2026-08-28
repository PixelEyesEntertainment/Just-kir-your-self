using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCaster : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float Distance = 10f; // Maximum distance for the ray
    public LayerMask IgnoreLayers; // Layers to ignore during raycast

    [Header("Debug Information")]
    public GameObject HitObject; // The object hit by the ray
    public GameObject TempCollider; // Default object if nothing is hit

    private void Start()
    {
        // Initialize HitObject to TempCollider by default
        HitObject = TempCollider;
    }

    void FixedUpdate()
    {
        // Invert IgnoreLayers to only hit layers not specified
        int layerMask = ~IgnoreLayers;

        // Perform the raycast
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Distance, layerMask))
        {
            // Debug ray in yellow for hits
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            HitObject = hit.collider.gameObject;
        }
        else
        {
            // Debug ray in white for no hits
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * Distance, Color.white);
            HitObject = TempCollider;
        }
    }
}
