using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Convenience methods related to asynchronous operations.
/// </summary>
internal static class Asynchronous
{
    static IEnumerator RunAfterCoroutine(Action lambda, float after)
    {
        yield return new WaitForSeconds(after);
        lambda();
    }

    /// <summary>
    /// Runs the specified <see cref="Action"/> after the specified number of seconds.
    /// </summary>
    /// <param name="self">The caller of this method.</param>
    /// <param name="after">How long to wait before running the specified <see cref="Action"/>.</param>
    /// <param name="lambda">The target <see cref="Action"/> to run after the specified number of seconds.</param>
    public static void RunAfter(this MonoBehaviour self, float after, Action lambda)
        => self.StartCoroutine(RunAfterCoroutine(lambda, after));
}