using UnityEngine;

/// <summary>
/// Represents a countdown.
/// </summary>
public class Countdown {
    /// <summary>
    /// The position of the countdown.
    /// </summary>
    public float Position { get; set; } = 0f;

    /// <summary>
    /// Whether the countdown is running.
    /// </summary>
    public bool Running { get; set; } = true;

    /// <summary>
    /// Whether the countdown has ended.
    /// </summary>
    public bool Ended => Position <= 0f;

    /// <summary>
    /// Advances the timer. Call this method in <c>Update</c>.
    /// </summary>
    public void Advance()
    {
        if (!Running) return;

        if(Position > 0f) {
            Position -= Time.deltaTime;

            if (Position < 0f)
                Position = 0f;
        }
    }

    public override string ToString()
    {
        return Position + (Running ? "" : " (paused)");
    }
}
