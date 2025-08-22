using UnityEngine;
using System;

public class BotProgress : MonoBehaviour
{
    public event Action<float> ProgressUpdated;

    private RacePath _path;
    private Vector3 _startPosition;
    private Vector3 _finishPosition;
    private float _totalDistance;
    private bool _usePath;

    public void Initialize(Vector3 startPosition, Vector3 finishPosition)
    {
        _path = null;
        _usePath = false;
        
        _startPosition = startPosition;
        _finishPosition = finishPosition;
        _totalDistance = Vector3.Distance(startPosition, finishPosition);
        
        UpdateProgress();
    }

    public void InitializePath(RacePath path)
    {
        _path = path;
        _usePath = _path != null && _path.IsValid;
        
        UpdateProgress();
    }
    
    public void UpdateProgressFromPosition(Vector3 currentPosition)
    {
        float progress = 0f;

        if (_usePath && _path != null && _path.IsValid)
        {
            progress = _path.ComputeProgress(currentPosition);
        }
        else
        {
            float currentDistance = Vector3.Distance(currentPosition, _finishPosition);
            
            progress = _totalDistance > 0f ? (1f - currentDistance / _totalDistance) : 0f;
            progress = Mathf.Clamp01(progress);
        }
        
        ProgressUpdated?.Invoke(progress);
    }

    private void UpdateProgress()
    {
        UpdateProgressFromPosition(transform.position);
    }
}