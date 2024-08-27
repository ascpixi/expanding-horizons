using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CollisionController))]
[RequireComponent(typeof(AnimationController))]
public class MovementController2D : MonoBehaviour
{
    [Header("Movement")]
    public float Speed;
    public float JumpHeight;
    [Min(0)] public int ExtraJumpAmount = 0;
    [Range(0, 1)] public float JumpControlMultiplier = 0.65f;
    [Range(0, 1)] public float MovingJumpMultiplier = 0.85f;
    public float MaxJumpDuration = 0.265f;
    
    [Header("Timing")]
    public int AccelerationSpeed = 3;
    public int DecelerationSpeed = 1;
    [Range(0, 1)] public float AccelerationBase = 0.5f;
    public AnimationCurve AccelerationCurve;
    public AnimationCurve DecelerationCurve;

    [Header("Leniency")]
    public float JumpInputBufferingTime = 0.15f;
    public float JumpGroundedLeniency = 0.1f;

    [Header("Horizontal Gravity")]
    public float VelocityCounteractStrength = 3;
    public float Drag = 24;
    public Vector2 WallJumpForce = new(9, 10);
    public float WallJumpLeniency = 0.2f;
    public float WallSlideMaximumVelocity = 2;
    public float SameWallJumpCooldown = 0.2f;
    
    [Header("Audio")]
    public AudioSource Audio;
    public AudioClip[] GroundHitSfx;
    [Range(0, 1)] public float GroundHitSfxVolume = 0.6f;
    public AudioClip JumpSfx;
    public AudioClip[] WalkSfx;
    public float WalkSfxDelay = 0.4f;
    [Range(0, 1)] public float WalkSfxVolume = 0.5f;
    public AudioClip ExtraJumpSfx;

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

            if (ExtraJumpSfx != null) {
                Audio.PlayOneShot(ExtraJumpSfx);
            }
            
            jumpsUsed++;
        }
        else {
            anim.PlayAnimation(PlayerAnimationType.Jump);
            particles.SpawnJumpParticles();
            
            if (JumpSfx != null) {
                Audio.PlayOneShot(JumpSfx);
            }
        }

        jumping = true;
        //rb.velocity = new Vector2(rb.velocity.x, jumpHeight * (Moving ? movingJumpMultiplier : 1f));
		
		rb.velocity = new Vector2(0, JumpHeight * (Moving ? MovingJumpMultiplier : 1f));
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
        Audio.PlayOneShot(WalkSfx.Random(), WalkSfxVolume);
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
            JumpBuffer.Position = JumpInputBufferingTime;
            JumpHoldTimer.Position = MaxJumpDuration;
        }

        if (!Controllable || GlobalGameBehaviour.Frozen) {
            return;
        }

        if (Input.GetButtonUp("Jump") || (Input.GetButton("Jump") && JumpHoldTimer.Ended)) {
            if (jumping) {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * JumpControlMultiplier);
                jumping = false;
            }
        }

        bool currentlyGrounded = coll.Grounded;
        if (currentlyGrounded) {
            CoyoteTime.Position = JumpGroundedLeniency;
            jumpsUsed = 0;
            velocityYTrack = rb.velocity.y; // should be 0
        }

        if (!JumpBuffer.Ended && (GroundedCoyote || jumpsUsed < ExtraJumpAmount)) {
            if (!Physics2D.Raycast(transform.position, Vector2.up, coll.CollisionSizeY, coll.GroundLayer)) {
                Jump();
            }
        }

        // Play walk sfx
        if (WalkSfx != null && Moving && currentlyGrounded && walkSfxLastPlayed >= WalkSfxDelay) {
            walkSfxLastPlayed = 0f;
            Audio.PlayOneShot(WalkSfx.Random(), WalkSfxVolume);
        }
        
        if (CoyoteTime.Ended && wasGrounded) {
            wasGrounded = false;
        }

        if (GroundedCoyote && !wasGrounded) {
            // Player has hit the ground
            wasGrounded = true;

            if (GroundHitSfx != null) {
                Audio.PlayOneShot(GroundHitSfx.Random(), GroundHitSfxVolume);
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
            accelerationFrame = -DecelerationSpeed;
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
            float actualSpeed = Speed * (
                AccelerationBase + (
                    AccelerationCurve.Evaluate(
                        Mathf.Clamp01(accelerationFrame / AccelerationSpeed)
                    )
                ) * (1f - AccelerationBase)
            );
            
            var simulatedPosition = transform.position + new Vector3(horz * Speed * controlAmt, 0);
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
                float deceleration = Speed * DecelerationCurve.Evaluate(Mathf.Clamp01(Mathf.Abs(accelerationFrame) / DecelerationSpeed));
                Vector3 simulatedPosition = transform.position + new Vector3(decelerationTurn * (Speed + 0.05f), 0);
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
