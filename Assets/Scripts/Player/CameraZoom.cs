using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [Header("Zoom Settings")]
    public float zoomedFOV = 30f; 
    public float normalFOV = 60f;
    public float zoomSpeed = 10f;
    public bool canZoom = true;
    private Camera cam;
    public void ZoomToggle(bool zoom)
    {
        canZoom = zoom;
    }
    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("CameraZoom script must be attached to a Camera!");
        }
    }

    void Update()
    {
        if (cam == null) return;
        if (canZoom == false) return;
        if (Input.GetMouseButton(1))
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, zoomedFOV, Time.deltaTime * zoomSpeed);
        }
        else
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, normalFOV, Time.deltaTime * zoomSpeed);
        }
    }
}
