using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Runtime.InteropServices;

public class Advertising : MonoBehaviour
{
    [DllImport("__Internal")]
    
    private static extern void ShowFullscreen();
    public static bool IsReclama;

    [SerializeField] private Text _waitText;

    private void Start()
    {
        StartCoroutine(TryLaunch());
    }
    
    public static void ShowNow()
    {
        IsReclama = true;
        
#if UNITY_WEBGL && !UNITY_EDITOR
        ShowFullscreen();
#else
        Debug.Log("[Advertising] ShowNow() called (stub outside WebGL).");
#endif
    }
    
    private IEnumerator TryLaunch()
    {
        yield return new WaitForSeconds(60f);
        
        _waitText.gameObject.SetActive(true);
        _waitText.text = "Реклама через 2...";
        
        yield return new WaitForSeconds(1f);
        
        _waitText.text = "Реклама через 1...";
        
        yield return new WaitForSeconds(1f);
        
        _waitText.gameObject.SetActive(false);
        
        ShowNow();
        
        StartCoroutine(TryLaunch());
    }
}