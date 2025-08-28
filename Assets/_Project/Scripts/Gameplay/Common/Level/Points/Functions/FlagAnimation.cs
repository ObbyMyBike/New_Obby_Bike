using System.Collections;
using UnityEngine;

public class FlagAnimation
{
    private readonly Transform _transform;
    private readonly Renderer _renderer;
    private readonly Color _startColor;
    private readonly Color _endColor;
    private readonly float _rotateSpeed;

    public FlagAnimation(Transform flagTransform, Renderer targetRenderer, float rotateSpeed, Color endColor)
    {
        _transform = flagTransform;
        _renderer = targetRenderer;
        _rotateSpeed = rotateSpeed;
        _endColor = endColor;
        _startColor = targetRenderer != null ? targetRenderer.material.color : Color.white;
    }

    public IEnumerator Animate()
    {
        float rotatedAngle = 0f;

        while (rotatedAngle < 360f)
        {
            float delta = _rotateSpeed * Time.deltaTime;
            float step = Mathf.Min(delta, 360f - rotatedAngle);
            
            _transform.Rotate(Vector3.up, step, Space.World);
            
            rotatedAngle += step;

            float t = Mathf.Clamp01(rotatedAngle / 360f);
            
            if (_renderer != null)
                _renderer.material.color = Color.Lerp(_startColor, _endColor, t);

            yield return null;
        }

        if (_renderer != null)
            _renderer.material.color = _endColor;
    }
}