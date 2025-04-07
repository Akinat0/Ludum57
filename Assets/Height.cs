using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Height : MonoBehaviour
{
    void Start()
    {
        GetComponentInChildren<TextMeshPro>().text = Mathf.RoundToInt(GameScene.Instance.GetTerrainHeightAtLocation(transform.position) * 1000) + "m";
    }

}
