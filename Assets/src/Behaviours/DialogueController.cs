using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    public Image Background;
    public TextMeshProUGUI TextRenderer;
    public Transform Speaker;
    public Vector2 SpeakerOffset;
    public Vector2 Padding;
    public AudioClip[] SpeakSfx;
    public AudioSource Audio;
    public float AudioVolume = 1;
    
    
    public void DisplayDialogue(DialogueData data)
        => StartCoroutine(DialogueCoroutine(data));

    void PlayTalkSfx()
    {
        Audio.Stop();
        Audio.PlayOneShot(SpeakSfx.Random(), AudioVolume);
    }
    
    public IEnumerator DialogueCoroutine(DialogueData data)
    {
        bool prevFrozen = GlobalGameBehaviour.Frozen;
        GlobalGameBehaviour.Frozen = true;

        TextRenderer.gameObject.SetActive(true);
        Background.gameObject.SetActive(true);
        
        for (var i = 0; i < data.Lines.Length; i++) {
            PlayTalkSfx();
            
            string line = data.Lines[i];
            var bgOffset = data.BackgroundOffset[i];

            TextRenderer.text = line;

            yield return null;
            
            Background.rectTransform.sizeDelta = TextRenderer.textBounds.size + Padding.WithZ(0);
            Background.rectTransform.localPosition = bgOffset;

            while (!Input.GetButtonDown("Primary")) {
                yield return null;
            }
        }
        
        TextRenderer.gameObject.SetActive(false);
        Background.gameObject.SetActive(false);

        GlobalGameBehaviour.Frozen = prevFrozen;
    }

    void Update()
    {
        var uiPos = RectTransformUtility.WorldToScreenPoint(
            Camera.main,
            Speaker.TransformPoint(new(0, 0, 0))
        );

        transform.position = uiPos + SpeakerOffset;
    }
}