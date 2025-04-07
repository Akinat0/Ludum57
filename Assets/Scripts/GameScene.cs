using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;


public class GameScene : MonoBehaviour
{
    public static GameScene Instance { get; set; }

    [SerializeField] SpriteRenderer terrain;
    [SerializeField] Texture2D terrainTexture;
    [SerializeField] Transform playerPoint;
    [SerializeField] List<Transform> targetPoints;
    [SerializeField] RectTransform heightPivot;
    [SerializeField] TextMeshProUGUI heightText;
    [SerializeField] float height = 1000;
    [SerializeField] Transform pointPrefab;
    [SerializeField] Polyline line;
    [SerializeField] float stepScale = 5;
    [SerializeField] Gradient gradient;
    [SerializeField] float heightToSpeed = 10; //meters per second?
    [SerializeField] Flare flarePrefab;
    [SerializeField] TextMeshProUGUI predictedTimeText;
    [SerializeField] TextMeshProUGUI predictedDistText;

    List<Transform> points = new List<Transform>();
    
    bool endPointConnected => connectedTargetPoint;
    Transform connectedTargetPoint;

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


    public float GetTerrainHeightAtLocation(Vector3 worldPoint)
    {
        Rect rect = GetSpriteRectInWorld(terrain);

        float u = Remap(worldPoint.x, rect.xMin, rect.xMax, 0, 1);
        float v = Remap(worldPoint.y, rect.yMin, rect.yMax, 0, 1);
        
        return terrainTexture.GetPixel((int)(u * terrainTexture.width), (int)(v * terrainTexture.height)).a;

    }

    void Awake()
    {
        Instance = this;
    }

    IEnumerator Start()
    {
        StartPath();
        
        while (true)
        {
            Vector3 mouseWorldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            

            if (Input.GetKey(KeyCode.Space))
            {
                /*float speed = 1;

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
                playerPoint.transform.position = newPos;*/
            }

            if (EventSystem.current.IsPointerOverGameObject())
            {
                heightPivot.gameObject.SetActive(false);
            }
            else
            {
                heightPivot.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + Vector3.forward * 5 + Vector3.down;// + new Vector3(heightPivot.rect.width / 1.3f, -heightPivot.rect.height / 1.3f));
                heightPivot.gameObject.SetActive(true);
                heightText.text = Mathf.RoundToInt(GetTerrainHeightAtLocation(mouseWorldPoint) * height) + "m";
                
                
                if (Input.GetMouseButtonDown(0))
                {
                    Vector2 clickPoint = new Vector2(mouseWorldPoint.x, mouseWorldPoint.y);
                    
                    
                    bool pointDeleted = false;

                    //    ADD/REMOVE TARGET POINT
                    foreach (var targetPoint in targetPoints)
                    {
                        if (Vector2.Distance(clickPoint, new Vector2(targetPoint.transform.position.x, targetPoint.transform.position.y)) < targetPoint.localScale.x)
                        {
                            if (!connectedTargetPoint)
                            {
                                points.Add(targetPoint);
                                connectedTargetPoint = targetPoint;
                                Instantiate(flarePrefab, targetPoint.transform.position, Quaternion.identity);
                            }
                            else if (targetPoint == connectedTargetPoint)
                            {
                                points.Remove(targetPoint);
                                connectedTargetPoint = null;
                                Instantiate(flarePrefab, targetPoint.transform.position, Quaternion.identity);
                            }
                            
                            pointDeleted = true;
                            break;
                        }
                    }

                    //    REMOVE WAYPOINT
                    if(!pointDeleted)
                    {
                        for (int i = 1 /* don't touch player*/ ; i < points.Count; i++)
                        {
                            Transform p = points[i];
                            

                            if (Vector2.Distance(clickPoint,
                                    new Vector2(p.transform.position.x, p.transform.position.y)) < p.localScale.x)
                            {
                                Instantiate(flarePrefab, p.transform.position, Quaternion.identity);
                                Destroy(p.gameObject);
                                points.RemoveAt(i);
                                pointDeleted = true;

                                break;
                            }
                        }
                    }

                    //    ADD WAYPOINT
                    if (!pointDeleted && !endPointConnected)
                    {
                        Vector3 pos = mouseWorldPoint;
                        pos.z = terrain.transform.position.z - 1;
                        var point = Instantiate(pointPrefab, pos, Quaternion.identity);
                        Instantiate(flarePrefab, pos, Quaternion.identity);
                        point.parent = terrain.transform;
                    
                        points.Add(point);
                    }
                    
                    
                    RefreshLine();
                }
            }
            
            
            yield return null;
        }
    }


    void StartPath()
    {
        points.Clear();
        points.Add(playerPoint);
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

        
            while (targetPosition != currentPosition)
            {
                Vector3 nextPos = Vector3.MoveTowards(currentPosition, targetPosition, (float)stepScale / terrain.sprite.pixelsPerUnit);
                dist += (nextPos - currentPosition).magnitude;
                
                positions.Add(currentPosition);
                distances.Add(dist);
                depths.Add(GetTerrainHeightAtLocation(currentPosition));
                
                currentPosition = nextPos;
            }
            
        }



        float timeToPass = 0;
        for (int i = 0; i < distances.Count; i++)
        {
            float heightDiff = 0;
            float d = 0;
            
            if (i != 0)
            {
                heightDiff = depths[i]  - depths[i - 1];
                d = distances[i] - distances[i - 1]; //distance
            }
            
            
            Color color = i == 0 ? gradient.Evaluate(0) : gradient.Evaluate(Mathf.Abs(heightDiff) * 100);
            
            colors.Add(color);


            float speed = heightToSpeed * Mathf.Abs(heightDiff);
            
            if(Mathf.Approximately(speed, 0))
                continue;
            
            float time = d / speed;
            timeToPass += time;
        }

        
        predictedTimeText.text = Mathf.Round(timeToPass) + " min" ;
        predictedDistText.text = Mathf.Round(dist) + " km";
        


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