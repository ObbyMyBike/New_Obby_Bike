using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressBarView : MonoBehaviour
{
    private const float PROGRESS_MIN = 0f;
    private const float PROGRESS_MAX = 1f;
    private const int PERCENT_MAX = 100;
    
    [SerializeField] private Slider _slider;
    [SerializeField] private TextMeshProUGUI _percentText;
    [SerializeField] private GameObject _botMarkerPrefab;
    [SerializeField] private GameObject _playerMarkerPrefab;
    [SerializeField] private RectTransform _progressBarRect;
    [SerializeField] private float _horizontalPaddingPixels = 10f;

    [Header("Colors and animations")]
    [SerializeField] private float _animationDurationSeconds = 0.5f;
    [SerializeField] private Color _playerMarkerColor = new Color(1f, 0.55f, 0.1f, 1f);
    [SerializeField] private Vector2 _forbiddenHueRangeDeg = new Vector2(30f, 60f);
    [SerializeField] private Vector2 _botSaturationRange = new Vector2(0.5f, 1f);
    [SerializeField] private Vector2 _botValueRange = new Vector2(0.7f, 1f);
    
    private readonly Dictionary<GameObject, MarkerController> botMarkers = new Dictionary<GameObject, MarkerController>();
    
    private HorizontalBarLayout _barLayout;
    private ColorGenerator _colorGenerator;
    private MarkerController _playerMarkerController;
    private RectTransform _markerParent; 
    private Coroutine _sliderAnimation;

    private RectTransform _barLayoutRect => _progressBarRect != null ? _progressBarRect : (_slider != null ? _slider.GetComponent<RectTransform>() : null);
    
    private void Start()
    {
        if (_slider != null)
        {
            _slider.minValue = PROGRESS_MIN;
            _slider.maxValue = PROGRESS_MAX;
            _slider.value = PROGRESS_MIN;
        }
        
        if (_percentText != null)
            _percentText.text = "0%";
        
        RectTransform trackRect = _progressBarRect != null ? _progressBarRect : (_slider != null ? _slider.GetComponent<RectTransform>() : null);
        
        if (trackRect == null)
            return;
        
        ForceBuildLayout(trackRect);

        _barLayout = new HorizontalBarLayout(trackRect, _horizontalPaddingPixels);
        _colorGenerator = new ColorGenerator(_forbiddenHueRangeDeg, _botSaturationRange, _botValueRange);
    }

    public void InitializePlayer()
    {
        if (_playerMarkerController != null)
            return;

        GameObject prefabInstance = _playerMarkerPrefab != null ? _playerMarkerPrefab : _botMarkerPrefab;
        GameObject markerInstance = Instantiate(prefabInstance, _progressBarRect);

        if (markerInstance.TryGetComponent(out RectTransform rect))
        {
            if (rect == null)
            {
                Destroy(markerInstance);

                return;
            }
        }

        if (markerInstance.TryGetComponent(out Image image))
            image.color = _playerMarkerColor;

        if (_barLayout != null)
            ForceBuildLayout((_barLayoutRect ?? _progressBarRect));
        
        _playerMarkerController = new MarkerController(rect, _barLayout, _animationDurationSeconds, this);
        _playerMarkerController.SetInstant(0f);
    }
    
    public void UpdatePlayerProgress(float normalizedProgress)
    {
        if (_playerMarkerController == null)
            return;

        _playerMarkerController.AnimateTo(Mathf.Clamp01(normalizedProgress));
    }

    public void AnimatePlayerProgress(float newPercent)
    {
        float clampedPercent = Mathf.Clamp(newPercent, 0f, PERCENT_MAX);
        float targetSliderValue = clampedPercent / PERCENT_MAX;

        if (_sliderAnimation != null)
            StopCoroutine(_sliderAnimation);

        _sliderAnimation = StartCoroutine(AnimateSliderTo(targetSliderValue));
    }

    public void InitializeBot(GameObject bot)
    {
        if (bot == null || botMarkers.ContainsKey(bot))
            return;
        
        if (botMarkers.ContainsKey(bot))
            return;

        GameObject markerObject = Instantiate(_botMarkerPrefab, _progressBarRect);

        if (markerObject.TryGetComponent(out RectTransform rect))
        {
            if (rect == null)
            {
                Destroy(markerObject);
                
                return;
            }
        }

        if (markerObject.TryGetComponent(out Image image))
            image.color = _colorGenerator.RandomColorAvoidingForbiddenRange();

        MarkerController controller = new MarkerController(rect, _barLayout, _animationDurationSeconds, this);
        controller.SetInstant(0f);

        botMarkers.Add(bot, controller);
    }

    public void UpdateBotProgress(GameObject bot, float normalizedProgress)
    {
        if (bot == null)
            return;

        if (botMarkers.TryGetValue(bot, out MarkerController controller))
            controller.AnimateTo(Mathf.Clamp01(normalizedProgress));
    }

    private IEnumerator AnimateSliderTo(float targetValue)
    {
        float startValue = _slider.value;
        float elapsed = 0f;

        if (_percentText != null)
            _percentText.text = $"{Mathf.RoundToInt(targetValue * PERCENT_MAX)}%";

        while (elapsed < _animationDurationSeconds)
        {
            elapsed += Time.deltaTime;

            float time = Mathf.Clamp01(elapsed / _animationDurationSeconds);
            float current = Mathf.Lerp(startValue, targetValue, time);

            _slider.value = current;

            if (_percentText != null)
                _percentText.text = $"{Mathf.RoundToInt(current * PERCENT_MAX)}%";

            yield return null;
        }

        _slider.value = targetValue;

        if (_percentText != null)
            _percentText.text = $"{Mathf.RoundToInt(targetValue * PERCENT_MAX)}%";

        _sliderAnimation = null;
    }
    
    private void ForceBuildLayout(RectTransform rect)
    {
        if (rect.rect.width <= 0f || rect.rect.height <= 0f)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }
    }
}