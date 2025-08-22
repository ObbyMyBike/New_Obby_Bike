using UnityEngine;

public class ColorGenerator
{
    private readonly Vector2 forbiddenHueRangeDeg;
    private readonly Vector2 saturationRange;
    private readonly Vector2 valueRange;

    public ColorGenerator(Vector2 forbiddenHueRangeDeg, Vector2 saturationRange, Vector2 valueRange)
    {
        this.forbiddenHueRangeDeg = forbiddenHueRangeDeg;
        this.saturationRange = saturationRange;
        this.valueRange = valueRange;
    }

    public Color RandomColorAvoidingForbiddenRange()
    {
        float hueDeg = Random.value * 360f;
        
        if (forbiddenHueRangeDeg.x < forbiddenHueRangeDeg.y)
        {
            while (hueDeg >= forbiddenHueRangeDeg.x && hueDeg <= forbiddenHueRangeDeg.y)
                hueDeg = Random.value * 360f;
        }

        float saturation = Random.Range(saturationRange.x, saturationRange.y);
        float value = Random.Range(valueRange.x, valueRange.y);
        
        return Color.HSVToRGB(hueDeg / 360f, saturation, value);
    }
}