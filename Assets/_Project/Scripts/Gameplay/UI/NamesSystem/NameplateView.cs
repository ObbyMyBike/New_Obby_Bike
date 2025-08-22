using UnityEngine;
using TMPro;

public class NameplateView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    
    public void SetText(string value) => _text.text = value;
}