using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class Height : MonoBehaviour
{
    public Transform point;

    void Start()
    {
        point.DOPunchScale(new Vector3(.1f,.1f,.1f), 0.2f);
    }

    void LateUpdate()
    {
        GetComponentInChildren<TextMeshPro>().text = Mathf.RoundToInt(GameScene.Instance.GetTerrainHeightAtLocation(transform.position) * 1000) + "m";

        
    }

}
