using UnityEngine;
using TMPro;

public class RacePlaceView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _placeText;
    
    public void SetPlace(int place, int total)
    {
        if (_placeText == null)
            return;
        
        _placeText.text = $"{place}/{total}";
    }
}