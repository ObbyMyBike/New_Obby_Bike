using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Zenject;

public class InternetConnectionChecker : IInternetConnectionChecker, IInitializable
{
    private const string CHECK_URL = "https://www.google.com";
    
    public event Action ConnectionLost;
    public event Action Connected;
    
    private readonly ICoroutineRunner _coroutineRunner;
    private readonly float _checkInterval;
    
    private Coroutine _checkRoutine;
    private bool _isConnected = true;

    public bool IsConnected => _isConnected;

    public InternetConnectionChecker(ICoroutineRunner coroutineRunner, float checkInterval)
    {
        _coroutineRunner = coroutineRunner;
        _checkInterval = Mathf.Max(1f, checkInterval);
    }

    public void Initialize()
    {
        _checkRoutine = _coroutineRunner.StartCoroutine(CheckLoop());
    }

    public void ForceCheckNow()
    {
        _coroutineRunner.StartCoroutine(CheckInternetConnectionAsync());
    }

    public void Dispose()
    {
        if (_checkRoutine != null)
        {
            MonoBehaviour monoBehaviour = _coroutineRunner as MonoBehaviour;
            
            if (monoBehaviour)
                monoBehaviour.StopCoroutine(_checkRoutine);
            
            _checkRoutine = null;
        }
    }
    
    private IEnumerator CheckLoop()
    {
        while (true)
        {
            yield return CheckInternetConnectionAsync();
            
            yield return new WaitForSecondsRealtime(_checkInterval);
        }
    }

    private IEnumerator CheckInternetConnectionAsync()
    {
        using (UnityWebRequest request = UnityWebRequest.Head(CHECK_URL))
        {
            request.timeout = 2;
#if UNITY_2020_1_OR_NEWER
            yield return request.SendWebRequest();
#else
            yield return request.Send();
#endif

            bool currentConnectionStatus = false;
#if UNITY_2020_1_OR_NEWER
            currentConnectionStatus = request.result == UnityWebRequest.Result.Success;
#else
            currentConnectionStatus = !request.isNetworkError && !request.isHttpError;
#endif

            if (currentConnectionStatus != _isConnected)
            {
                _isConnected = currentConnectionStatus;

                if (_isConnected)
                    Connected?.Invoke();
                else
                    ConnectionLost?.Invoke();
            }
        }
    }
}