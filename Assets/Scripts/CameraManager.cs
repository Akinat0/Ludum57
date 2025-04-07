using System;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] float zoomStepSize = 2;
    [SerializeField] float zoomSmoothTime = 4;
    [SerializeField] SpriteRenderer terrain;
    Camera mainCamera;
    
    public Rect GetCameraRect()
    {
        float width = mainCamera.aspect * 2f * mainCamera.orthographicSize;
        float height = 2f * mainCamera.orthographicSize;

        return new Rect(transform.position.x - width / 2, transform.position.y - height / 2, width, height);
    } 
    
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


        Rect spriteRectInWorld = GameScene.GetSpriteRectInWorld(terrain);
        Rect cameraRect = GetCameraRect();

        if (cameraRect.xMin < spriteRectInWorld.xMin)
        {
            mainCamera.transform.position += Vector3.right * (spriteRectInWorld.xMin - cameraRect.xMin);
        }
        
        if (cameraRect.xMax > spriteRectInWorld.xMax)
        {
            mainCamera.transform.position += Vector3.left * (cameraRect.xMax - spriteRectInWorld.xMax);
        }
        
        if (cameraRect.yMax > spriteRectInWorld.yMax)
        {
            mainCamera.transform.position += Vector3.down * (cameraRect.yMax - spriteRectInWorld.yMax);
        }
        
        if (cameraRect.yMin < spriteRectInWorld.yMin)
        {
            mainCamera.transform.position += Vector3.up * (spriteRectInWorld.yMin - cameraRect.yMin);
        }

    }
}
