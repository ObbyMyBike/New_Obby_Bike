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

    public void SetInstant(float normalizedProgress)
    {
        float x = layout.EvaluateX(normalizedProgress);
        
        Vector2 position = markerRect.anchoredPosition;
        position.x = x;
        markerRect.anchoredPosition = position;
    }

    public void AnimateTo(float normalizedProgress)
    {
        float targetX = layout.EvaluateX(normalizedProgress);

        if (_runningAnimation != null)
            coroutineRunner.StopCoroutine(_runningAnimation);

        _runningAnimation = coroutineRunner.StartCoroutine(AnimateX(targetX));
    }

    private IEnumerator AnimateX(float targetX)
    {
        float elapsed = 0f;
        float startX = markerRect.anchoredPosition.x;

        while (elapsed < animationDurationSeconds)
        {
            elapsed += Time.deltaTime;
            
            float t = Mathf.Clamp01(elapsed / animationDurationSeconds);

            Vector2 markerPosition = markerRect.anchoredPosition;
            markerPosition.x = Mathf.Lerp(startX, targetX, t);
            markerRect.anchoredPosition = markerPosition;

            yield return null;
        }

        Vector2 finalMarkerPosition = markerRect.anchoredPosition;
        finalMarkerPosition.x = targetX;
        markerRect.anchoredPosition = finalMarkerPosition;

        _runningAnimation = null;
    }
}