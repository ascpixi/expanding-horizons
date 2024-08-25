using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamChargeIndicator : MonoBehaviour
{
    public SpriteRenderer Renderer;
    public int Spacing = 1;
    public float PixelsPerUnit = 16;
    
    [Header("Sprites")]
    public Sprite ActiveSprite;
    public Sprite InactiveSprite;
    public Sprite ChargingHalfSprite;
    public Sprite ChargingFullSprite;

    Sprite computed;

    void Update()
    {
        if (!LevelData.Current.AllowBeam) {
            Renderer.enabled = false;
            return;
        }
        
        Renderer.enabled = true;
        
        var player = PlayerController.Main;
        int count = LevelData.Current.BeamCount; // number of indicators
        int singleWidth = ActiveSprite.texture.width + Spacing;
        int singleHeight = ActiveSprite.texture.height;
        
        var tex2d = new Texture2D(
            count * singleWidth,
            singleHeight,
            ActiveSprite.texture.format,
            false, // mipChain
            false, // linear
            true // createUninitialized
        ) {
            filterMode = FilterMode.Point
        };

        for (var i = 0; i < count; i++) {
            Sprite copySprite;

            if (player.IsRecalling) {
                float beamProg = player.RecallProgress * count;
                float relProg = beamProg - i;

                copySprite = relProg switch {
                    <= 0.0f => InactiveSprite,
                    >= 0.75f => ChargingFullSprite,
                    _ => ChargingHalfSprite
                };
            }
            else {
                copySprite = player.BeamsLeft < (i + 1) ? InactiveSprite : ActiveSprite;
            }
            
            tex2d.SetPixels32(
                singleWidth * i,
                0,
                singleWidth - Spacing,
                singleHeight,
                copySprite.texture.GetPixels32()
            );

            for (var y = 0; y < singleHeight; y++) {
                tex2d.SetPixel((singleWidth * i) - 1, y, default);
            }
        }
        
        tex2d.Apply(false, true);
        computed = Sprite.Create(
            tex2d,
            new Rect(0, 0, tex2d.width, tex2d.height),
            new Vector2(0.5f, 0.5f),
            PixelsPerUnit
        );

        Renderer.sprite = computed;
    }
}
