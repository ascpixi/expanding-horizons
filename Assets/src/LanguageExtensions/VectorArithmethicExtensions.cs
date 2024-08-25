using UnityEngine;

/// <summary>
/// Provides extension methods related to vector arithmethic.
/// </summary>
public static class VectorArithmethicExtensions
{
    public static Vector2 Abs(this Vector2 self) => new(Mathf.Abs(self.x), Mathf.Abs(self.y));
    
    public static float DistanceTo(this Vector3 v, Vector3 other) => Vector3.Distance(v, other);
    public static float DistanceTo2D(this Vector3 v, Vector2 other) => Vector2.Distance(v, other);
    
    public static Vector2 AddEach(this Vector2 v, float x) => new(v.x + x, v.y + x);
    public static Vector2 Add(this Vector2 v, float x, float y) => new(v.x + x, v.y + y);
    public static Vector3 AddEach(this Vector3 v, float x) => new(v.x + x, v.y + x, v.z + x);
    public static Vector3 Add(this Vector3 v, float x, float y) => new(v.x + x, v.y + y, v.z);
    public static Vector3 Add(this Vector3 v, float x, float y, float z) => new(v.x + x, v.y + y, v.z + z);
    public static Vector2 SubEach(this Vector2 v, float x) => new(v.x - x, v.y - x);
    public static Vector2 Sub(this Vector2 v, float x, float y) => new(v.x - x, v.y - y);
    public static Vector3 SubEach(this Vector3 v, float x) => new(v.x - x, v.y - x, v.z - x);
    public static Vector3 Sub(this Vector3 v, float x, float y) => new(v.x - x, v.y - y, v.z);
    public static Vector3 Sub(this Vector3 v, float x, float y, float z) => new(v.x - x, v.y - y, v.z - z);
    public static Vector2 Multiply(this Vector2 v, float x, float y) => new(v.x * x, v.y * y);
    public static Vector3 Multiply(this Vector3 v, float x, float y) => new(v.x * x, v.y * y, v.z);
    public static Vector3 Multiply(this Vector3 v, float x, float y, float z) => new(v.x * x, v.y * y, v.z * z);
    public static Vector2 Divide(this Vector2 v, float x, float y) => new(v.x / x, v.y / y);
    public static Vector3 Divide(this Vector3 v, float x, float y) => new(v.x / x, v.y / y, v.z);
    public static Vector3 Divide(this Vector3 v, float x, float y, float z) => new(v.x / x, v.y / y, v.z / z);
    public static Vector3 Divide(this Vector3 v, Vector3 other) => new(v.x / other.x, v.y / other.y, v.z / other.z);
}