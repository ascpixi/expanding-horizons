using System;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;

public enum ViewportSide
{
    None = -1,
    Top,
    Bottom,
    Left,
    Right
}

public class ViewportBehaviour : MonoBehaviour
{
    public float SizeDelta;
    public Vector2 MaxSize;
    public float SizeChangeTweenDuration;
    public float SizeReturnTweenDuration = 0.05f; // This should be really short
    public Transform FollowTransform;
    public float HitboxClampMultiplier = 1.5f;
    
    [Header("Border Objects")]
    public SpriteRenderer UpperSideBorder;
    public SpriteRenderer RightSideBorder;
    public SpriteRenderer LeftSideBorder;
    public SpriteRenderer LowerSideBorder;

    [Header("Border Sprites")]
    public Sprite HorzSideSprite;
    public Sprite VertSideSprite;
    public Sprite AnchorHorzSideSprite;
    public Sprite AnchorVertSideSprite;
    
    public ViewportSide CurrentSide { get; private set; } = ViewportSide.None;
    
    Vector3[] sides = new Vector3[4];
    [CanBeNull] Tween sideTransitionTween;
    float sideAnchorExtent;
    float anchorSize;          // The amount to add to the side's appropriate dimension
    
    int returningSideIdx = -1; // The index of the side that is returning to its original size
    float returnAnimPos;
    Vector3 returnSideOrigin;
    SpriteRenderer[] sideBorders;

    const int TopIndex = (int)ViewportSide.Top;
    const int BottomIndex = (int)ViewportSide.Bottom;
    const int LeftIndex = (int)ViewportSide.Left;
    const int RightIndex = (int)ViewportSide.Right;
    
    public Vector3 OriginScale { get; set; }
    
    public Vector3 ViewportPosition {
        get => FollowTransform.transform.position;
        set => FollowTransform.transform.position = value;
    }

    public static ViewportBehaviour Main { get; private set; }

    public bool IsInViewport(Vector2 vec)
    {
        var bounds = new Bounds(ViewportPosition, transform.localScale);
        return bounds.Contains(vec);
    }

    // A passive version of the more forceful "sides[idx] = vec".
    // An assignment with this method may be ignored if, e.g., a quick animation is playing.
    void SetSide(int idx, Vector2 vec)
    {
        if (returningSideIdx == idx)
            return;

        sides[idx] = vec;
    }
    
    void Awake()
    {
        if (Main != null)
            throw new Exception("There may be only one ViewportBehaviour in a scene.");

        Main = this;
    }

    void Start()
    {
        OriginScale = transform.localScale;

        UpperSideBorder.sprite = LowerSideBorder.sprite = HorzSideSprite;
        RightSideBorder.sprite = LeftSideBorder.sprite = VertSideSprite;

        sideBorders = new SpriteRenderer[4];
        sideBorders[TopIndex] = UpperSideBorder;
        sideBorders[BottomIndex] = LowerSideBorder;
        sideBorders[LeftIndex] = LeftSideBorder;
        sideBorders[RightIndex] = RightSideBorder;
    }

