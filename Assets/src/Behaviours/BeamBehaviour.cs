using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamBehaviour : MonoBehaviour
{
    public bool IsPrimary;
    public float Speed;
    public LayerMask CollisionMask;
    public Vector2 CollisionSize;
    public string ViewportSideTag;
    public float InitialCheckSize;
    
    public PlayerController Source { get; set; }
    
    public Vector2 Velocity { get; set; }
    
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, CollisionSize.WithZ(1));
    }
#endif

    void Start()
    {
        // If we encounter an obstacle right away, that means that the player is shooting into
        // an obstacle right in front of them, for some reason
        var diff = (CollisionSize * InitialCheckSize) - CollisionSize;
        var overlap = Physics2D.OverlapBox(
            transform.position,
            CollisionSize + (diff * Velocity).Abs(),
            0f,
            CollisionMask
        );
        
        if (overlap) {
            Debugging.DrawBox(transform.position, CollisionSize + (diff * Velocity).Abs(), Color.red);
            HitDestroy();
            PlayerController.Main.BeamsLeft++;
        }
    }

    ViewportSide GetSideByPointBox(Vector2 pos)
    {
        var viewport = ViewportBehaviour.Main;
        ViewportSide side;

        side = viewport.GetSideByPoint(pos);
        if (side != ViewportSide.None) return side;
        
        side = viewport.GetSideByPoint(pos.Add(0.25f, 0f));
        if (side != ViewportSide.None) return side;
        
        side = viewport.GetSideByPoint(pos.Add(-0.25f, 0f));
        if (side != ViewportSide.None) return side;

        side = viewport.GetSideByPoint(pos.Add(0f, -0.25f));
        if (side != ViewportSide.None) return side;
        
        side = viewport.GetSideByPoint(pos.Add(0f, -0.25f));
        return side;
    }
    
    void Update()
    {
        var prevPos = transform.position;
        transform.position += Velocity.XYZ() * (Speed * Time.deltaTime);

        var area = new Bounds(transform.position, CollisionSize);
        area.Encapsulate(prevPos); // extend to the previous position, so that we don't skip the area we traveled
        
        var overlap = Physics2D.OverlapBox(area.center, area.size, 0f, CollisionMask);
        if (!overlap)
            return;

        if (overlap.CompareTag(ViewportSideTag)) {
            var viewport = ViewportBehaviour.Main;
            
            var hitSide = GetSideByPointBox(transform.position);
            if (hitSide == ViewportSide.None) {
                Debug.LogWarning($"GetSideByPointBox(new Vector2({transform.position})) returned None when we're sure there was a collision!");
                return;
            }
            
            Source.Audio.PlayOneShot(Source.HitSfx.Random(), Source.HitSfxVolume);
            
            if (hitSide != viewport.CurrentSide) {
                viewport.ChangeCurrentSide(hitSide);
            }
            
            if (IsPrimary) {
                viewport.Shrink();
            }
            else {
                viewport.Expand();
            }
        }
        else {
            PlayerController.Main.BeamsLeft++;
            Source.Audio.PlayOneShot(Source.MissSfx.Random(), Source.MissSfxVolume);
        }
        
        HitDestroy();
    }

    void HitDestroy()
    {
        // TODO: particles
        Destroy(gameObject);
    }
}
