using UnityEngine;
using DG.Tweening;

/// <summary>
/// Represents a single block on the game board
/// Handles visuals and animations
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Block : MonoBehaviour
{
    [Header("Components")]
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    
    [Header("Block State")]
    public int ColorID { get; private set; }
    public GridPosition GridPos { get; private set; }
    public bool IsActive { get; private set; }
    
    [Header("Animation Settings")]
    [SerializeField] private float spawnPunchScale = 1.2f;
    [SerializeField] private float spawnPunchDuration = 0.3f;
    [SerializeField] private float clickPunchScale = 1.15f;
    [SerializeField] private float clickPunchDuration = 0.2f;
    
    private Tween currentTween;
    private Tween scaleTween;
    private BlockColorData currentColorData;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }
    }

    /// <summary>
    /// Initialize the block with color and position
    /// </summary>
    public void Initialize(int colorID, GridPosition gridPos, BlockColorData colorData)
    {
        ColorID = colorID;
        GridPos = gridPos;
        currentColorData = colorData;
        IsActive = true;
        
        spriteRenderer.sprite = colorData.DefaultSprite;
        spriteRenderer.enabled = true;
        boxCollider.enabled = true;
        
        // Reset transform
        transform.localScale = Vector3.zero;
        
        // Spawn animation
        KillScaleTween();
        scaleTween = transform.DOScale(Vector3.one, spawnPunchDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(UpdateType.Normal, true);
    }

    /// <summary>
    /// Update the block's grid position
    /// </summary>
    public void SetGridPosition(GridPosition newPos)
    {
        GridPos = newPos;
    }

    /// <summary>
    /// Update the sprite based on group size
    /// </summary>
    public void UpdateSpriteForGroupSize(int groupSize, int thresholdA, int thresholdB, int thresholdC)
    {
        if (!IsActive || currentColorData == null) return;

        Sprite newSprite = currentColorData.DefaultSprite;

        if (groupSize > thresholdC && currentColorData.Icon3Sprite != null)
        {
            newSprite = currentColorData.Icon3Sprite;
        }
        else if (groupSize > thresholdB && currentColorData.Icon2Sprite != null)
        {
            newSprite = currentColorData.Icon2Sprite;
        }
        else if (groupSize > thresholdA && currentColorData.Icon1Sprite != null)
        {
            newSprite = currentColorData.Icon1Sprite;
        }

        spriteRenderer.sprite = newSprite;
    }

    /// <summary>
    /// Animate block falling to new position
    /// </summary>
    public Tween AnimateFall(Vector3 targetPosition, float duration)
    {
        KillMoveTween();
        currentTween = transform.DOLocalMove(targetPosition, duration)
            .SetEase(Ease.InQuad)
            .SetUpdate(UpdateType.Normal, true);
        return currentTween;
    }

    /// <summary>
    /// Play click feedback animation
    /// </summary>
    public void PlayClickFeedback()
    {
        KillScaleTween();
        scaleTween = transform.DOPunchScale(Vector3.one * clickPunchScale, clickPunchDuration, 1, 0.5f)
            .SetUpdate(UpdateType.Normal, true);
    }

    /// <summary>
    /// Deactivate the block (return to pool)
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        KillAllTweens();
        spriteRenderer.enabled = false;
        boxCollider.enabled = false;
    }

    /// <summary>
    /// Kill movement tween
    /// </summary>
    private void KillMoveTween()
    {
        if (currentTween != null && currentTween.IsActive())
        {
            currentTween.Kill();
            currentTween = null;
        }
    }

    /// <summary>
    /// Kill scale tween
    /// </summary>
    private void KillScaleTween()
    {
        if (scaleTween != null && scaleTween.IsActive())
        {
            scaleTween.Kill();
            scaleTween = null;
        }
    }

    /// <summary>
    /// Kill all active tweens on this block
    /// </summary>
    public void KillAllTweens()
    {
        KillMoveTween();
        KillScaleTween();
    }

    private void OnDestroy()
    {
        KillAllTweens();
    }
}
