using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

public class BikeProgressUI : MonoBehaviour
{
    [Header("Progress Settings")]
    [SerializeField] private BikeSkinData[] _bikeSkins;
    [SerializeField] private int _checkpointsPerStep = 5;
    [SerializeField] private float _progressStep = 25f;

    [Header("UI References")]
    [SerializeField] private GameObject _progressPanel;
    [SerializeField] private Image _fillableProgressImage;
    [SerializeField] private Image _bikeBackgroundImage;
    [SerializeField] private Image _rightClickHintImage;
    [SerializeField] private TMP_Text _progressText;
    [SerializeField] private TMP_Text _newBikeUnlockedText;
    [SerializeField] private Button _closeButton;
    [SerializeField] private float _fillDuration = 1f;
    [SerializeField] private float _autoHideDelay = 2f;

    private BikeProgressManager _manager;
    private Coroutine _fillCoroutine;
    private Coroutine _autoHideCoroutine;
    
    [Inject] private IEnumerable<CheckPoints> _checkPoints; 

    private bool IsDesktopLike => !Application.isMobilePlatform;
    
    private void Awake()
    {
        _manager = new BikeProgressManager(_checkpointsPerStep, _progressStep, _bikeSkins, this);

        _fillableProgressImage.type = Image.Type.Filled;
        _fillableProgressImage.fillMethod = Image.FillMethod.Vertical;
        _fillableProgressImage.fillAmount = 0f;

        _progressPanel.SetActive(false);
        _newBikeUnlockedText?.gameObject.SetActive(false);
        _rightClickHintImage?.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        CheckPoints.Reached += OnReached; 
        _manager.ProgressUpdated += UpdateProgressUI;
        _manager.BikeUnlocked += OnBikeUnlocked;
        _manager.ProgressReseted += OnProgressReseted;
        
        _closeButton.onClick.AddListener(HideProgressPanel);
    }

    private void OnDisable()
    {
        CheckPoints.Reached -= OnReached; 
        _manager.ProgressUpdated -= UpdateProgressUI;
        _manager.BikeUnlocked -= OnBikeUnlocked;
        _manager.ProgressReseted -= OnProgressReseted;
        
        _closeButton.onClick.RemoveListener(HideProgressPanel);
    }
    
    private void Update()
    {
        if (IsDesktopLike && _progressPanel.activeSelf && Input.GetMouseButtonDown(1))
            HideProgressPanel();
    }

    private void UpdateProgressUI(float progress, Sprite uiSprite)
    {
        float maxPercents = 100f;
        
        ShowProgressPanel();

        if (uiSprite != null)
        {
            _fillableProgressImage.sprite = uiSprite;
            _bikeBackgroundImage.sprite = uiSprite;
        }

        if (_fillCoroutine != null)
            StopCoroutine(_fillCoroutine);

        _fillCoroutine = StartCoroutine(AnimateFill(progress / maxPercents));

        _progressText.text = $"{Mathf.RoundToInt(progress)}%";
        _progressText.gameObject.SetActive(true);
        _newBikeUnlockedText.gameObject.SetActive(false);
    }

    private void ShowProgressPanel()
    {
        _progressPanel.SetActive(true);
        _rightClickHintImage?.gameObject.SetActive(IsDesktopLike);

        CancelAutoHide();

        _autoHideCoroutine = StartCoroutine(AutoHide());
    }

    private void CancelAutoHide()
    {
        if (_autoHideCoroutine != null)
        {
            StopCoroutine(_autoHideCoroutine);

            _autoHideCoroutine = null;
        }
    }

    private void HideProgressPanel()
    {
        CancelAutoHide();

        _progressPanel.SetActive(false);
    }

    private void OnBikeUnlocked()
    {
        ShowProgressPanel();
        
        _progressText.gameObject.SetActive(true);
        _newBikeUnlockedText.gameObject.SetActive(true);
    }

    private void OnProgressReseted()
    {
        CancelAutoHide();
        
        _progressPanel.SetActive(false);
        _progressText.gameObject.SetActive(false);
        _newBikeUnlockedText.gameObject.SetActive(false);
        _fillableProgressImage.fillAmount = 0f;
    }

    private void OnReached(CheckPoints _)
    {
        _manager.CheckpointReached();
    }
    
    private IEnumerator AnimateFill(float targetFill)
    {
        float start = _fillableProgressImage.fillAmount;
        float time = 0f;
        
        while (time < 1f)
        {
            time += Time.deltaTime / _fillDuration;
            
            _fillableProgressImage.fillAmount = Mathf.Lerp(start, targetFill, time);
            
            yield return null;
        }
        
        _fillableProgressImage.fillAmount = targetFill;
    }
    
    private IEnumerator AutoHide()
    {
        yield return new WaitForSeconds(_autoHideDelay);

        HideProgressPanel();
    }
}