    void Update()
    {
        var ppos = PlayerController.Main.Position;
        
        // If a side is selected, the scale of the viewport stays anchored to that side.
        // For example, if the player moves right, and the left side of the viewport
        // is selected, the viewport should extend.
        // CalculateSides();
        
        Debugging.DrawCircle(sides[0], .25f, CurrentSide == ViewportSide.Top ? Color.green : Color.red);
        Debugging.DrawCircle(sides[1], .25f, CurrentSide == ViewportSide.Bottom ? Color.green : Color.red);
        Debugging.DrawCircle(sides[2], .25f, CurrentSide == ViewportSide.Left ? Color.green : Color.red);
        Debugging.DrawCircle(sides[3], .25f, CurrentSide == ViewportSide.Right ? Color.green : Color.red);
        
        // TODO: Clean this up
        switch (CurrentSide) {
            case ViewportSide.None: {
                SetSide(TopIndex, ppos + new Vector3(0, OriginScale.y / 2)); 
                SetSide(BottomIndex, ppos - new Vector3(0, OriginScale.y / 2)); 
                SetSide(RightIndex, ppos + new Vector3(OriginScale.x / 2, 0)); 
                SetSide(LeftIndex, ppos - new Vector3(OriginScale.x / 2, 0)); 
                break;
            }
            case ViewportSide.Top: {
                SetSide(TopIndex, new Vector3(ppos.x, sideAnchorExtent + anchorSize));
                SetSide(BottomIndex, ppos - new Vector3(0, OriginScale.y / 2)); 
                SetSide(RightIndex, ppos + new Vector3(OriginScale.x / 2, 0)); 
                SetSide(LeftIndex, ppos - new Vector3(OriginScale.x / 2, 0)); 
                break;
            }
            case ViewportSide.Bottom: {
                SetSide(TopIndex, ppos + new Vector3(0, OriginScale.y / 2));
                SetSide(BottomIndex, new Vector3(ppos.x, sideAnchorExtent - anchorSize)); 
                SetSide(RightIndex, ppos + new Vector3(OriginScale.x / 2, 0)); 
                SetSide(LeftIndex, ppos - new Vector3(OriginScale.x / 2, 0));
                break;
            }
            case ViewportSide.Right: {
                SetSide(TopIndex, ppos + new Vector3(0, OriginScale.y / 2)); 
                SetSide(BottomIndex, ppos - new Vector3(0, OriginScale.y / 2)); 
                SetSide(RightIndex, new Vector3(sideAnchorExtent + anchorSize, ppos.y)); 
                SetSide(LeftIndex, ppos - new Vector3(OriginScale.x / 2, 0));
                break;
            }
            case ViewportSide.Left: {
                SetSide(TopIndex, ppos + new Vector3(0, OriginScale.y / 2)); 
                SetSide(BottomIndex, ppos - new Vector3(0, OriginScale.y / 2)); 
                SetSide(RightIndex, ppos + new Vector3(OriginScale.x / 2, 0)); 
                SetSide(LeftIndex, new Vector3(sideAnchorExtent - anchorSize, ppos.y));
                break;
            }
        }

        if (returnAnimPos > 0f && returningSideIdx != -1) {
            returnAnimPos = Mathf.Max(0, returnAnimPos - Time.deltaTime);

            if (returnAnimPos != 0) {
                var targetPos = returningSideIdx switch {
                    TopIndex    => ppos + new Vector3(0, OriginScale.y / 2),
                    BottomIndex => ppos - new Vector3(0, OriginScale.y / 2),
                    RightIndex  => ppos + new Vector3(OriginScale.x / 2, 0),
                    LeftIndex   => ppos - new Vector3(OriginScale.x / 2, 0),
                    _ => throw new Exception($"We should never be here! (CurrentSide = {CurrentSide})")
                };
                
                sides[returningSideIdx] = Vector3.Lerp(
                    targetPos,
                    returnSideOrigin,
                    Mathf.Clamp01(returnAnimPos / SizeReturnTweenDuration)
                );
            }
            else  {
                returningSideIdx = -1;
            }
        }
        
        ClampSides();
        UpdateSides();
    }

    // Calculates the centers of the viewport's sides, writing the results to the 'sides' buffer.
    void CalculateSides()
    {
        sides[0] = ViewportPosition + new Vector3(0, transform.localScale.y / 2); // top-center
        sides[1] = ViewportPosition - new Vector3(0, transform.localScale.y / 2); // bottom-center
        sides[2] = ViewportPosition - new Vector3(transform.localScale.x / 2, 0); // left-center
        sides[3] = ViewportPosition + new Vector3(transform.localScale.x / 2, 0); // right-center
    }
    
    void UpdateSides()
    {
        transform.localScale = new Vector3(
            Vector3.Distance(sides[3], sides[2]),
            Vector3.Distance(sides[0], sides[1]),
            transform.localScale.z
        );

        ViewportPosition = new Vector3(
            (sides[RightIndex].x + sides[LeftIndex].x) / 2,
            (sides[TopIndex].y + sides[BottomIndex].y) / 2,
            ViewportPosition.z
        );
        
        // Update the borders
        Vector3 sideLeft = sides[LeftIndex],
                sideRight = sides[RightIndex],
                sideTop = sides[TopIndex],
                sideBottom = sides[BottomIndex];

        sideLeft.y = sideRight.y = (sideTop.y + sideBottom.y) / 2;
        sideTop.x = sideBottom.x = (sideLeft.x + sideRight.x) / 2;
        
        LeftSideBorder.transform.position = sideLeft.Add(-0.5f, 0);
        LeftSideBorder.size = LeftSideBorder.size.WithY(transform.localScale.y);
        
        RightSideBorder.transform.position = sideRight.Add(0.5f, 0);
        RightSideBorder.size = RightSideBorder.size.WithY(transform.localScale.y);
        
        UpperSideBorder.transform.position = sideTop.Add(0, 0.5f);
        UpperSideBorder.size = UpperSideBorder.size.WithX(transform.localScale.x);
        
        LowerSideBorder.transform.position = sideBottom.Add(0, -0.5f);
        LowerSideBorder.size = LowerSideBorder.size.WithX(transform.localScale.x);
    }
    
