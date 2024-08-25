using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;


public class CheckpointBehaviour : MonoBehaviour
{
    public Vector2 HitboxSize = new(1.25f, 1.25f);
    public GameObject ReachedEffectPrefab;
    public SpriteRenderer Renderer;
    public Sprite ReachedSprite;
    public Sprite DefaultSprite;
    public AudioSource Audio;
    public AudioClip ReachedSfx;
    public SpriteRenderer XrayRenderer;
    
    public bool HasBeenReached { get; set; }
    [CanBeNull] public static CheckpointBehaviour Current { get; set; }
    public static List<CheckpointBehaviour> Reached { get; private set; } = new();

    void Update()
    {
        if (HasBeenReached)
            return;

        var player = PlayerController.Main;
        var bounds = new Bounds(transform.position, HitboxSize);

        if (bounds.Contains(player.Position)) {
            HasBeenReached = true;
            
            Instantiate(ReachedEffectPrefab, transform.position, transform.rotation, transform);
            
            Renderer.sprite = ReachedSprite;
            XrayRenderer.sprite = ReachedSprite;
            Current = this;
            
            Reached.Add(this);
            
            Audio.PlayOneShot(ReachedSfx);
            
            player.Recall(true);
        }
    }

    public static void ResetCheckpoints()
    {
        foreach (var checkpoint in Reached) {
            checkpoint.HasBeenReached = false;
            checkpoint.Renderer.sprite = checkpoint.DefaultSprite;
            checkpoint.XrayRenderer.sprite = checkpoint.DefaultSprite;
        }
        
        Reached.Clear();
        Current = null;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, HitboxSize);
    }
#endif
}
