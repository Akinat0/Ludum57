using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
// ReSharper disable All

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
    [SerializeField] Transform pointPrefab;
    [SerializeField] Polyline line;

    List<Transform> points = new List<Transform>();

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
                
                
                if (Input.GetMouseButtonDown(0))
                {
                    Vector2 clickPoint = new Vector2(mouseWorldPoint.x, mouseWorldPoint.y);
                    
                    
                    bool pointDeleted = false;
                    
                    for (int i = 0; i < points.Count; i++)
                    {
                        Transform p = points[i];
                        
                        if (Vector2.Distance(clickPoint, new Vector2(p.transform.position.x, p.transform.position.y)) < 1)
                        {
                            Destroy(p.gameObject);
                            points.RemoveAt(i);
                            pointDeleted = true;
                            
                            break;
                        }
                    }

                    if (!pointDeleted)
                    {
                        Vector3 pos = mouseWorldPoint;
                        pos.z = terrain.transform.position.z - 1;
                        var point = Instantiate(pointPrefab, pos, Quaternion.identity);
                        point.parent = terrain.transform;
                    
                        points.Add(point);
                    }
                    
                    
                    RefreshLine();
                }
            }
            
            
            yield return null;
        }
    }


    void RefreshLine()
    {
        // line.positionCount = points.Count;

        List<Vector3> positions = new List<Vector3>();
        List<float>   distances = new List<float>();
        List<float>   depths    = new List<float>();
        List<Color>   colors    = new List<Color>();


        float dist = 0;
        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 targetPosition  = points[i + 1].transform.position;
            Vector3 currentPosition = points[i].transform.position;

            Vector3 direction = (targetPosition - currentPosition).normalized;
        
            while (targetPosition != currentPosition)
            {
                Vector3 nextPos = Vector3.MoveTowards(currentPosition, targetPosition, 1);
                dist += (nextPos - currentPosition).magnitude;
                
                positions.Add(currentPosition);
                distances.Add(dist);
                depths.Add(GetTerrainHeightAtLocation(currentPosition));
                
                currentPosition = nextPos;
            }
            
        }
        


        for (int i = 0; i < distances.Count; i++)
        {
            Color color = Color.Lerp(Color.red, Color.green, depths[i]);
            colors.Add(color);
        }


        line.points = positions;
        line.Colors = colors;
        line.Rebuild();
    }

    void OnDestroy()
    {
        Instance = null;
    }

    void OnApplicationQuit()
    {
        
    }
    
}