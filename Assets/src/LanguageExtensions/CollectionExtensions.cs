using System.Collections.Generic;

public static class CollectionExtensions
{
    public static T Random<T>(this IReadOnlyList<T> list)
        => list[UnityEngine.Random.Range(0, list.Count)];
}