using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class WandPickupBehaviour : MonoBehaviour
{
    public Vector2 PickupRegion;
    public TutorialSequence Sequence;
    
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, PickupRegion);
    }
#endif
    
    void Update()
    {
        var player = PlayerController.Main;
        var region = new Bounds(transform.position.WithZ(0), PickupRegion);

        if (!region.Contains(player.Position.WithZ(0)))
            return;

        Sequence.Begin();
        Destroy(gameObject);
    }
}
