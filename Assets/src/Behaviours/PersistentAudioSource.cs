using System.Collections;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Represents an audio source that persists between scene loads.
/// </summary>
public class PersistentAudioSource : MonoBehaviour
{
    public float FadeOutLength;
    public float FadeInLength = 1;
    public bool Forceful;
    
    public AudioSource[] Tracks { get; private set; }
    
    public static PersistentAudioSource Existing { get; private set; }
    
    IEnumerator Start()
    {
        Tracks = GetComponents<AudioSource>();
        
        if (Existing != null) {
            if (!Forceful) {
                Debug.Log("A previous persistent audio source has been detected - removing the scene's one.");
                Destroy(gameObject);
                yield break;
            }
            
            Debug.Log($"A previous persistent audio source has been detected - fading it out over {Existing.FadeOutLength}s.");

            foreach (var track in Existing.Tracks) {
                track.DOFade(0, FadeOutLength);
            }

            var oldExisting = Existing;
            this.RunAfter(FadeOutLength, () => Destroy(oldExisting.gameObject));
            
            yield return new WaitForSeconds(FadeOutLength / 2f);
        }

        DontDestroyOnLoad(gameObject);
        Existing = this;

        foreach (var track in Tracks) {
            track.Play();

            if (FadeInLength != 0) {
                float targetVolume = track.volume;
                track.volume = 0f;
                track.DOFade(targetVolume, FadeInLength);
            }
        }
    }
}
