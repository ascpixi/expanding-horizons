using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

/// <summary>
/// Provides additional debugging support.
/// </summary>
public static class Debugging
{
#if DEBUG && UNITY_EDITOR
    private class DebugTextDisplay
    {
        public string Content;
        public Vector2 Position;

        public DebugTextDisplay(string content, Vector2 position)
        {
            Content = content;
            Position = position;
        }
    }

    private class JellyDebugComponent : MonoBehaviour
    {
        public readonly List<DebugTextDisplay> PendingTextDisplays = new();

        private void Awake()
        {
            hideFlags |= HideFlags.HideInInspector;
            hideFlags |= HideFlags.HideInHierarchy;
        }

        private void OnGUI()
        {
            foreach (var display in PendingTextDisplays) {
                GUI.Label(
                    new Rect(
                        display.Position,
                        new(9 * display.Content.Length, 18)
                    ),
                    display.Content
                );
            }

            if(Event.current.type == EventType.Repaint) {
                PendingTextDisplays.Clear();
            }
        }
    }
#endif

    /// <summary>
    /// Draws text for one frame. A call to this method is only
    /// performed in debug mode and while in the editor.
    /// </summary>
    /// <remarks>
    /// To render the text, the Unity GUI system is used. This will spawn
    /// no objects except an internal, hidden component, which is used to
    /// catch GUI events.
    /// </remarks>
    /// <param name="self">The caller of this method.</param>
    /// <param name="position">The top-right position of the text.</param>
    /// <param name="text">The content of the text to draw.</param>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void DrawText(GameObject self, Vector2 position, string text)
    {
#if DEBUG && UNITY_EDITOR
        if(!self.TryGetComponent<JellyDebugComponent>(out var dbg)) {
            dbg = self.AddComponent<JellyDebugComponent>();
        }

        dbg.PendingTextDisplays.Add(new(text, position));
#endif
    }

    /// <inheritdoc cref="DrawText(GameObject, Vector2, string)"/>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void DrawText(Component self, Vector2 position, string text)
        => DrawText(self.gameObject, position, text);

    /// <summary>
    /// Draws a 2D box for one frame. A call to this method is only performed
    /// in debug mode and while in the editor.
    /// </summary>
    /// <param name="center">The center of the box.</param>
    /// <param name="size">The size of the box.</param>
    /// <param name="color">The color of the box.</param>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void DrawBox(Vector2 center, Vector2 size, Color color)
    {
#if DEBUG && UNITY_EDITOR
        var halfSize = size / 2;
        float hsx = halfSize.x;
        float hsy = halfSize.y;

        Debug.DrawLine(center + new Vector2(-hsx, -hsy), center + new Vector2(hsx, -hsy), color);
        Debug.DrawLine(center + new Vector2(hsx, -hsy), center + new Vector2(hsx, hsy), color);
        Debug.DrawLine(center + new Vector2(hsx, hsy), center + new Vector2(-hsx, hsy), color);
        Debug.DrawLine(center + new Vector2(-hsx, hsy), center + new Vector2(-hsx, -hsy), color);
#endif
    }

    /// <summary>
    /// Draws a 2D circle for one frame. A call to this method is only performed
    /// in debug mode and while in the editor.
    /// </summary>
    /// <param name="position">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="color">The color of the circle.</param>
    /// <param name="segments">The number of vertices in the circle.</param>
    // Source: https://dev-tut.com/2022/unity-draw-a-circle-part2/
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void DrawCircle(Vector3 position, float radius, Color color, int segments = 32)
    {
#if DEBUG && UNITY_EDITOR
        // If either radius or number of segments are less or equal to 0, skip drawing
        if (radius <= 0.0f || segments <= 0) {
            return;
        }

        // Single segment of the circle covers (360 / number of segments) degrees
        float angleStep = (360.0f / segments);

        // The result is multiplied by Mathf.Deg2Rad constant which transforms degrees to radians
        // which are required by Unity's Mathf class trigonometry methods

        angleStep *= Mathf.Deg2Rad;

        // lineStart and lineEnd variables are declared outside the following for loop
        Vector3 lineStart = Vector3.zero;
        Vector3 lineEnd = Vector3.zero;

        for (var i = 0; i < segments; i++) {
            // Line start is defined as the starting angle of the current segment (i)
            lineStart.x = Mathf.Cos(angleStep * i);
            lineStart.y = Mathf.Sin(angleStep * i);

            // Line end is defined by the angle of the next segment (i+1)
            lineEnd.x = Mathf.Cos(angleStep * (i + 1));
            lineEnd.y = Mathf.Sin(angleStep * (i + 1));

            // Results are multiplied so they match the desired radius
            lineStart *= radius;
            lineEnd *= radius;

            // Results are offset by the desired position/origin 
            lineStart += position;
            lineEnd += position;

            // Points are connected using DrawLine method and using the passed color
            Debug.DrawLine(lineStart, lineEnd, color);
        }
#endif
    }

    /// <summary>
    /// Logs all the declared fields of the given object.
    /// A call to this method is only performed in debug mode and while in the editor.
    /// </summary>
    /// <param name="obj">The object to log the fields of.</param>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogDump(object obj)
    {
#if UNITY_EDITOR && DEBUG
        var sb = new StringBuilder($"({obj.GetType().Name}:");
        var fields = obj.GetType().GetFields(
            BindingFlags.DeclaredOnly | BindingFlags.Instance |
            BindingFlags.NonPublic | BindingFlags.Public
        );

        for (int i = 0; i < fields.Length; i++) {
            var field = fields[i];

            sb.Append(' ');
            sb.Append(field.Name);
            sb.Append(" = ");

            if (field.FieldType == typeof(float)) {
                var f = (float)field.GetValue(obj);
                sb.Append(f.ToString("0.###"));
            }
            else if (field.FieldType == typeof(double)) {
                var d = (double)field.GetValue(obj);
                sb.Append(d.ToString("0.####"));
            }
            else if (field.FieldType == typeof(string)) {
                var s = (string)field.GetValue(obj);
                sb.Append('"');
                sb.Append(s);
                sb.Append('"');
            } else {
                sb.Append(field.GetValue(obj));
            }

            if(i != fields.Length - 1) {
                sb.Append(',');
            }
        }

        sb.Append(')');

        Debug.Log(sb.ToString());
#endif
    }

    /// <summary>
    /// Logs a message to the Unity Console when called from a debug editor build.
    /// </summary>
    /// <param name="message">String or object to be converted to string representation for display.</param>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogInfo(object message)
    {
#if DEBUG && UNITY_EDITOR
        Debug.Log(message);
#endif
    }

    /// <summary>
    /// Logs a warning message to the Unity Console when called from a debug editor build.
    /// </summary>
    /// <param name="message">String or object to be converted to string representation for display.</param>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogWarning(object message)
    {
#if DEBUG && UNITY_EDITOR
        Debug.LogWarning(message);
#endif
    }

    /// <summary>
    /// Logs an error message to the Unity Console when called from a debug editor build.
    /// </summary>
    /// <param name="message">String or object to be converted to string representation for display.</param>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogError(object message)
    {
#if DEBUG && UNITY_EDITOR
        Debug.LogError(message);
#endif
    }
}