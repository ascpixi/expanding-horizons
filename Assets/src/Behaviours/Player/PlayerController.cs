using System.Collections;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Handles players.
/// </summary>
[DisallowMultipleComponent, RequireComponent(typeof(MovementController2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    public AudioSource Audio;
    
    [Header("Objects")]
    public Transform ProjectileOrigin;
    public Transform ProjectileOriginParent;

    [Header("Prefabs")]
    public GameObject PrimaryBeamPrefab;
    public GameObject SecondaryBeamPrefab;
    
    public float TeleportDuration = 0.25f;

    [Header("Audio")]
    public AudioClip[] HitSfx;
    [Range(0, 1)] public float HitSfxVolume = 0.5f;
    public AudioClip[] RecallSfx;
    [Range(0, 1)] public float RecallSfxVolume = 0.5f;
    public AudioClip[] MissSfx;
    [Range(0, 1)] public float MissSfxVolume = 0.4f;
    public AudioClip[] RespawnSfx;
    [Range(0, 1)] public float RespawnSfxVolume = 0.5f;
    
    [Header("Misc.")]
    public bool MainPlayer = true;
    public float RecallDuration = 0.5f; // the amount of time the player has to hold the recall button
    public float DeathFallYCutoff = -10;
    
    public bool IsRecalling { get; private set; } 
    
    public int BeamsLeft { get; set; }

    public bool IsAimingUp { get; private set; } 
    public bool IsAimingDown { get; private set; } 
    public bool NoEquipment { get; private set; }
    
    public bool WandDisabled { get; set; }
    
    public float RecallProgress => recallHoldTime / RecallDuration;
    
    float recallHoldTime;
    Vector3 respawnPoint;
    
    /// <summary>
    /// The movement controller of the player.
    /// </summary>
    public MovementController2D Movement { get; private set; }

    /// <summary>
    /// The main player controller.
    /// </summary>
    public static PlayerController Main { get; private set; }

    /// <summary>
    /// Whether the player is dead (or currently playing the death animation).
    /// </summary>
    public bool IsRespawning { get; private set; }

    /// <summary>
    /// Returns <c>transform.position</c>. This property is included to improve code readibility.
    /// </summary>
    public Vector3 Position => transform.position;

    void Awake()
    {
        if (MainPlayer) {
            if (Main != null) {
                Debug.LogWarning("More than one player was marked with being the main player!");
            }
            else {
                Main = this;
            }
        }
    }

    void Start()
    {
        Movement = GetComponent<MovementController2D>();
        BeamsLeft = LevelData.Current.BeamCount;
        respawnPoint = transform.position;
    }

    void Update()
    {
        if(transform.position.y <= DeathFallYCutoff) {
            Damage();
            return;
        }

        if (GlobalGameBehaviour.Frozen) {
            NoEquipment = LevelData.Current.AllowBeam;
            return;
        }
        
        bool aimingDown = false, aimingUp = false;
    
        if (Input.GetButton("Aim Up")) {
            aimingUp = true;
        }
        else if (Input.GetButton("Aim Down") && !Movement.GroundedCoyote) {
            aimingDown = true;
        }

        if (
            Input.GetButton("Recall") &&
            BeamsLeft != LevelData.Current.BeamCount &&
            !Movement.Moving &&
            !Movement.Airborne &&
            Movement.Collisions.Grounded &&
            !WandDisabled
        ) {
            IsRecalling = true;
            recallHoldTime += Time.deltaTime;
        }
        else {
            IsRecalling = false;
            recallHoldTime = 0;
        }

        if (recallHoldTime >= RecallDuration) {
            Audio.PlayOneShot(RecallSfx.Random(), RecallSfxVolume);
            Recall(true);
        }

        ProjectileOriginParent.localScale = new Vector3(Movement.FacingLeft ? -1 : 1, 1, 1);

        if (LevelData.Current.AllowBeam) {
            IsAimingDown = aimingDown;
            IsAimingUp = aimingUp;
            NoEquipment = false;

            if (BeamsLeft > 0 && !WandDisabled) {
                bool primary = Input.GetButtonDown("Primary Beam");
                bool secondary = Input.GetButtonDown("Secondary Beam");

                bool castingAny = primary || (secondary && LevelData.Current.AllowSecondary);
                
                if (castingAny && !Interactable.AnyInRange) {
                    FireBeam(primary, aimingUp, aimingDown);
                    BeamsLeft--;
                }
            }
        }
        else {
            NoEquipment = true;
        }
    }

    public void Recall(bool teleport)
    {
        ViewportBehaviour.Main.ChangeCurrentSide(ViewportSide.None);
        BeamsLeft = LevelData.Current.BeamCount;
        
        foreach (var beam in FindObjectsOfType<BeamBehaviour>()) {
            Destroy(beam.gameObject);
        }

        if (teleport) {
            Movement.Freeze();

            var position = CheckpointBehaviour.Current != null
                ? CheckpointBehaviour.Current.transform.position
                : respawnPoint;
            
            transform.DOMove(position, TeleportDuration).OnComplete(() =>
            {
                Movement.Unfreeze();
            });
        }
    }
    
    void FireBeam(bool isPrimary, bool aimingUp, bool aimingDown)
    {
        var obj = Instantiate(
            isPrimary ? PrimaryBeamPrefab : SecondaryBeamPrefab,
            ProjectileOrigin.position,
            Quaternion.identity
        );
        
        var beam = obj.GetComponent<BeamBehaviour>();
        beam.Source = this;
        beam.Velocity =
            aimingUp ? new Vector2(0, 1)
            : aimingDown ? new Vector2(0, -1)
            : Movement.FacingLeft ? new Vector2(-1, 0)
            : new Vector2(1, 0);

        obj.transform.rotation =
            aimingUp ? Quaternion.Euler(0, 0, 90)
            : aimingDown ? Quaternion.Euler(0, 0, -90)
            : Movement.FacingLeft ? Quaternion.Euler(0, 180, 0)
            : Quaternion.identity;
    }
    
    void OnTriggerEnter2D(Collider2D collision) => OnAnyCollision2D(collision);

    void OnCollisionEnter2D(Collision2D collision) => OnAnyCollision2D(collision.otherCollider);
    
    public void Damage()
    {
        if (IsRespawning)
            return;
        
        StartCoroutine(DamageCoroutine());
    }

    IEnumerator DamageCoroutine()
    {
        IsRespawning = true;
        
        Audio.PlayOneShot(RespawnSfx.Random(), RespawnSfxVolume);
        
        Recall(true);
        yield return new WaitForSeconds(TeleportDuration);
        
        IsRespawning = false;
        
        yield return null;
    }

    void OnAnyCollision2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy")) {
            Damage();
        }
    }
}
