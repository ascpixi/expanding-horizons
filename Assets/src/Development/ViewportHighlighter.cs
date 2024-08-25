#if DEBUG
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewportHighlighter : MonoBehaviour
{
    SpriteRenderer rend;

    void Start()
    {
        rend = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        rend.color = ViewportBehaviour.Main.IsInViewport(transform.position) ? Color.green : Color.red;
    }
}
#endif