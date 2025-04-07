using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Flare : MonoBehaviour
{
    
    void Start()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = Color.clear;
        transform.localScale = Vector3.zero;
        spriteRenderer.DOColor(Color.white, 0.2f).OnComplete(() => spriteRenderer.DOColor(Color.clear, 0.2f).OnComplete(() => Destroy(gameObject)));
        transform.DOPunchScale(Vector3.one, 0.39f, vibrato: 1);
    }

    
}
