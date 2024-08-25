using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class IntroManager : MonoBehaviour
{
    public float EndSequenceAfter;
    public Animator IntroAnimator;
    public Behaviour[] BehavioursToEnable;
    public float CameraZoom;
    public PixelPerfectCamera TargetCamera;
    
    float baseCameraSize;
    Camera baseCamera;
    bool introPlaying;
    
    IEnumerator Start()
    {
        introPlaying = true;
        
        baseCameraSize = TargetCamera.orthographicSize;
        baseCamera = TargetCamera.GetComponent<Camera>();
        
        yield return new WaitForSeconds(EndSequenceAfter);
        Debug.Log("The intro sequence has ended.");
        
        introPlaying = false;
        
        IntroAnimator.keepAnimatorStateOnDisable = true;
        IntroAnimator.enabled = false;

        // When we disable the intro animator, some state is lost (mainly some components
        // don't become enabled)
        foreach (var behaviour in BehavioursToEnable) {
            behaviour.enabled = true;
        }
    }

    void Update()
    {
        if (!introPlaying) {
            enabled = false;
            return;
        }
        
        baseCamera.orthographicSize = baseCameraSize / CameraZoom;
    }
}
