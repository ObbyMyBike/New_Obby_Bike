using System;
using System.Collections;
using UnityEngine;

public class BikeProgressManager
{
    private const float MAX_PROGRESS_PERCENT = 100f;
    
    public event Action<float, Sprite> ProgressUpdated;
    public event Action BikeUnlocked;
    public event Action ProgressReseted;

    private readonly BikeSkinData[] bikeSkins;
    private readonly MonoBehaviour runner;
    private readonly int checkpointsPerStep;
    private readonly float progressStep;

    private float _currentProgress;
    private int _checkpointCount;
    private int _currentSkinIndex;
    private bool _isFirstUnlock = true;


    public BikeProgressManager(int checkpointsPerStep, float progressStep, BikeSkinData[] bikeSkins, MonoBehaviour runner)
    {
        this.checkpointsPerStep = Mathf.Max(1, checkpointsPerStep);
        this.progressStep =  Mathf.Max(0f, progressStep);
        this.bikeSkins = bikeSkins ?? Array.Empty<BikeSkinData>();
        this.runner = runner;
    }

    public void CheckpointReached()
    {
        _checkpointCount++;
        
        if (_checkpointCount % checkpointsPerStep != 0)
            return;

        _currentProgress += progressStep;

        float clamped = Mathf.Min(_currentProgress, MAX_PROGRESS_PERCENT);
        
        Sprite uiSprite = null;
        
        if (bikeSkins.Length > 0)
        {
            int nextIndex = _isFirstUnlock ? 0 : Mathf.Min(_currentSkinIndex + 1, bikeSkins.Length - 1);
            BikeSkinData data = bikeSkins[nextIndex];
            
            if (data != null)
                uiSprite = data.BikeUISprite;
        }

        ProgressUpdated?.Invoke(clamped, uiSprite);

        if (clamped >= MAX_PROGRESS_PERCENT)
            Unlock();
    }

    private void Unlock()
    {
        BikeUnlocked?.Invoke();

        if (bikeSkins.Length > 0)
            if (!_isFirstUnlock)
                _currentSkinIndex = (_currentSkinIndex + 1) % bikeSkins.Length;

        _isFirstUnlock = false;
        
        if (runner != null)
            runner.StartCoroutine(DelayedReset());
        else
            Reset();
    }

    private void Reset()
    {
        _currentProgress = 0f;
        _checkpointCount = 0;

        ProgressReseted?.Invoke();
    }
    
    private IEnumerator DelayedReset()
    {
        yield return new WaitForSeconds(3f);

        Reset();
    }
}