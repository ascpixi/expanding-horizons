using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Handles player particles.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(MovementController2D))]
public class PlayerParticleController : MonoBehaviour
{
    static readonly Quaternion oneEightyDegrees = Quaternion.Euler(0, 0, 180);

    [Header("Particle System Setup")]
    public ParticleSystem JumpParticles;
    public ParticleSystem LandParticles;
    public ParticleSystem WallParticles;
    
    MovementController2D mvmt;

    private void Start()
    {
        mvmt = GetComponent<MovementController2D>();
    }

    /// <summary>
    /// Scales the emitter of a particle system relative to the player's position,
    /// so that it never goes over the surface the player is standing on.
    /// </summary>
    /// <param name="ps">The target particle system.</param>
    void ScaleEmitterToSurface(ParticleSystem ps)
    {
        var hit = Physics2D.BoxCast(
            transform.position.Add(0f, -0.05f), // cast slightly below the player
            new Vector2(transform.localScale.x, 0.1f),
            0f,
            Vector2.down,
            2f,
            mvmt.Collisions.GroundLayer
        );

        var emitter = ps.shape;

        if (hit) {
            // first we need to obtain the scale and start of the tile
            // because of how tilemap colliders work, we need to
            // work around them
            Vector3 topLeft, scale;

            if(hit.collider is TilemapCollider2D || hit.collider is CompositeCollider2D) {
                // we've hit a tilemap collider, so we can't just use
                // its transform.position property (it would point to the
                // start of the tilemap, NOT the start of the tile itself)
                // what we need to do is get the tile at the position of the
                // raycast tile, and then call CellToWorld to get the
                // beginning of the tile
                Tilemap tilemap = hit.collider.GetComponent<Tilemap>();
                var tilePos = tilemap.layoutGrid.CellToWorld(tilemap.layoutGrid.WorldToCell(hit.point));
                
                topLeft = tilePos;
                scale = tilemap.cellSize;
            } else {
                // we've hit a regular collider, we can just use its position
                // outright
                topLeft = hit.transform.position;
                scale = hit.transform.localScale;
            }

            // -1 if the tile is on our right and 1 if its on our left
            float side = Mathf.Sign(transform.position.x - topLeft.x);

            // check for a neightbouring object in the direction that we
            // would scale the particle system in - if we can detect one,
            // scaling should not be performed as the particle can simply
            // reside on said neighbouring object
            Vector3 neighbour;

            if (hit.collider is TilemapCollider2D) {
                Tilemap tilemap = hit.collider.GetComponent<Tilemap>();
                var tilePos = tilemap.layoutGrid.CellToWorld(tilemap.layoutGrid.WorldToCell(topLeft.Add(side * scale.x, 0f)));
                neighbour = tilePos;
            }
            else {
                neighbour = hit.transform.position.Add(side * scale.x);
            }

            if (!Physics2D.OverlapPoint(neighbour, mvmt.Collisions.GroundLayer)) {
                // no neighbour - we need to scale the emitter accordingly
                // to "cut" the particle emitter
                Vector3 middle = topLeft.Add(scale.x / 2, 0f);
                float distanceToEdge = middle.x - transform.position.x;

                emitter.scale = new Vector3(
                    Mathf.Clamp01(Mathf.Abs(1f - distanceToEdge)),
                    1f, 1f
                );

                emitter.position = new Vector3(distanceToEdge / 2f, emitter.position.y);
            } else {
                // neighbour detected - we can probably fit the whole particle,
                // so no need for scaling
                emitter.scale = new Vector3(1f, 1f, 1f);
                emitter.position = new Vector3(0f, emitter.position.y);
            }
            
        } else {
            emitter.scale = new Vector3(1f, 1f, 1f);
            emitter.position = new Vector3(0f, emitter.position.y);
        }
    }

    /// <summary>
    /// Moves a particle system's Y coordinate to the surface
    /// the player is standing on.
    /// </summary>
    /// <param name="ps">The target particle system.</param>
    void MoveParticleSystemToGround(ParticleSystem ps)
    {
        var hit = Physics2D.BoxCast(
            transform.position,
            new Vector2(transform.localScale.x, 0.5f),
            0f,
            Vector2.down,
            5f,
            mvmt.Collisions.GroundLayer
        );

        if (hit) {
            ps.transform.position = new Vector3(
                ps.transform.position.x,
                hit.point.y,
                ps.transform.position.z
            );
        }
    }

    /// <summary>
    /// Moves a particle system's X coordinate to the wall the
    /// player is standing against.
    /// </summary>
    /// <param name="ps">The target particle system.</param>
    void MoveParticleSystemToWall(ParticleSystem ps)
    {
        var hit = Physics2D.BoxCast(
            transform.position,
            new Vector2(0.5f, transform.localScale.y),
            0f,
            // TODO: This somehow works but realistically these vectors should be swapped
            mvmt.FacingLeft ? Vector2.left : Vector2.right,
            5f,
            mvmt.Collisions.GroundLayer
        );

        if (hit) {
            ps.transform.position = new Vector3(
                hit.point.x,
                ps.transform.position.y,
                ps.transform.position.z
            );
        }
    }
    
    public void SpawnJumpParticles()
    {
        MoveParticleSystemToGround(JumpParticles);
        ScaleEmitterToSurface(JumpParticles);
        JumpParticles.Play();
    }
    
    public void SpawnLandParticles()
    {
        ScaleEmitterToSurface(LandParticles);
        LandParticles.Play();
    }
    
    public void SpawnWallJumpParticles()
    {
        var pTransform = WallParticles.transform;

        int sign = (mvmt.FacingLeft ? 1 : -1);
        float newX = Mathf.Abs(pTransform.localPosition.x) * sign;

        pTransform.localPosition = new Vector3(newX, pTransform.localPosition.y, pTransform.localPosition.z);
        pTransform.localRotation = mvmt.FacingLeft ? Quaternion.identity : oneEightyDegrees;
        
        MoveParticleSystemToWall(WallParticles);
        WallParticles.Play();
    }
}
