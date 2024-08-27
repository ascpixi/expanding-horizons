using System;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

public enum PlayerAnimationType
{
    Land,
    Jump,
    ExtraJump,
    Death
}

[DisallowMultipleComponent]
[RequireComponent(typeof(MovementController2D))]
[RequireComponent(typeof(PlayerController))]
public class AnimationController : MonoBehaviour
{
    [Header("Components")]
    public SpriteRenderer Renderer;
    public Animator Animator;
    public Transform VisualTransform;

    [Header("Tweens")]
    public Vector2 LandSquishScale = new(0.45f, 0.025f);
    public float LandSquishTime = 0.25f;
    public Vector2 ExtraJumpSquishScale = new(0.025f, 0.3f);
    public float ExtraJumpSquishTime = 0.2f;
    public Vector2 JumpSquishScale = new(0.45f, 0.025f);
    public float JumpSquishTime = 0.25f;

    [Header("Animator Parameters")]
    public string MovingParamName = "Moving";
    public string GroundedParamName = "Grounded";
    public string JumpingParamName = "Jumping";
    public string WallSlidingParamName = "WallSliding";
    public string AimUpParamName = "AimingUp";
    public string AimDownParamName = "AimingDown";
    public string NoEquipParamName = "NoEquip";
    
    MovementController2D mvmt;
    PlayerController player;
    [CanBeNull] Tweener visualTransformTween = null;
    
    /// <summary>
    /// A value indicating whether the animation controller should update.
    /// </summary>
    public bool Frozen { get; set; } = false;
    
    void Start()
    {
        mvmt = GetComponent<MovementController2D>();
        player = GetComponent<PlayerController>();
    }

    void Update()
    {
        Animator.SetBool(NoEquipParamName, player.NoEquipment);
        
        if (Frozen || GlobalGameBehaviour.Frozen) {
            // I'd introduce a method like SetBools if Unity didn't use an ancient
            // version of C# that requires an array allocation for vararg-like methods :(
            Animator.SetBool(GroundedParamName, true);
            Animator.SetBool(MovingParamName, false);
            Animator.SetBool(JumpingParamName, false);
            Animator.SetBool(AimUpParamName, false);
            Animator.SetBool(AimDownParamName, false);
            return;
        }
        
        Animator.SetBool(AimUpParamName, player.IsAimingUp);
        Animator.SetBool(AimDownParamName, player.IsAimingDown);
        
        Animator.SetBool(MovingParamName, mvmt.Moving);
        Animator.SetBool(JumpingParamName, mvmt.Jumping);
        Animator.SetBool(WallSlidingParamName, mvmt.WallSliding);

        Animator.SetBool(GroundedParamName, mvmt.Collisions.Grounded);

        if (mvmt.WallSliding) {
            if (mvmt.Collisions.TouchingRightWall ||  mvmt.Collisions.TouchingLeftWall) {
                Renderer.flipX = Input.GetAxis("Horizontal") > 0f;
            }
        }
        else {
            Renderer.flipX = mvmt.FacingLeft;
        }
    }

    void OnDestroy()
    {
        // finish the visual transform tween if it's in progress
        // if we don't do this, DOTween will try to access a null transform
        // (because the object has been destroyed) and throw an exception
        visualTransformTween?.Kill();
    }
    
    /// <summary>
    /// Plays a "squish" animation.
    /// </summary>
    /// <param name="scale">How much to squish the player sprite by.</param>
    /// <param name="duration">The duration of the squish animation.</param>
    public void Squish(Vector2 scale, float duration)
    {
        ClearVisualTransformTweener();
        visualTransformTween = VisualTransform.DOPunchScale(
            scale,
            duration,
            vibrato: 1
        );
    }

    /// <summary>
    /// Completes any tween-based animations.
    /// </summary>
    public void ClearVisualTransformTweener()
    {
        if (visualTransformTween != null) {
            visualTransformTween.Complete();
            visualTransformTween = null;
        }
    }
    
    /// <summary>
    /// Triggers a specific animation.
    /// </summary>
    /// <param name="animation">The animation type.</param>
    public void PlayAnimation(PlayerAnimationType animation)
    {
        switch (animation) {
            case PlayerAnimationType.Land: {
                Squish(LandSquishScale, LandSquishTime);
                break;
            }
            case PlayerAnimationType.ExtraJump: {
                Squish(ExtraJumpSquishScale, ExtraJumpSquishTime);
                break;
            }
            case PlayerAnimationType.Jump: {
                Squish(JumpSquishScale, JumpSquishTime);
                break;
            }
            case PlayerAnimationType.Death: {
                Animator.SetBool(WallSlidingParamName, false);
                // Animator.SetTrigger(DeathAnimatorTriggerName);
                break;
            }
            default:
                throw new NotImplementedException();
        }
    }
}