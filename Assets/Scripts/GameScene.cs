using System.Collections;
using UnityEngine;

public class GameScene : MonoBehaviour
{
    static GameScene Instance { get; set; }

    

    IEnumerator Start()
    {
        while (true)
        {

            if (Input.GetMouseButtonDown(0))
            {
                
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