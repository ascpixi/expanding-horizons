
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TutorialSequence : MonoBehaviour
{
    public AudioClip ObtainSfx;
    public AudioSource Audio;
    public DialogueController Dialogues;
    public Transform WispTransform;
    public SpriteRenderer WispRenderer;
    public int ViewportHighlightsAnimCount = 2;
    public float ViewportExpandDuration;
    public float WispFadeDuration;
    public float WispDisappearDuration = 2;
    
    public string[] IntroductionDialogue;
    public string[] FirstDialogue;
    public string[] SecondDialogue;
    public string[] ThirdDialogue;
    public string[] FinalDialogue;
    
    public Vector2 FirstPosition;
    public Vector2 FirstTriggerSize;
    public float FirstTiming;
    // public Vector2 SecondPosition;
    // public Vector2 SecondTriggerSize;
    // public float SecondTiming;
    
    
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(FirstPosition, FirstTriggerSize);
        // Gizmos.DrawWireCube(SecondPosition, SecondTriggerSize);
    }
#endif
    
    public void Begin() => StartCoroutine(SequenceCoroutine());
    
    IEnumerator SequenceCoroutine()
    {
        LevelData.Current.AllowBeam = true;
        LevelData.Current.BeamCount = 2;
        PlayerController.Main.BeamsLeft = 2;

        var viewport = ViewportBehaviour.Main;
        
        GlobalGameBehaviour.Frozen = true;

        Audio.PlayOneShot(ObtainSfx);
        
        DOTween.To(
            () => viewport.OriginScale,
            x => viewport.OriginScale = x,
            viewport.OriginScale * 1.5f,
            ViewportExpandDuration
        );

        float div = (4 * ViewportHighlightsAnimCount);
        for (int i = 0; i < ViewportHighlightsAnimCount; i++) {
            viewport.LeftSideBorder.sprite = viewport.AnchorVertSideSprite;
            yield return new WaitForSeconds(ViewportExpandDuration / div);
            viewport.LeftSideBorder.sprite = viewport.VertSideSprite;
        
            viewport.UpperSideBorder.sprite = viewport.AnchorHorzSideSprite;
            yield return new WaitForSeconds(ViewportExpandDuration / div);
            viewport.UpperSideBorder.sprite = viewport.HorzSideSprite;
        
            viewport.RightSideBorder.sprite = viewport.AnchorVertSideSprite;
            yield return new WaitForSeconds(ViewportExpandDuration / div);
            viewport.RightSideBorder.sprite = viewport.VertSideSprite;
        
            viewport.LowerSideBorder.sprite = viewport.AnchorHorzSideSprite;
            yield return new WaitForSeconds(ViewportExpandDuration / div);
            viewport.LowerSideBorder.sprite = viewport.HorzSideSprite;
        }

        
        // The player has picked up the wand! Fade in the wisp...
        WispRenderer.DOFade(1, WispFadeDuration);

        yield return new WaitForSeconds(WispFadeDuration);
        
        yield return Dialogues.DialogueCoroutine(IntroductionDialogue);

        GlobalGameBehaviour.Frozen = false;
        
        WispTransform.DOMove(FirstPosition, FirstTiming);
        yield return new WaitForSeconds(FirstTiming);

        var firstRegion = new Bounds(FirstPosition, FirstTriggerSize);
        var player = PlayerController.Main;
        
        while (!firstRegion.Contains(player.Position.WithZ(0))) {
            yield return null;
        }
        
        yield return Dialogues.DialogueCoroutine(FirstDialogue);

        // wait until the player shoots the left side
        while (viewport.CurrentSide != ViewportSide.Left) {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        
        // now we need the player to recall
        yield return Dialogues.DialogueCoroutine(SecondDialogue);

        while (player.BeamsLeft != 2) {
            yield return null;
        }

        yield return new WaitForSeconds(0.75f);
        
        // great, player now knows how to recall :)
        // now teach em how to shrink
        yield return Dialogues.DialogueCoroutine(ThirdDialogue);

        while (player.BeamsLeft != 0) {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        
        // aight we're done at this point
        yield return Dialogues.DialogueCoroutine(FinalDialogue);
        
        // the player can figure what to do
        WispRenderer.DOFade(0, WispDisappearDuration);
    }
}
