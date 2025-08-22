using UnityEngine;
using UnityEngine.UI;

public class MusicSettings : MonoBehaviour
{
    private const string PREF_KEY = "MusicVolume";
    
    [SerializeField] private Slider _musicSlider;
    
    private AudioSource _musicSource;

    private void Awake()
    {
        _musicSource = GetComponent<AudioSource>();
        _musicSource.ignoreListenerPause = true;
    }

    private void Start()
    {
        float saved = PlayerPrefs.GetFloat(PREF_KEY, 1f);
        
        _musicSlider.value = saved;
        _musicSource.volume = saved;
        
        _musicSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    private void OnDestroy()
    {
        _musicSlider.onValueChanged.RemoveListener(OnVolumeChanged);
    }
    
    private void OnVolumeChanged(float value)
    {
        _musicSource.volume = value;
        
        PlayerPrefs.SetFloat(PREF_KEY, value);
        PlayerPrefs.Save();
    }
}