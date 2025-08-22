using UnityEngine;
using UnityEngine.UI;

public class SkinProgressView
{
    private readonly Slider _slider; 
    private readonly bool _hideWhenEmpty = true;

    public SkinProgressView(Slider slider, bool hideWhenEmpty)
    {
        _slider = slider;
        _hideWhenEmpty = hideWhenEmpty;
        
        _slider.minValue = 0f;
        _slider.maxValue = 1f;
        _slider.value = 0f;
        
        if (_hideWhenEmpty)
            _slider.gameObject.SetActive(false);
    }

    public void Show()
    {
        if (_hideWhenEmpty)
            _slider.gameObject.SetActive(true);
        
        _slider.value = 0f;
    }

    public void Hide()
    {
        _slider.value = 0f;
        
        if (_hideWhenEmpty)
            _slider.gameObject.SetActive(false);
    }

    public void Set01(float value01)
    {
        _slider.value = Mathf.Clamp01(value01);
    }
}