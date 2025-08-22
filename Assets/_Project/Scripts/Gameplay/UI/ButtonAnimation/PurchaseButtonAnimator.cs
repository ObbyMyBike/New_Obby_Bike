using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PurchaseButtonAnimator
{
    private readonly Transform _buttonTransform;
    private readonly RectTransform _rectTransform;
    private readonly Vector3 _originalScale;
    private readonly Vector3 _originalLocalPos;
    private readonly Vector2 _originalAnchoredPos;
    private readonly Image _purchasedImage;
    private readonly float _pressScale;
    private readonly float _pressTime;
    private readonly float _shakeTime;
    private readonly float _shakeStrength;
    private readonly int _shakeVibrato;

    public PurchaseButtonAnimator(Transform buttonTransform, Image purchasedImage, float pressScale, float pressTime,
        float shakeTime, float shakeStrength, int shakeVibrato)
    {
        _buttonTransform = buttonTransform;
        _rectTransform = buttonTransform as RectTransform;
        _originalScale = buttonTransform.localScale;
        _originalLocalPos = buttonTransform.localPosition;
        _originalAnchoredPos = _rectTransform != null ? _rectTransform.anchoredPosition : Vector2.zero;
        _purchasedImage = purchasedImage;
        _pressScale = pressScale;
        _pressTime = pressTime;
        _shakeTime = shakeTime;
        _shakeStrength = shakeStrength;
        _shakeVibrato = shakeVibrato;

        if (_purchasedImage != null)
            _purchasedImage.gameObject.SetActive(false);
    }

    public void PressDown()
    {
        _buttonTransform.DOScale(_originalScale * _pressScale, _pressTime).SetEase(Ease.OutQuad);
    }

    public void Success()
    {
        _buttonTransform.DOScale(Vector3.zero, _pressTime).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            if (_purchasedImage != null)
                _purchasedImage.gameObject.SetActive(true);
        });
    }

    public void Fail()
    {
        _buttonTransform.DOScale(_originalScale, _pressTime).SetEase(Ease.OutQuad);
        
        const float randomness = 90f;
        
        if (_rectTransform != null)
        {
            _rectTransform.DOShakeAnchorPos(_shakeTime, _shakeStrength, _shakeVibrato, randomness, false)
                .OnComplete(() => { _rectTransform.anchoredPosition = _originalAnchoredPos; });
        }
        else
        {
            _buttonTransform.DOShakePosition(_shakeTime, _shakeStrength, _shakeVibrato, randomness, false)
                .OnComplete(() => { _buttonTransform.localPosition = _originalLocalPos; });
        }
    }
}