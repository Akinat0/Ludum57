using System;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] float zoomStepSize = 2;
    [SerializeField] float zoomSmoothTime = 4;
    Camera mainCamera;
    
    void Awake()
    {
        mainCamera = Camera.main;
    }

    float currentZoom = 5;
    float targetZoom  = 3;
    float currentZoomVelocity;

    Vector2 prevMousePos;
    
    float homeZoomVelocity;
    Vector2 homePosVelocity;

    
    void LateUpdate()
    {
        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButton(1))
        {
            // >>>>>>> MOUSE DRAG SCROLL <<<<<<<<
            Vector2 delta = mousePos - prevMousePos;
            transform.position -= new Vector3(delta.x, delta.y);
            //update mouse pos
            mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        }
    
        // >>>>>>> UPDATE ZOOM <<<<<<<<    

        
        targetZoom -= Input.mouseScrollDelta.y;
        targetZoom = Mathf.Clamp(targetZoom, 1, 5);
        currentZoom = Mathf.SmoothDamp(currentZoom, targetZoom, ref currentZoomVelocity, zoomSmoothTime);
        mainCamera.orthographicSize = currentZoom * zoomStepSize;
    

        prevMousePos = mousePos;

    }
}
