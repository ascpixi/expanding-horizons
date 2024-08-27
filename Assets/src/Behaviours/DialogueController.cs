using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
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

    RectTransform rectTransform;
    
    public void DisplayDialogue(string[] lines)
        => StartCoroutine(DialogueCoroutine(lines));

    void PlayTalkSfx()
    {
        Audio.Stop();
        Audio.PlayOneShot(SpeakSfx.Random(), AudioVolume);
    }
    
    public IEnumerator DialogueCoroutine(string[] lines)
    {
        bool prevFrozen = GlobalGameBehaviour.Frozen;
        GlobalGameBehaviour.Frozen = true;

        TextRenderer.gameObject.SetActive(true);
        Background.gameObject.SetActive(true);
        
        foreach (string line in lines) {
            PlayTalkSfx();
            
            var textSize = TextRenderer.GetPreferredValues(line);
            
            TextRenderer.text = line;

            rectTransform.sizeDelta = Background.rectTransform.sizeDelta = textSize.WithZ(1) + Padding.WithZ(0);
            Background.rectTransform.localPosition = new(0, 0);

            TextRenderer.rectTransform.sizeDelta = textSize;
            
            yield return null;
            
            while (!Input.GetButtonDown("Primary")) {
                yield return null;
            }
        }
        
        TextRenderer.gameObject.SetActive(false);
        Background.gameObject.SetActive(false);

        GlobalGameBehaviour.Frozen = prevFrozen;
    }

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
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