using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[RequireComponent(typeof(Collider))]
public class SkinPickupPoint : MonoBehaviour
{
    [SerializeField] private SkinDefinition _skin;
    [SerializeField] private Slider _slider;
    [SerializeField] private bool _hideWhenEmpty = true;
    [SerializeField] private float _holdTime = 3f;

    private PlayerSkin _playerSkin;
    private SkinProgressView _progressView;
    private SkinSaver _skinSaver;
    
    private Collider _collider;
    private Coroutine _progressRoutine;
    
    private bool _granted;

    [Inject]
    public void Construct(PlayerSkin playerSkin, Camera mainCamera, SkinSaver skinSaver)
    {
        _playerSkin = playerSkin;
        _skinSaver = skinSaver;
        
        Canvas canvas = _slider.GetComponentInParent<Canvas>();
        
        if (canvas != null)
            canvas.worldCamera = mainCamera;
    }

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _collider.isTrigger = true;
        _progressView = new SkinProgressView(_slider, _hideWhenEmpty);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other) || _granted)
            return;

        if (_progressRoutine == null)
            _progressRoutine = StartCoroutine(ProgressCoroutine());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
            return;

        if (_progressRoutine != null)
        {
            StopCoroutine(_progressRoutine);
            
            _progressRoutine = null;
            _progressView.Hide();
        }
    }

    private bool IsPlayer(Collider other)
    {
        return other.GetComponent<Player>() != null || other.GetComponentInParent<PlayerCharacterRoot>() != null;
    }
    
    private IEnumerator ProgressCoroutine()
    {
        _progressView.Show();

        float timer = 0f;

        while (timer < _holdTime)
        {
            timer += Time.deltaTime;
            
            _progressView.Set01(timer / _holdTime);
            
            yield return null;
        }

        if (_granted)
            yield break;
        
        if (_skin.Prefab != null || _skin.PrefabReference.RuntimeKeyIsValid())
        {
            _granted = true; 
            _collider.enabled = false; 
            
            _ = _playerSkin.ApplyCharacterSkinAsync(_skin);
            
            string id = _skin.name;
            
            _skinSaver.AddPurchased(id);
            _skinSaver.SetSelected(id);
            
            Advertising.ShowNow();
        }

        gameObject.SetActive(false);
        _progressRoutine = null;
    }
}