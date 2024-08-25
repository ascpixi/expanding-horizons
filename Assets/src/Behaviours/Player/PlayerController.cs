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
    public SpriteRenderer SpriteRenderer;
    public AudioSource AudioSource;
    public Animator Animator;

    [Header("Objects")]
    public Transform ProjectileOrigin;
    public Transform ProjectileOriginParent;

    [Header("Prefabs")]
    public GameObject PrimaryBeamPrefab;
    public GameObject SecondaryBeamPrefab;
    
    [Header("Animation")]
    public string HurtParamName = "Hurt";

    public string AimUpParamName = "AimingUp";
    public string AimDownParamName = "AimingDown";
    public string NoEquipParamName = "NoEquip";
    public float HurtAnimDuration = .7f;
    public float WinAnimDuration = 1f;
    public float TeleportDuration = 0.25f;
    
    [Header("Audio")]
    public AudioClip HurtSfx;
    public AudioClip LevelPassSfx;

    [Header("Misc.")]
    public bool MainPlayer = true;
    public float RecallDuration = 0.5f; // the amount of time the player has to hold the recall button
    public float DeathFallYCutoff = -10;
    
    public bool IsRecalling { get; private set; } 
    
    public int BeamsLeft { get; set; }

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

        if (!GlobalGameBehaviour.Frozen) {
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
                Movement.Collisions.Grounded
            ) {
                IsRecalling = true;
                recallHoldTime += Time.deltaTime;
            }
            else {
                IsRecalling = false;
                recallHoldTime = 0;
            }

            if (recallHoldTime >= RecallDuration) {
                Recall(true);
            }

            ProjectileOriginParent.localScale = new Vector3(Movement.FacingLeft ? -1 : 1, 1, 1);

            if (LevelData.Current.AllowBeam) {
                Animator.SetBool(AimUpParamName, aimingUp);
                Animator.SetBool(AimDownParamName, aimingDown);
                Animator.SetBool(NoEquipParamName, false);

                if (BeamsLeft > 0) {
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
                Animator.SetBool(NoEquipParamName, true);
            }
        }
        else {
            Animator.SetBool(NoEquipParamName, LevelData.Current.AllowBeam);
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
        // if (IsDead) return;
        //
        // Animator.SetBool(Movement.groundedParameterName, true);
        // Animator.SetBool(Movement.jumpingParameterName, false);
        // Animator.SetTrigger(HurtParamName);
        // AudioSource.Stop();
        // Movement.Freeze();
        // IsDead = true;
        //
        // GlobalGameBehaviour.Frozen = true;
        //
        // AudioSource.PlayOneShot(HurtSfx);
        // this.RunAfter(HurtAnimDuration, GlobalGameBehaviour.RestartCurrentScene);

        if (IsRespawning)
            return;
        
        StartCoroutine(DamageCoroutine());
    }

    IEnumerator DamageCoroutine()
    {
        IsRespawning = true;
        
        // GlobalGameBehaviour.Frozen = true;
        
        // TransitionManager.FadeOut();
        // yield return TransitionManager.WaitForCompletion();

        // CheckpointBehaviour.ResetCheckpoints();

        Recall(true);
        yield return new WaitForSeconds(TeleportDuration);
        
        // // transform.position = respawnPoint;
        // GlobalGameBehaviour.Frozen = false;
        IsRespawning = false;

        // TransitionManager.FadeIn();
        yield return null;
    }

    void OnAnyCollision2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy")) {
            Damage();
        }
        
        if (other.gameObject.CompareTag("WinCheckpoint")) {
            Movement.Freeze();
            GlobalGameBehaviour.Frozen = true;

            PersistentData.Global.CurrentLevel = LevelData.Current.LevelIndex + 1;
            PersistentData.Global.Save();

            AudioSource.PlayOneShot(LevelPassSfx);

            float actualDuration = WinAnimDuration * 1.25f;

            transform.DOMove(other.transform.position, actualDuration);
            transform.DORotate(new Vector3(0f, 0f, 115f), actualDuration);
            transform.DOScale(.25f, actualDuration * 1.5f);

            this.RunAfter(WinAnimDuration * 1.5f, GlobalGameBehaviour.LoadNextScene);
        }
    }
}
