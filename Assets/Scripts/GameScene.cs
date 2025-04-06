using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameScene : MonoBehaviour
{
    static GameScene Instance { get; set; }

    [SerializeField] SpriteRenderer terrain;
    [SerializeField] Texture2D terrainTexture;
    [SerializeField] Transform playerPoint;
    [SerializeField] Transform targetPoint;
    [SerializeField] RectTransform heightPivot;
    [SerializeField] TextMeshProUGUI heightText;
    [SerializeField] float height = 1000;

    public static Rect GetSpriteRectInWorld(SpriteRenderer spriteRenderer)
    {
        float width =  spriteRenderer.sprite.rect.width  / spriteRenderer.sprite.pixelsPerUnit;
        float height = spriteRenderer.sprite.rect.height / spriteRenderer.sprite.pixelsPerUnit;

        width  *= spriteRenderer.transform.lossyScale.x;
        height *= spriteRenderer.transform.lossyScale.y;
        
        float xPos = spriteRenderer.transform.position.x - width / 2;
        float yPos = spriteRenderer.transform.position.y - height / 2;
            
        return new Rect(xPos, yPos, width, height);
    }
    
    public static float Remap (float value, float from1, float to1, float from2, float to2) {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }


    float GetTerrainHeightAtLocation(Vector3 worldPoint)
    {
        Rect rect = GetSpriteRectInWorld(terrain);

        float u = Remap(worldPoint.x, rect.xMin, rect.xMax, 0, 1);
        float v = Remap(worldPoint.y, rect.yMin, rect.yMax, 0, 1);
        
        return terrainTexture.GetPixel((int)(u * terrainTexture.width), (int)(v * terrainTexture.height)).a;

    }

    IEnumerator Start()
    {
        while (true)
        {
            Vector3 mouseWorldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            if (Input.GetMouseButtonDown(0))
            {
            }

            if (Input.GetKey(KeyCode.Space))
            {
                float speed = 1;

                Vector3 newPos = playerPoint.transform.position + speed * Time.deltaTime * (targetPoint.position - playerPoint.position).normalized;

                float currentHeight = GetTerrainHeightAtLocation(playerPoint.transform.position);
                float targetHeight = GetTerrainHeightAtLocation(newPos);

                float delta = targetHeight - currentHeight;

                if (!Mathf.Approximately(delta, 0))
                {
                    print(delta);
                    speed /= 3;
                }
                
                newPos = playerPoint.transform.position + speed * Time.deltaTime * (targetPoint.position - playerPoint.position).normalized;
                playerPoint.transform.position = newPos;
            }

            if (EventSystem.current.IsPointerOverGameObject())
            {
                heightPivot.gameObject.SetActive(false);
            }
            else
            {
                heightPivot.position = Input.mousePosition + new Vector3(heightPivot.rect.width / 1.3f, -heightPivot.rect.height / 1.3f);
                heightPivot.gameObject.SetActive(true);
                heightText.text = Mathf.RoundToInt(GetTerrainHeightAtLocation(mouseWorldPoint) * height) + "m";
            }
            
            
            yield return null;
        }
    }

    void OnDestroy()
    {
        Instance = null;
    }

    void OnApplicationQuit()
    {
        
    }
    
}