using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Represents an area of the world that the player can interact with.
/// </summary>
public class Interactable : MonoBehaviour
{
    public Vector2 RegionSize;
    public Vector2 UIElementPosition;
    public UnityEvent OnInteract;
    public bool OnlyOnce = true;
    
    public Image UIElement { get; set; }
    public Tween UITween { get; set; }
    public bool UIShown { get; set; }
    
    /// <summary>
    /// Whether this interactable has checked if the player is in its range.
    /// </summary>
    public bool Validated { get; private set; }
    
    /// <summary>
    /// Whether the player is in range of the interactable (in its region) and can interact with it.
    /// </summary>
    public bool PlayerInRange { get; set; }

    /// <summary>
    /// Whether the player is in range of any interactable. This property gets reset
    /// on LateUpdate, meaning reads of it from within LateUpdate are unreliable.
    /// </summary>
    public static bool AnyInRange {
        get {
            foreach (var interactable in All) {
                if (!interactable.Validated) {
                    interactable.CheckForPlayer();
                }

                if (interactable.PlayerInRange)
                    return true;
            }

            return false;
        }
    }
    
    /// <summary>
    /// A list of all of the <see cref="Interactable"/> instances in the scene.
    /// </summary>
    public static readonly List<Interactable> All = new();

    void CheckForPlayer()
    {
        var player = PlayerController.Main;
        PlayerInRange = new Bounds(transform.position.WithZ(0), RegionSize).Contains(player.Position.WithZ(0));
        Validated = true;
    }
    
    void Update()
    {
        if (!Validated) {
            CheckForPlayer();
        }
        
        if (Input.GetButtonDown("Primary") && PlayerInRange) {
            OnInteract.Invoke();

            if (OnlyOnce) {
                Destroy(gameObject);
            }
        }
    }

    void LateUpdate()
    {
        foreach (var interactable in All) {
            interactable.Validated = false;
        }
    }

    void Awake() => All.Add(this);

    void OnDestroy()
    {
        All.Remove(this);

        if (UIElement != null) {
            InteractableUIRenderer.DestroyElement(this);
        }
    }
    
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, RegionSize);
        Gizmos.DrawWireSphere(transform.position + UIElementPosition.WithZ(0), 0.5f);
    }
#endif
}
