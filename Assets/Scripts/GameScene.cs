using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


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
    [SerializeField] Polyline linePrefab;
    [SerializeField] float stepScale = 5;
    [SerializeField] Gradient gradient;
    [SerializeField] float heightToSpeed = 10; //meters per second?
    [SerializeField] Flare flarePrefab;
    [SerializeField] TextMeshProUGUI predictedTimeText;
    [SerializeField] TextMeshProUGUI predictedDistText;
    [SerializeField] TextMeshProUGUI predictedElevationText;
    [SerializeField] TextMeshProUGUI pointRemainsText;
    [SerializeField] TextMeshProUGUI pointCurrText;
    [SerializeField] TextMeshProUGUI totalDistText;
    [SerializeField] TextMeshProUGUI totalTimeText;
    [SerializeField] float playerMoveSpeed = 10;
    [SerializeField] Sprite targetPointDisabledSprite;
    [SerializeField] GameObject failScreen;
    [SerializeField] GameObject uiToDisable;

    public int pointsRemains = 10;
    public int currentPoints = 0;
    public float totalMinutes = 0;
    public float totalDistance = 0;
    float predictedDistance;
    float predictedTime;

    HashSet<Transform> visitedPoints = new HashSet<Transform>();
    List<Transform> points = new List<Transform>();
    Polyline line;
    
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


    void UpdateTotalTime()
    {
        int hours = Mathf.RoundToInt(totalMinutes) / 60 + 6;
        int minutes = Mathf.RoundToInt(totalMinutes) % 60;

        totalTimeText.color = hours > 22 ? Color.red : new Color(0.6196079f, 0.6039216f,0.6039216f );
            
        
        if (hours >= 24)
        {
            Failed();
        }
        
        totalTimeText.text = string.Format("{0:D2}:{1:D2}", hours, minutes);
        
    }

    IEnumerator Start()
    {
        totalDistText.text = 0 + " km";
        UpdateTotalTime();

        StartPath();
        
        while (true)
        {
            Vector3 mouseWorldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            

            if (pendingClear)
            {
                pendingClear = false;

                foreach (var p in points)
                {
                    if (p != playerPoint && !targetPoints.Contains(p))
                    {
                        Instantiate(flarePrefab, p.position, Quaternion.identity);
                        Destroy(p.gameObject);
                    }
                }
                
                points.Clear();
                DestroyImmediate(line.gameObject);
                
                line = null;
                pointsRemains += currentPoints;
                connectedTargetPoint = null;
                StartPath();
                continue;
            }
            
            if (Input.GetKeyDown(KeyCode.Space) || pendingWalk)
            {
                if (points.Count > 1)
                {
                    StartCoroutine(TimeDistanceRoutine());
                    yield return CharacterWalk();
                    pendingWalk = false;
                    continue;
                }
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


                    if (Vector2.Distance(clickPoint,new Vector2(playerPoint.transform.position.x, playerPoint.transform.position.y)) < playerPoint.localScale.x * 1.1f)
                    {
                        //ignore click on player
                        pointDeleted = true;
                    }
                    else
                    {
                        //    ADD/REMOVE TARGET POINT
                        foreach (var targetPoint in targetPoints)
                        {
                            if (Vector2.Distance(clickPoint,
                                    new Vector2(targetPoint.transform.position.x, targetPoint.transform.position.y)) <
                                targetPoint.localScale.x)
                            {
                                if (!connectedTargetPoint && pointsRemains > 0)
                                {
                                    points.Add(targetPoint);
                                    connectedTargetPoint = targetPoint;
                                    Instantiate(flarePrefab, targetPoint.transform.position, Quaternion.identity);
                                    pointsRemains--;
                                    currentPoints++;
                                }
                                else if (targetPoint == connectedTargetPoint)
                                {
                                    points.Remove(targetPoint);
                                    connectedTargetPoint = null;
                                    Instantiate(flarePrefab, targetPoint.transform.position, Quaternion.identity);
                                    pointsRemains++;
                                    currentPoints--;
                                }

                                pointDeleted = true;
                                break;
                            }
                        }
                    }

                    //    REMOVE WAYPOINT
                    if(!pointDeleted)
                    {
                        for (int i = 1 /* don't touch player */ ; i < points.Count; i++)
                        {
                            Transform p = points[i];
                            

                            if (Vector2.Distance(clickPoint,
                                    new Vector2(p.transform.position.x, p.transform.position.y)) < p.localScale.x)
                            {
                                Instantiate(flarePrefab, p.transform.position, Quaternion.identity);
                                Destroy(p.gameObject);
                                points.RemoveAt(i);
                                pointsRemains++;
                                currentPoints--;
                                pointDeleted = true;

                                break;
                            }
                        }
                    }

                    //    ADD WAYPOINT
                    if (!pointDeleted && !endPointConnected && pointsRemains > 0)
                    {
                        Vector3 pos = mouseWorldPoint;
                        pos.z = terrain.transform.position.z - 1;
                        var point = Instantiate(pointPrefab, pos, Quaternion.identity);
                        Instantiate(flarePrefab, pos, Quaternion.identity);
                        point.parent = terrain.transform;
                        pointsRemains--;
                        currentPoints++;
                    
                        points.Add(point);
                    }

                    pointRemainsText.text = pointsRemains.ToString();
                    pointCurrText.text = currentPoints.ToString();
                    
                    RefreshLine();
                }
            }
            
            
            yield return null;
        }
    }


    IEnumerator CharacterWalk()
    {
        for (int i = 1; i < points.Count; i++)
        {
            Transform p = points[i];
            
            while (p.position != playerPoint.position)
            {
                playerPoint.position = Vector3.MoveTowards(playerPoint.position, p.position, playerMoveSpeed * Time.deltaTime);
                yield return null;
            }
        }
        
        if (connectedTargetPoint)
        {
            visitedPoints.Add(connectedTargetPoint);
            connectedTargetPoint = null;

            if (visitedPoints.Count == targetPoints.Count)
            {
                //TODO WINWINWIWNWIWNWIWNWIWWNNWNW
                Restart();
            }
        }
        
        
        StartPath();
    }

    IEnumerator TimeDistanceRoutine()
    {
        float duration = predictedDistance / playerMoveSpeed;
        float t = 0;
        float prevDist = totalDistance;
        float prevTime = totalMinutes;
        
        while (t < duration)
        {
            t += Time.deltaTime;
            totalDistance = Mathf.Lerp(prevDist, prevDist + predictedDistance, t / duration);
            totalMinutes = Mathf.Lerp(prevTime, prevTime + predictedTime, t / duration);
            UpdateTotalTime();
            totalDistText.text = Mathf.RoundToInt(totalDistance) + " km";
            yield return null;
        }
        
        totalMinutes = prevTime + predictedTime;
        totalDistance = prevDist + predictedDistance;
        totalDistText.text = Mathf.RoundToInt(totalDistance) + " km";
        UpdateTotalTime();
    }

    void StartPath()
    {
        if (pointsRemains == 0)
            Failed();
        
        currentPoints = 0;
        pointCurrText.text = currentPoints.ToString();
        pointRemainsText.text = pointsRemains.ToString();
        predictedElevationText.text = 0.ToString() + "m";
        predictedDistText.text = 0 + " km";
        predictedTimeText.text = 0 + " min";
        if (line)
        {
            line.thickness *= 0.5f;
            line.Rebuild();
            line.SetDisabledMat();
        }

        for (int i = 1; i < points.Count; i++)
        {
            if (targetPoints.Contains(points[i]))
            {
                points[i].GetComponent<SpriteRenderer>().sprite = targetPointDisabledSprite;
                points[i].localScale = Vector3.one * 0.2f;
                
            }
            else
            {
                points[i].localScale *= 0.45f;
                var h = points[i].GetComponentInChildren<Height>();
                if(h)
                    h.gameObject.SetActive(false);
            }
        }
        
        
        points.Clear();
        points.Add(playerPoint);
        line = Instantiate(linePrefab, terrain.transform);
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
        float totalPredictedHeight = 0;
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
            totalPredictedHeight += Mathf.Abs(heightDiff * 1000);
        }

        
        predictedTimeText.text = Mathf.Round(timeToPass) + " min" ;
        predictedDistText.text = Mathf.Round(dist) + " km";
        predictedDistance = dist;
        predictedTime = timeToPass;
        predictedElevationText.text = Mathf.Round(totalPredictedHeight) + "m";
        


        line.points = positions;
        line.Colors = colors;
        line.Rebuild();
        
        
    }

    void OnDestroy()
    {
        Instance = null;
    }
    
    void Failed()
    {
        failScreen.SetActive(true);
    }

    public void SetUIVisibility()
    {
        if (uiToDisable.activeSelf)
            uiToDisable.SetActive(false);
        else uiToDisable.SetActive(true);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    bool pendingWalk;
    bool pendingClear;
    public void StartWalking()
    {
        if (points.Count > 0)
            pendingWalk = true;
    }
    
    public void Clear()
    {
        pendingClear = true;
    }
}