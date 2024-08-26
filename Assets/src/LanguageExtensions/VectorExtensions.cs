using System;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// Provides extension methods related to vector component operations. 
/// </summary>
public static class VectorExtensions
{
    /// <summary>
    /// Adds the specified value to the X and Y components of the <see cref="Vector2"/>.
    /// </summary>
    /// <param name="vec">The target vector.</param>
    /// <param name="amt">The value to add to the vector's X and Y components.</param>
    /// <returns>The modified <see cref="Vector2"/>.</returns>
    public static Vector3 Add(this Vector3 vec, float amt) => new(vec.x + amt, vec.y + amt, vec.z);
    
    public static Vector2 Abs(this Vector2 self) => new(Mathf.Abs(self.x), Mathf.Abs(self.y));
    public static Vector2 Add(this Vector2 v, float x, float y) => new(v.x + x, v.y + y);
    public static Vector3 Add(this Vector3 v, float x, float y) => new(v.x + x, v.y + y, v.z);
    public static Vector3 Multiply(this Vector3 v, float x, float y, float z) => new(v.x * x, v.y * y, v.z * z);
    
    public static Vector2 WithX(this Vector2 v, float x) => new(x, v.y);

    public static Vector2 WithY(this Vector2 v, float y) => new(v.x, y);

    public static Vector3 WithZ(this Vector3 v, float z) => new(v.x, v.y, z);
    public static Vector3 WithZ(this Vector2 v, float z) => new(v.x, v.y, z);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 XYZ(this Vector2 v) => new(v.x, v.y);
}
