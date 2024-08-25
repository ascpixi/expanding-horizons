using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class InteractableUIRenderer : MonoBehaviour
{
    public GameObject IndicatorPrefab;
    public Transform ElementParent;
    public float AppearDuration = 0.5f;
    public AudioSource Audio;
    public AudioClip AppearSfx;
    public AudioClip DisappearSfx;
    
    public static InteractableUIRenderer Main { get; private set; }

    public static void DestroyElement(Interactable target)
    {
        target.UITween?.Kill();
        target.UITween = target.UIElement.rectTransform.DOScale(0, Main.AppearDuration)
            .OnComplete(() => Destroy(target.UIElement));
                
        target.UIShown = false;
    }
    
    void Start()
    {
        Main = this;
        
        foreach (var interactable in Interactable.All) {
            var element = Instantiate(
                IndicatorPrefab,
                interactable.UIElementPosition,
                Quaternion.identity,
                ElementParent
            );

            var img = element.GetComponent<Image>();
            img.enabled = false;
            img.rectTransform.localScale = new(0, 0);

            interactable.UIElement = img;
        }
    }

    void Update()
    {
        foreach (var reg in Interactable.All) {
            if (reg.PlayerInRange && !reg.UIShown) {
                reg.UITween?.Kill();
                reg.UIElement.enabled = true;
                reg.UITween = reg.UIElement.rectTransform.DOScale(1, AppearDuration);
                reg.UIShown = true;
                
                Audio.PlayOneShot(AppearSfx);
            } else if (!reg.PlayerInRange && reg.UIShown) {
                reg.UITween?.Kill();
                reg.UITween = reg.UIElement.rectTransform.DOScale(0, AppearDuration)
                    .OnComplete(() => reg.UIElement.enabled = false);
                
                reg.UIShown = false;
                
                Audio.PlayOneShot(DisappearSfx);
            }

            var uiPos = RectTransformUtility.WorldToScreenPoint(
                Camera.main,
                reg.transform.TransformPoint(reg.UIElementPosition)
            );
            
            reg.UIElement.transform.position = uiPos;
        }
    }
}
