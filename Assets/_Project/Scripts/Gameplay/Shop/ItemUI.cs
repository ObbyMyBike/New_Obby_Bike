using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Zenject;

public class ItemUI : MonoBehaviour
{
    public event Action<ItemType> Purchased;
    
    [SerializeField] private ButtonAnimationSettings _buttonAnimationSettings;
    [SerializeField] private Button _buyButton;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Image _purchasedImage;
    [SerializeField] private TextMeshProUGUI _priceText;
    
    private CurrencyService _currencyService;
    private SkinSaver _skinSaver;
    private PurchaseButtonAnimator _animator;

    [Inject]
    public void Construct(CurrencyService currencyService, SkinSaver skinSaver)
    {
        _currencyService = currencyService;
        _skinSaver = skinSaver;
    }

    public ItemData Data { get; private set; }
    
    private void Awake()
    {
        _animator = new PurchaseButtonAnimator(_buyButton.transform, _purchasedImage, _buttonAnimationSettings.PressScale, _buttonAnimationSettings.PressTime,
            _buttonAnimationSettings.ShakeTime, _buttonAnimationSettings.ShakeStrength, _buttonAnimationSettings.ShakeVibrato);
        
        _buyButton.onClick.AddListener(OnPointerUp);

        var trigger = _buyButton.gameObject.GetComponent<EventTrigger>() ?? _buyButton.gameObject.AddComponent<EventTrigger>();
        trigger.triggers.Clear();
        
        var entryDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        entryDown.callback.AddListener((data) => { _animator.PressDown(); });
        trigger.triggers.Add(entryDown);
    }
    
    public void Setup(ItemData data)
    {
        Data = data;
        
        _iconImage.sprite = data.Icon;
        _priceText.text = data.Price.ToString();

        if (data.CharacterSkin != null && _skinSaver.IsPurchased(data.CharacterSkin.name))
        {
            _buyButton.interactable = false;
            _purchasedImage.gameObject.SetActive(true);
        }
        else
        {
            _buyButton.interactable = true;
            _purchasedImage.gameObject.SetActive(false);
        }
    }

    public void MarkAsPurchased()
    {
        _buyButton.interactable = false;
        _purchasedImage.gameObject.SetActive(true);
        _buyButton.transform.localScale = Vector3.one;
    }
    
    private bool TryBuy()
    {
        if (_currencyService.TrySpend(Data.Price))
        {
            _buyButton.interactable = false;

            Purchased?.Invoke(Data.Type);

            return true;
        }
        else
        {
            return false;
        }
    }
    
    private void OnPointerUp()
    {
        bool success = TryBuy();

        if (success)
            _animator.Success();
        else
            _animator.Fail();
    }
}