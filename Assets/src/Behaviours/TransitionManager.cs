using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TransitionManager : MonoBehaviour
{
    public Image TransitionImage;
    public float Duration = 0.5f;
    
    public static TransitionManager Main { get; private set; }

    static Tween transitionTween;
    
    void Awake()
    {
        if (Main != null) {
            Debug.LogWarning("There's already a transition manager present in this scene! Destroying the calling component.");
            Destroy(this);
            return;
        }

        Main = this;
    }

    /// <summary>
    /// Fades the scene out to black (i.e., fades in the transition image).
    /// </summary>
    public static void FadeOut()
    {
        transitionTween = Main.TransitionImage.DOFade(1f, Main.Duration);
    }

    /// <summary>
    /// Fades the scene in from black (i.e., fades out the transition image).
    /// </summary>
    public static void FadeIn()
    {
        transitionTween = Main.TransitionImage.DOFade(0f, Main.Duration);
    }

    public static YieldInstruction WaitForCompletion()
        => transitionTween.WaitForCompletion();
}
