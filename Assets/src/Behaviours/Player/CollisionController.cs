using UnityEngine;

public class CollisionController : MonoBehaviour
{
    public float CollisionSizeX = 0.6f;
    public float CollisionSizeY = 0.8f;
    public LayerMask GroundLayer = 1 << 6;
    public float WallCollisionLeniency = 0.005f;
    [Min(0)] public int ExtraRaycastAmount = 4;

    [Header("Wall Collisions")]
    public float WallDetectorRadius = 0.15f;
    public Transform RightWallDetector;
    public Transform LeftWallDetector;
    
    public bool Grounded { get; private set; }
    public bool TouchingRightWall { get; private set; }
    public bool TouchingLeftWall { get; private set; }
    public bool TouchingAnyWall => TouchingRightWall || TouchingLeftWall;
    
    void Update()
    {
        Grounded = CheckIfGrounded();
        TouchingLeftWall = Physics2D.OverlapCircle(LeftWallDetector.position, WallDetectorRadius, GroundLayer);
        TouchingRightWall = Physics2D.OverlapCircle(RightWallDetector.position, WallDetectorRadius, GroundLayer);
    }

    bool CheckIfGrounded()
    {
        return Physics2D.Raycast(transform.position, Vector2.down, CollisionSizeY, GroundLayer) ||
               Physics2D.Raycast(new Vector2(transform.position.x + (CollisionSizeY * 0.5f), transform.position.y), Vector2.down, CollisionSizeY, GroundLayer) ||
               Physics2D.Raycast(new Vector2(transform.position.x - (CollisionSizeY * 0.5f), transform.position.y), Vector2.down, CollisionSizeY, GroundLayer);
    }
    
    /// <summary>
    /// Checks if a movement is realistically possible.
    /// </summary>
    /// <param name="newPosition">The new position to simulate the object in.</param>
    /// <returns><see langword="true"/> if the movement is possible, <see langword="false"/> otherwise.</returns>
    public bool SimulateMovement(Vector3 newPosition)
    {
        // Cast a ray from the start of the player's collider in the direction the new position is
        // relative to the player's position.
        var start = transform.position;
        var direction = (newPosition - start).normalized;

        // Main raycast
        if (!Physics2D.Raycast(start, direction, CollisionSizeX, GroundLayer)) {
#if DEBUG
            Debug.DrawRay(start, direction * CollisionSizeX, Color.green);
#endif

            // Extra raycasts
            for (var i = 1; i <= ExtraRaycastAmount / 2; i++) {
                var originPositive = new Vector2(start.x, start.y + (CollisionSizeY / 2 / i) - WallCollisionLeniency);
                var originNegative = new Vector2(start.x, start.y - (CollisionSizeY / 2 / i) + WallCollisionLeniency);

                if (
                    Physics2D.Raycast(originPositive, direction, CollisionSizeX, GroundLayer) ||
                    Physics2D.Raycast(originNegative, direction, CollisionSizeX, GroundLayer)
                ) {
#if DEBUG
                    Debug.DrawRay(originPositive, direction * CollisionSizeX, Color.red, Time.fixedDeltaTime);
                    Debug.DrawRay(originNegative, direction * CollisionSizeX, Color.red, Time.fixedDeltaTime);
#endif
                    return false;
                }
#if DEBUG
                Debug.DrawRay(originPositive, direction * CollisionSizeX, Color.green, Time.fixedDeltaTime);
                Debug.DrawRay(originNegative, direction * CollisionSizeX, Color.green, Time.fixedDeltaTime);
#endif
            }
        }
        else {
#if DEBUG
            Debug.DrawRay(start, direction * CollisionSizeX, Color.red, Time.fixedDeltaTime);
#endif
            return false;
        }

        if (!ViewportBehaviour.Main.IsInViewport(newPosition))
            return false;
        
        return true;
    }
    
#if UNITY_EDITOR
    // Draw collision boxes in the editor while the object is selected
    private void OnDrawGizmosSelected()
    {
        // Draw detectors
        if (RightWallDetector != null) {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(RightWallDetector.position, WallDetectorRadius);
        }

        if (LeftWallDetector != null) {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(LeftWallDetector.position, WallDetectorRadius);
        }

        // Gizmos.color = Color.green;
        // Gizmos.DrawWireCube(transform.position, CollisionSize);
    }
#endif
}