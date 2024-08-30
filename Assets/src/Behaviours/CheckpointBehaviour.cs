using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class CheckpointBehaviour : MonoBehaviour
{
    public Vector2 HitboxSize = new(1.25f, 1.25f);
    public GameObject ReachedEffectPrefab;
    public SpriteRenderer Renderer;
    public Sprite ReachedSprite;
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

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, HitboxSize);
    }
#endif
}
