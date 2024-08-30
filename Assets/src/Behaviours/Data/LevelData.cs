using UnityEngine;

/// <summary>
/// Represents information about a level.
/// </summary>
public class LevelData : MonoBehaviour
{
    [Header("General")]
    public bool FadeIn = true;

    [Header("Player Capabilities")]
    public bool AllowBeam;
    public bool AllowSecondary;
    public int BeamCount;
    
    /// <summary>
    /// The current level data. <see langword="null"/> if the player currently isn't playing a level.
    /// </summary>
    public static LevelData Current { get; private set; }

    /// <summary>
    /// Whether the player is playing a level.
    /// </summary>
    public static bool InLevel => Current != null;

    void Awake()
    {
        Current = this;
    }

    void OnDestroy()
    {
        Current = null;
    }
}
