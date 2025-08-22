using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PushPopupView : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private TextMeshProUGUI _labelText;

    [Header("Animations")]
    [SerializeField] private Ease _easeIn = Ease.OutBack;
    [SerializeField] private Ease _easeOut = Ease.InSine;
    [SerializeField] private float _showScale = 1f;
    [SerializeField] private float _scaleTime = 0.2f;
    [SerializeField] private float _stayTime = 0.8f;
    [SerializeField] private float _hideTime = 0.2f;

    private RectTransform _imageRect;
    private CanvasGroup _group;
    private Sequence _seq;
    private void Awake()
    {
        if (_backgroundImage == null)
            _backgroundImage = GetComponentInChildren<Image>(true);

        _imageRect = _backgroundImage != null ? _backgroundImage.rectTransform : transform as RectTransform;
        
        if (_backgroundImage != null)
        {
            _group = _backgroundImage.GetComponent<CanvasGroup>();
            
            if (_group == null)
                _group = _backgroundImage.gameObject.AddComponent<CanvasGroup>();
        }
        else
        {
            _group = gameObject.GetComponent<CanvasGroup>();
            
            if (_group == null)
                _group = gameObject.AddComponent<CanvasGroup>();
        }

        _imageRect.localScale = Vector3.zero;
        _group.alpha = 0f;
    }

    private void OnDestroy() => _seq?.Kill();
    
    public void Play(string message)
    {
        if (_labelText != null)
            _labelText.text = message;

        _seq?.Kill();
        _seq = DOTween.Sequence();
        
        _seq.Append(_group.DOFade(1f, _scaleTime));
        _seq.Join(_imageRect.DOScale(_showScale, _scaleTime).SetEase(_easeIn));
        _seq.AppendInterval(_stayTime);
        _seq.Append(_group.DOFade(0f, _hideTime).SetEase(_easeOut));
        _seq.Join(_imageRect.DOScale(0f, _hideTime).SetEase(_easeOut));
        _seq.OnComplete(() => Destroy(gameObject));
    }
}