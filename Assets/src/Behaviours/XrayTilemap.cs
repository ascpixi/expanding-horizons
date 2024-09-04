using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class XrayTilemap : MonoBehaviour
{
    public GameObject CloneTarget;
    public Material XrayMaterial;
    
    static readonly int playerPos = Shader.PropertyToID("_PlayerPos");

    void Start()
    {
        var cloned = Instantiate(CloneTarget, transform);
        var rend = cloned.GetComponent<TilemapRenderer>();
        rend.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
        rend.material = XrayMaterial;
        
        Destroy(cloned.GetComponent<TilemapCollider2D>());
    }

    void Update()
    {
        XrayMaterial.SetVector(playerPos, PlayerController.Main.Position);
    }
}
