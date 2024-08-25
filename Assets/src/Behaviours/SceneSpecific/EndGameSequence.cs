using System.Collections;
using DG.Tweening;
using UnityEngine;

public class EndGameSequence : MonoBehaviour
{
    public Transform WispTransform;
    public DialogueController Dialogues;
    public DialogueData Text;
    public Vector2 TriggerRadius;
    public float EndAnimationDuration;
    public AudioSource Audio;
    public AudioClip EndSfx;
    public float WhiteScreenFadeOutDuration;
    
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, TriggerRadius);
    }
#endif
    
    IEnumerator Start()
    {
        var bounds = new Bounds(transform.position.WithZ(0), TriggerRadius);
        var player = PlayerController.Main;

        while (!bounds.Contains(player.transform.position.WithZ(0))) {
            yield return null;
        }
        
        var viewport = ViewportBehaviour.Main;
        
        GlobalGameBehaviour.Frozen = true;
        
        yield return Dialogues.DialogueCoroutine(Text);
        
        Audio.PlayOneShot(EndSfx);
        
        DOTween.To(
            () => viewport.OriginScale,
            x => viewport.OriginScale = x,
            viewport.OriginScale.Multiply(10, 5, 1),
            EndAnimationDuration
        );

        player.Movement.enabled = false;
        player.transform.DOMoveY(player.transform.position.y + 6f, EndAnimationDuration);
        player.transform.DORotate(new Vector3(0, 0, 70f), EndAnimationDuration);

        StartCoroutine(ColorCycleCoroutine());
        
        TransitionManager.Main.TransitionImage.DOColor(new(1, 1, 1, 1), EndAnimationDuration);

        yield return new WaitForSeconds(EndAnimationDuration);

        TransitionManager.Main.TransitionImage.DOColor(new(0, 0, 0, 1), WhiteScreenFadeOutDuration);

        yield return new WaitForSeconds(WhiteScreenFadeOutDuration);
        
        GlobalGameBehaviour.LoadNextScene();
    }

    IEnumerator ColorCycleCoroutine()
    {
        float delay = 0.25f;
        var viewport = ViewportBehaviour.Main;

        while (true) {
            viewport.LeftSideBorder.sprite = viewport.AnchorVertSideSprite;
            yield return new WaitForSeconds(delay);
            delay *= 0.9f;
            viewport.LeftSideBorder.sprite = viewport.VertSideSprite;
        
            viewport.UpperSideBorder.sprite = viewport.AnchorHorzSideSprite;
            yield return new WaitForSeconds(delay);
            delay *= 0.9f;
            viewport.UpperSideBorder.sprite = viewport.HorzSideSprite;
        
            viewport.RightSideBorder.sprite = viewport.AnchorVertSideSprite;
            yield return new WaitForSeconds(delay);
            delay *= 0.9f;
            viewport.RightSideBorder.sprite = viewport.VertSideSprite;
        
            viewport.LowerSideBorder.sprite = viewport.AnchorHorzSideSprite;
            yield return new WaitForSeconds(delay);
            delay *= 0.9f;
            viewport.LowerSideBorder.sprite = viewport.HorzSideSprite;
        }
    }
}