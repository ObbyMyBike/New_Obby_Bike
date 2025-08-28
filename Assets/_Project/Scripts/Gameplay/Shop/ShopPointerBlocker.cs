using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ShopPointerBlocker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] private UIInfo _uiInfo;

    private void Awake()
    {
        var graphic = GetComponent<Graphic>();
        
        if (graphic)
            graphic.raycastTarget = false;
        
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup)
            canvasGroup.blocksRaycasts = false;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        _uiInfo.Down();
        
        ReleaseCursor();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _uiInfo.Up();
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        ReleaseCursor();
    }

    private void ReleaseCursor()
    {
        if (!Application.isMobilePlatform)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}