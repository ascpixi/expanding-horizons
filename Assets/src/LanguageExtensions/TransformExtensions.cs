using UnityEngine;

public static class TransformExtensions
{
    public static void MoveX(this Transform self, float x)
        => self.position += new Vector3(x, 0);

    public static void MoveY(this Transform self, float y)
        => self.position += new Vector3(0, y);
}