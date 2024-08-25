using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    static GameObject audioObj;
    static AudioManager mono;
    static readonly Queue<AudioSource> readyAudioSources = new();

    public static float MasterVolume => Mathf.Clamp01(PersistentData.Global.MasterVolume / 100f);
    public static float MusicVolume => Mathf.Clamp01(PersistentData.Global.MusicVolume / 100f);
    public static float SFXVolume => Mathf.Clamp01(PersistentData.Global.SFXVolume / 100f);

    public static void PlayAudio(AudioClip clip, float volume = 1.0f, float pitch = 1.0f)
    {
        AudioSource src;

        if(readyAudioSources.Count == 0) {
            if(audioObj == null) {
                audioObj = new GameObject("(global audio object)");
                mono = audioObj.AddComponent<AudioManager>();

                DontDestroyOnLoad(audioObj);
                DontDestroyOnLoad(mono);
            }

            src = audioObj.AddComponent<AudioSource>();
        } else {
            src = readyAudioSources.Dequeue();
        }

        src.pitch = pitch;
        src.PlayOneShot(clip, volume * SFXVolume * MasterVolume);
        mono.StartCoroutine(WaitAndEmplaceAudioSource(clip, src));
    }

    static IEnumerator WaitAndEmplaceAudioSource(AudioClip clip, AudioSource src)
    {
        yield return new WaitForSeconds(clip.length);
        readyAudioSources.Enqueue(src);
    }
}
