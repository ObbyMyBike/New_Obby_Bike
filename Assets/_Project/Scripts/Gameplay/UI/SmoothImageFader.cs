using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

[RequireComponent(typeof(Image))]
public class SmoothImageFader : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private UnityEvent OnEndAnim;
    
    private Image image;
    private Coroutine fadeCoroutine;
    private float initialAlpha;

    private void Awake()
    {
        image = GetComponent<Image>();
        initialAlpha = image.color.a;
        
        if (Mathf.Approximately(initialAlpha, 0f))
        {
            initialAlpha = 1f;
        }
    }

    private void OnEnable()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
        
        fadeCoroutine = StartCoroutine(FadeIn());
    }
    
    public void StartFadeOut()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            
            fadeCoroutine = null;
        }
        
        fadeCoroutine = StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        Color currentColor = image.color;
        currentColor.a = 0f;
        image.color = currentColor;

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);
            float alpha = Mathf.Lerp(0f, initialAlpha, t);
            image.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
            
            yield return null;
        }
        
        image.color = new Color(currentColor.r, currentColor.g, currentColor.b, initialAlpha);
    }

    private IEnumerator FadeOut()
    {
        Color currentColor = image.color;
        float startAlpha = currentColor.a;

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);
            float alpha = Mathf.Lerp(startAlpha, 0f, t);
            image.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
            
            yield return null;
        }
        
        image.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0f);
    }
}