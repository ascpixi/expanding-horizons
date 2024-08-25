using UnityEngine;

// TODO: Older portions of this class have public fields in camelCase.
// This will be refactored once the game jam ends and we have more time.
// All new public fields should be named in PascalCase

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CollisionController))]
[RequireComponent(typeof(AnimationController))]
public class MovementController2D : MonoBehaviour
{
    [Header("Movement")]
    public float speed;
    public float jumpHeight;
    [Min(0)] public int extraJumpAmount = 0;
    [Range(0, 1)] public float jumpControlMultiplier = 0.8f;
    [Range(0, 1)] public float movingJumpMultiplier = 0.5f;
    public float MaxJumpDuration = 1f;
    
    [Header("Timing")]
    public int accelerationSpeed = 3;
    public int decelerationSpeed = 1;
    [Range(0, 1)] public float accelerationBase = 0.5f;
    public AnimationCurve accelerationCurve;
    public AnimationCurve decelerationCurve;

    [Header("Leniency")]
    public float jumpInputBufferingTime;
    public float jumpGroundedLeniency;

    [Header("Horizontal Gravity")]
    public float VelocityCounteractStrength;
    public float Drag;
    public Vector2 WallJumpForce;
    public float WallJumpLeniency;
    public float WallSlideMaximumVelocity;
    public float SameWallJumpCooldown = 0.25f;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] groundHitSfx;
    [Range(0, 1)] public float groundHitSfxVolume = 0.6f;
    public AudioClip jumpSfx;
    public AudioClip[] walkSfx;
    public float walkSfxDelay = 0.4f;
    [Range(0, 1)] public float walkSfxVolume = 0.5f;
    public AudioClip extraJumpSfx;

    Rigidbody2D rb;
    CollisionController coll;
    AnimationController anim;
    PlayerParticleController particles;
    
    public readonly Countdown JumpBuffer = new();
    public readonly Countdown CoyoteTime = new();
    public readonly Countdown WallTouchTime = new();
    public readonly Countdown WallJumpCooldown = new();
    public readonly Countdown JumpHoldTimer = new();
    
    bool wasGrounded = true;
    float lastDirection = 0f;
    float decelerationTurn = 0f;
    int jumpsUsed = 0;
    bool jumping = false;
    float velocityYTrack = 0f;
    float walkSfxLastPlayed;
    float lastHorizontalInput = 0f;
    int accelerationFrame = 0;
    float velocityX;
    int wallSide = 0;
    int lastWallJumpSide = 0;
    
    public CollisionController Collisions => coll;
    
    /// <summary>
    /// Whether the coyote time buffer has not yet ended. This does not perform any collision checks.
    /// </summary>
    public bool GroundedCoyote => !CoyoteTime.Ended;
    

    /// <summary>
    /// Gets the last observed Y velocity. This is different from <see cref="Rigidbody2D.velocity"/>, as it
    /// is synchronized with the rendering loop, and as such, can be used in methods like Update.
    /// </summary>
    public float FallingVelocity => velocityYTrack;

    /// <summary>
    /// Whether the player is currently airborne (velocity not equal to zero).
    /// </summary>
    public bool Airborne => FallingVelocity != 0f;
    
    public bool Falling => FallingVelocity < 0f;
    
    public bool Controllable { get; set; } = true;
    
    public bool Jumping => jumping;
    
    public bool WallSliding { get; private set; }
    
    public Rigidbody2D Rigidbody => rb;

    public bool FacingLeft { get; private set; } = false;
    public bool Moving => Controllable && Input.GetAxisRaw("Horizontal") != 0f;

    private void Start()
    {
        particles = GetComponent<PlayerParticleController>();
        coll = GetComponent<CollisionController>();
        anim = GetComponent<AnimationController>();
        
        rb = GetComponent<Rigidbody2D>();
        rb.constraints |= RigidbodyConstraints2D.FreezePositionX;
        rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
    }

    /// <summary>
    /// Freezes this movement controller.
    /// </summary>
    public void Freeze()
    {
        Controllable = false;
        rb.constraints |= RigidbodyConstraints2D.FreezePositionY;
    }

    /// <summary>
    /// Un-freezes this movement controller. This will enable the player to control the object.
    /// </summary>
    public void Unfreeze()
    {
        Controllable = true;
        rb.constraints ^= RigidbodyConstraints2D.FreezePositionY;
    }

    /// <summary>
    /// Forces an movement update.
    /// </summary>
    public void ForceUpdate()
    {
        velocityYTrack = rb.velocity.y;
    }

    /// <summary>
    /// Makes the player jump.
    /// </summary>
    void Jump()
    {
        if (CoyoteTime.Ended) {
            anim.PlayAnimation(PlayerAnimationType.ExtraJump);

            if (extraJumpSfx != null) {
                audioSource.PlayOneShot(extraJumpSfx);
            }
            
            jumpsUsed++;
        }
        else {
            anim.PlayAnimation(PlayerAnimationType.Jump);
            particles.SpawnJumpParticles();
            
            if (jumpSfx != null) {
                audioSource.PlayOneShot(jumpSfx);
            }
        }

        jumping = true;
        //rb.velocity = new Vector2(rb.velocity.x, jumpHeight * (Moving ? movingJumpMultiplier : 1f));
		
		rb.velocity = new Vector2(0, jumpHeight * (Moving ? movingJumpMultiplier : 1f));
        rb.angularVelocity = 0f;
        JumpBuffer.Position = 0f;
    }

    void WallJump()
    {
        lastWallJumpSide = wallSide;
        
        if (coll.TouchingRightWall || coll.TouchingLeftWall) {
            particles.SpawnWallJumpParticles();
        }

        // TODO: This probably should be its own SFX
        audioSource.PlayOneShot(walkSfx.Random(), walkSfxVolume);
        //
        // var wallMat = GetWallTileMaterial();
        // if (wallMat != null) {
        //     audioSource.PlayOneShot(wallMat.stepSounds.Random(), walkSfxVolume);
        // }

        velocityX = WallJumpForce.x * -wallSide;
        rb.velocity = new Vector2(0f, WallJumpForce.y);
    }

    void Update()
    {
        walkSfxLastPlayed += Time.deltaTime;

        JumpBuffer.Advance();
        CoyoteTime.Advance();
        WallTouchTime.Advance();
        WallJumpCooldown.Advance();
        JumpHoldTimer.Advance();

        if (jumping && rb.velocity.y <= 0f) {
            jumping = false;
        }

        if (rb.velocity.y < velocityYTrack) {
            velocityYTrack = rb.velocity.y;
        }
		
		if (jumping) {
			rb.velocity = new Vector2(0, rb.velocity.y);
		}

        if (Input.GetButtonDown("Jump")) {
            JumpBuffer.Position = jumpInputBufferingTime;
            JumpHoldTimer.Position = MaxJumpDuration;
        }

        if (!Controllable) {
            return;
        }

        if (Input.GetButtonUp("Jump") || (Input.GetButton("Jump") && JumpHoldTimer.Ended)) {
            if (jumping) {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpControlMultiplier);
                jumping = false;
            }
        }

        bool currentlyGrounded = coll.Grounded;
        if (currentlyGrounded) {
            CoyoteTime.Position = jumpGroundedLeniency;
            jumpsUsed = 0;
            velocityYTrack = rb.velocity.y; // should be 0
        }

        if (!JumpBuffer.Ended && (GroundedCoyote || jumpsUsed < extraJumpAmount)) {
            if (!Physics2D.Raycast(transform.position, Vector2.up, coll.CollisionSizeY, coll.GroundLayer)) {
                Jump();
            }
        }

        // Play walk sfx
        if (walkSfx != null && Moving && currentlyGrounded && walkSfxLastPlayed >= walkSfxDelay) {
            walkSfxLastPlayed = 0f;
            audioSource.PlayOneShot(walkSfx.Random(), walkSfxVolume);
        }
        
        if (CoyoteTime.Ended && wasGrounded) {
            wasGrounded = false;
        }

        if (GroundedCoyote && !wasGrounded) {
            // Player has hit the ground
            wasGrounded = true;

            if (groundHitSfx != null) {
                audioSource.PlayOneShot(groundHitSfx.Random(), groundHitSfxVolume);
            }

            if (velocityYTrack < -10f) {
                particles.SpawnLandParticles();
            }
            
            velocityYTrack = 0f;// the player can't have any velocity if they just landed
            anim.PlayAnimation(PlayerAnimationType.Land);
        }
        
        if ((coll.TouchingLeftWall || coll.TouchingRightWall) && !coll.Grounded) {
            WallTouchTime.Position = WallJumpLeniency;

            wallSide = 0;

            if (coll.TouchingLeftWall) {
                wallSide = 1;
            }
            else if (coll.TouchingRightWall) {
                wallSide = -1;
            }
        }
        
        float horz = Input.GetAxisRaw("Horizontal");

        if (!WallTouchTime.Ended && CoyoteTime.Ended ) {
            if (horz == wallSide || horz == 0) {
                bool onCooldown = wallSide == lastWallJumpSide && !WallJumpCooldown.Ended;
                
                if (Input.GetButtonDown("Jump") && !onCooldown) {
                    // wall jumping
                    lastWallJumpSide = wallSide;
                    WallJumpCooldown.Position = SameWallJumpCooldown;
                    
                    WallJump();
                    WallSliding = false;
                    WallTouchTime.Position = 0f;
                }
                else {
                    if (horz == wallSide) {
                        // wall sliding
                        rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -WallSlideMaximumVelocity));
                        WallSliding = true;
                    }
                    else {
                        WallSliding = false;
                    }
                }
            }
        }
        else {
            WallSliding = false;
        }
    }

    void FixedUpdate()
    {
        if (GlobalGameBehaviour.Frozen) return;

        UpdateMovements();
    }

    void UpdateMovements()
    {
        float horz = Input.GetAxisRaw("Horizontal");
        float dir = Mathf.Sign(horz);

        if (lastHorizontalInput != 0f && horz == 0f) {
            accelerationFrame = -decelerationSpeed;
        }

        float controlAmt = 1 - (Mathf.Min(VelocityCounteractStrength, Mathf.Abs(velocityX)) / VelocityCounteractStrength);
        
        if (Controllable && horz != 0f) {
            // The player is controllable and is currently holding either one of the directional buttons
            if (lastDirection == 0f || lastDirection != dir) {
                // Player changed directions
                accelerationFrame = 0;
                lastDirection = dir;
                decelerationTurn = dir;
            }

            // Calculate the speed with the acceleration curve applied
            float actualSpeed = speed * (
                accelerationBase + (
                    accelerationCurve.Evaluate(
                        Mathf.Clamp01(accelerationFrame / accelerationSpeed)
                    )
                ) * (1f - accelerationBase)
            );
            
            var simulatedPosition = transform.position + new Vector3(horz * speed * controlAmt, 0);
            var desiredPosition = transform.position + new Vector3(horz * actualSpeed * controlAmt, 0);

            if (coll.SimulateMovement(simulatedPosition)) {
                transform.position = desiredPosition;
            }

            FacingLeft = horz < 0f;
        }
        else {
            lastDirection = 0f;

            if (accelerationFrame < 0) {
                // Debug.Log(decelerationTurn);
                float deceleration = speed * decelerationCurve.Evaluate(Mathf.Clamp01(Mathf.Abs(accelerationFrame) / decelerationSpeed));
                Vector3 simulatedPosition = transform.position + new Vector3(decelerationTurn * (speed + 0.05f), 0);
                Vector3 desiredPosition = transform.position + new Vector3(decelerationTurn * deceleration, 0);
                if (coll.SimulateMovement(simulatedPosition)) {
                    transform.position = desiredPosition;
                }
            }
        }

        // Player movement check done, now do physics
        // Update position according to velocity
        var posPlusVelocity = transform.position + new Vector3(velocityX * Time.deltaTime, 0);
        if (coll.SimulateMovement(posPlusVelocity)) {
            transform.position = posPlusVelocity;
        }
        else {
            // There is an obstacle in the way!
            velocityX = 0;
        }

        lastHorizontalInput = horz;
        accelerationFrame++;

        // Calculate velocity according to drag (air resistance)
        velocityX = Mathf.Max(Mathf.Abs(velocityX) - (Drag * Time.deltaTime), 0) * Mathf.Sign(velocityX);
    }
}
