using System.Collections;
using UnityEngine;

public class MarkerController
{
    private readonly MonoBehaviour coroutineRunner;
    private readonly RectTransform markerRect;
    private readonly HorizontalBarLayout layout;
    private readonly float animationDurationSeconds;

    private Coroutine _runningAnimation;

    public MarkerController(RectTransform markerRect, HorizontalBarLayout layout, float animationDurationSeconds, MonoBehaviour coroutineRunner)
    {
        this.markerRect = markerRect;
        this.layout = layout;
        this.animationDurationSeconds = Mathf.Max(0.01f, animationDurationSeconds);
        this.coroutineRunner = coroutineRunner;
    }

    public void Stop()
    {
        if (_runningAnimation != null)
        {
            coroutineRunner?.StopCoroutine(_runningAnimation);
            _runningAnimation = null;
        }
    }
    
    public void SetInstant(float normalizedProgress)
    {
        if (markerRect == null)
            return;
        
        float x = layout.EvaluateX(normalizedProgress);
        
        Vector2 position = markerRect.anchoredPosition;
        position.x = x;
        markerRect.anchoredPosition = position;
    }

    public void AnimateTo(float normalizedProgress)
    {
        if (markerRect == null)
            return;

        float targetX = layout.EvaluateX(normalizedProgress);
        
        Stop();
        
        _runningAnimation = coroutineRunner.StartCoroutine(AnimateX(targetX));
    }

    private IEnumerator AnimateX(float targetX)
    {
        if (markerRect == null)
        {
            _runningAnimation = null;
            
            yield break;
        }
        
        float elapsed = 0f;
        float startX = markerRect != null ? markerRect.anchoredPosition.x : 0f;

        while (elapsed < animationDurationSeconds)
        {
            if (markerRect == null)
            {
                _runningAnimation = null;
                
                yield break;
            }
            
            elapsed += Time.deltaTime;
            
            float time = Mathf.Clamp01(elapsed / animationDurationSeconds);

            Vector2 markerPosition = markerRect.anchoredPosition;
            markerPosition.x = Mathf.Lerp(startX, targetX, time);
            markerRect.anchoredPosition = markerPosition;

            yield return null;
        }

        if (markerRect != null)
        {
            Vector2 finalMarkerPosition = markerRect.anchoredPosition;
            finalMarkerPosition.x = targetX;
            markerRect.anchoredPosition = finalMarkerPosition;
        }

        _runningAnimation = null;
    }
}