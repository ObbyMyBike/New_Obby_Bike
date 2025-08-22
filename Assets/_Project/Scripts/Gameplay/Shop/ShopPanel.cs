using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

public class ShopPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _goldPlayerText;
    
    [Header("Items")]
    [SerializeField] private List<ItemData> _allItems;
    [SerializeField] private Transform _slotsContainer;
    [SerializeField] private ItemUI _itemSlotPrefab;

    [Header("Settings UI Panel")]
    [SerializeField] private RectTransform _shopPanel;
    [SerializeField] private float _hiddenX = -500f;
    [SerializeField] private float _shownX = 30f;
    [SerializeField] private float _shownY = 50f;
    [SerializeField] private float _tweenDuration = 0.5f;
    
    [Header("Debug / Testing")]
    [SerializeField] private Button _resetButton;

    private readonly Dictionary<ItemUI, ItemData> _dataSlots = new Dictionary<ItemUI, ItemData>();
    
    private CurrencyService _currencyService;
    private PlayerSkin _playerSkin;
    private SkinSaver _skinSaver;
    private UIInfo _uiInfo;
    private DiContainer _container;

    [Inject]
    public void Construct(CurrencyService currencyService, PlayerSkin playerSkin, UIInfo uiInfo, DiContainer container, SkinSaver skinSaver)
    {
        _currencyService = currencyService;
        _playerSkin = playerSkin;
        _uiInfo = uiInfo;
        _container = container;
        _skinSaver = skinSaver;
        
        _currencyService.GoldChanged += OnGoldChanged;
    }

    private void Awake()
    {
        _shopPanel.anchoredPosition = new Vector2(_hiddenX, 0);
        
        if (_resetButton != null)
            _resetButton.onClick.AddListener(OnResetClicked);
        
        if (_container != null && _slotsContainer != null && _itemSlotPrefab != null)
            PopulateSlots();

        if (_currencyService != null)
            OnGoldChanged(_currencyService.CurrentGold);
    }
    
    private void OnDestroy()
    {
        if (_currencyService != null)
            _currencyService.GoldChanged -= OnGoldChanged;
    }

    public void Show()
    {
        _uiInfo.Down();
        
        if (!Application.isMobilePlatform)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        
        OnGoldChanged(_currencyService.CurrentGold);
        
        _shopPanel.DOAnchorPos(new Vector2(_shownX, _shownY), _tweenDuration).SetEase(Ease.OutCubic);
    }

    public void Hide()
    {
        _uiInfo.Up();
        
        if (!Application.isMobilePlatform)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        _shopPanel.DOAnchorPosX(_hiddenX, _tweenDuration).SetEase(Ease.OutCubic);
    }
    
    private async void OnSlotPurchased(ItemType type)
    {
        ItemUI slot = _dataSlots.Keys.FirstOrDefault(item => item.Data.Type == type);
        
        if (slot == null)
            return;

        ItemData data = _dataSlots[slot];
        
        if (data.CharacterSkin != null)
        {
            await _playerSkin.ApplyCharacterSkinAsync(data.CharacterSkin);
            
            string id = data.CharacterSkin.name;
            
            _skinSaver.AddPurchased(id);
            _skinSaver.SetSelected(id);
            
            slot.MarkAsPurchased();
        }
    }
    
    private void PopulateSlots()
    {
        foreach (Transform slotTransform in _slotsContainer) 
            Destroy(slotTransform.gameObject);
        
        _dataSlots.Clear();
        
        foreach (ItemData itemData in _allItems)
        {
            ItemUI slot = _container.InstantiatePrefabForComponent<ItemUI>(_itemSlotPrefab.gameObject, _slotsContainer);
            
            slot.Setup(itemData);

            slot.Purchased += OnSlotPurchased;
            
            _dataSlots[slot] = itemData;
        }
    }

    private void OnGoldChanged(int newGold)
    {
        if (_goldPlayerText != null)
            _goldPlayerText.text = $"Gold: {newGold}";
    }
    
    private void OnResetClicked()
    {
        _skinSaver.ClearAll();
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}