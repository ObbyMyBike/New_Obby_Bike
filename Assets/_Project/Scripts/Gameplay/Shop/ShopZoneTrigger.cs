using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ShopZoneTrigger : MonoBehaviour
{
    [SerializeField] private ShopPanel _shopPanel;

    private Collider _collider;
    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _collider.isTrigger = true;
    }

    private void Reset()
    {
        _collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerCharacterRoot player))
            _shopPanel?.Show();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerCharacterRoot player)) 
            _shopPanel?.Hide();
    }
}