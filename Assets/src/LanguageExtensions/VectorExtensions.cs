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
    public static Vector3 Add(this Vector3 vec, float amt)
    {
        return new Vector3(vec.x + amt, vec.y + amt, vec.z);
    }
    
    public static Vector3 WithX(this Vector3 v, float x) => new(x, v.y, v.z);

    public static Vector3 WithY(this Vector3 v, float y) => new(v.x, y, v.z);
    
    public static Vector2 WithX(this Vector2 v, float x) => new(x, v.y);

    public static Vector2 WithY(this Vector2 v, float y) => new(v.x, y);

    public static Vector3 WithZ(this Vector3 v, float z) => new(v.x, v.y, z);
    public static Vector3 WithZ(this Vector2 v, float z) => new(v.x, v.y, z);
    
    #region Method Group: Sizzle Masks
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 XY(this Vector3 v) => new(v.x, v.y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int XY(this Vector3Int v) => new(v.x, v.y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 XYZ(this Vector2 v) => new(v.x, v.y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int XYZ(this Vector2Int v) => new(v.x, v.y);
    #endregion

    #region Method Group: Int <-> Float
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 AsFloat(this Vector3Int v) => new(v.x, v.y, v.z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 AsFloat(this Vector2Int v) => new(v.x, v.y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int Floor(this Vector3 v) => new((int)v.x, (int)v.y, (int)v.z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int Floor(this Vector2 v) => new((int)v.x, (int)v.y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int Round(this Vector3 v)
        => new(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int Round(this Vector2 v)
        => new(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
    #endregion

    #region Method Group: Transform()
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Transform(this Vector3 v, Func<float, float> f) => new(f(v.x), f(v.y), f(v.z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Transform(this Vector2 v, Func<float, float> f) => new(f(v.x), f(v.y));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int Transform(this Vector3Int v, Func<int, int> f) => new(f(v.x), f(v.y), f(v.z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int Transform(this Vector2Int v, Func<int, int> f) => new(f(v.x), f(v.y));
    #endregion

    #region Method Group: Unpack()
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (float, float) Unpack(this Vector2 v) => (v.x, v.y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (float, float, float) Unpack(this Vector3 v) => (v.x, v.y, v.z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (float, float) UnpackTwo(this Vector3 v) => (v.x, v.y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int, int) Unpack(this Vector2Int v) => (v.x, v.y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int, int, int) Unpack(this Vector3Int v) => (v.x, v.y, v.z);
    #endregion
}
