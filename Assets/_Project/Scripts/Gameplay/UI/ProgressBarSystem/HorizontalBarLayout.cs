using UnityEngine;

public class HorizontalBarLayout
{
    private readonly RectTransform barRect;
    private readonly float horizontalPadding;

    public HorizontalBarLayout(RectTransform barRect, float horizontalPadding)
    {
        this.barRect = barRect;
        this.horizontalPadding = Mathf.Max(0f, horizontalPadding);
    }
    
    public float EvaluateX(float normalizedProgress)
    {
        float clamp = Mathf.Clamp01(normalizedProgress);

        float barWidth = barRect.rect.width;
        float effectiveWidth = Mathf.Max(0f, barWidth - 2f * horizontalPadding);
        
        return clamp * effectiveWidth - (effectiveWidth * 0.5f);
    }
}