    void ClampSides()
    {
        sides[TopIndex] = new Vector3(sides[TopIndex].x, Math.Min(sides[TopIndex].y, MaxSize.y / 2));
        sides[BottomIndex] = new Vector3(sides[BottomIndex].x, Math.Max(sides[BottomIndex].y, -MaxSize.y / 2));
        sides[RightIndex] = new Vector3(Math.Min(sides[RightIndex].x, MaxSize.x / 2), sides[RightIndex].y);
        sides[LeftIndex] = new Vector3(Math.Max(sides[LeftIndex].x, -MaxSize.x / 2), sides[LeftIndex].y);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, MaxSize);
    }
#endif
    
    /// <summary>
    /// Changes the current bound side.
    /// When a side is bound, it is anchored to the position at the frame that
    /// this method was invoked at.
    /// </summary>
    /// <param name="newSide"></param>
    public void ChangeCurrentSide(ViewportSide newSide)
    {
        if (sideTransitionTween is { active: true }) {
            sideTransitionTween.Complete();
            returningSideIdx = -1;
        }
        
        if (CurrentSide != ViewportSide.None) {
            returnSideOrigin = sides[(int)CurrentSide];
            returnAnimPos = SizeReturnTweenDuration;
            returningSideIdx = (int)CurrentSide;

            sideBorders[(int)CurrentSide].sprite = CurrentSide is ViewportSide.Top or ViewportSide.Bottom
                ? HorzSideSprite
                : VertSideSprite;
        }
        
        CurrentSide = newSide;

        if (newSide == ViewportSide.None)
            return;
        
        CalculateSides();
            
        sideAnchorExtent = newSide is ViewportSide.Left or ViewportSide.Right
            ? sides[(int)newSide].x
            : sides[(int)newSide].y;

        anchorSize = 0f;
            
        sideBorders[(int)CurrentSide].sprite = newSide is ViewportSide.Top or ViewportSide.Bottom
            ? AnchorHorzSideSprite
            : AnchorVertSideSprite;
    }

    /// <summary>
    /// Gets the viewport side the given point resides on, with an acceptable margin of error
    /// on the opposing axis of 0.25 world units.
    /// </summary>
    public ViewportSide GetSideByPoint(Vector2 point)
    {
        if (new Bounds(sides[RightIndex], new Vector3(0.25f, 1000f)).Contains(point))
            return ViewportSide.Right;
        
        if (new Bounds(sides[LeftIndex], new Vector3(0.25f, 1000f)).Contains(point))
            return ViewportSide.Left;
        
        if (new Bounds(sides[TopIndex], new Vector3(1000f, 0.5f)).Contains(point))
            return ViewportSide.Top;
        
        if (new Bounds(sides[BottomIndex], new Vector3(1000f, 0.5f)).Contains(point))
            return ViewportSide.Bottom;

        return ViewportSide.None;
    }

    /// <summary>
    /// Makes the viewport expand by <see cref="SizeDelta"/>.
    /// </summary>
    public void Expand() => ChangeSizeOfSide(SizeDelta);

    /// <summary>
    /// Makes the viewport shrink by <see cref="SizeDelta"/>.
    /// </summary>
    public void Shrink() => ChangeSizeOfSide(-SizeDelta);
    
    void ChangeSizeOfSide(float delta)
    {
        if (CurrentSide == ViewportSide.None)
            throw new Exception("Attempted to change size of the current side while no side is bound");
        
        var playerPosition = PlayerController.Main.transform.position;
        var hitbox = PlayerController.Main.GetComponent<BoxCollider2D>();
        
        if (delta < 0) {
            // If we are decreasing the size, calculate the distance between the appropriate
            // player hitbox side and the side that we are shrinking.
            // If |distance| is lower than |delta|, we need to clamp it, as we would shrink
            // the viewport beyond the position of the player.
            float hitboxSideDim = CurrentSide switch {
                ViewportSide.Top    => playerPosition.y + (hitbox.size.y * HitboxClampMultiplier),
                ViewportSide.Bottom => playerPosition.y - (hitbox.size.y * HitboxClampMultiplier),
                ViewportSide.Left   => playerPosition.x - (hitbox.size.x * HitboxClampMultiplier),
                ViewportSide.Right  => playerPosition.x + (hitbox.size.x * HitboxClampMultiplier),
                _                   => throw new ArgumentOutOfRangeException(nameof(CurrentSide), CurrentSide, null)
            };
            
            float sideDim = CurrentSide switch {
                ViewportSide.Top    => sides[0].y,
                ViewportSide.Bottom => sides[1].y,
                ViewportSide.Left   => sides[2].x,
                ViewportSide.Right  => sides[3].x,
                _ => throw new ArgumentOutOfRangeException(nameof(CurrentSide), CurrentSide, null)
            };
            
            float sideDist = Math.Abs(hitboxSideDim - sideDim);
            
            delta = Math.Abs(delta) > sideDist ? -sideDist : delta;
        }
        
        DOTween.To(
            () => anchorSize,
            x => anchorSize = x,
            anchorSize + delta,
            SizeChangeTweenDuration
        );
    }
}
