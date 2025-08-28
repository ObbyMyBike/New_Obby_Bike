using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Zenject;

public class InternetConnectionChecker : IInternetConnectionChecker, IInitializable, IDisposable
{
    private static readonly string[] FallbackProbeUrls =
    {
        "https://www.google.com/generate_204",
        "https://httpstat.us/204",
        "https://ya.ru/generate_204"
    };
    
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

    void IInitializable.Initialize() => _checkRoutine = _coroutineRunner.StartCoroutine(CheckLoop());

    void IDisposable.Dispose()
    {
        if (_checkRoutine != null)
        {
            MonoBehaviour monoBehaviour = _coroutineRunner as MonoBehaviour;
            
            if (monoBehaviour)
                monoBehaviour.StopCoroutine(_checkRoutine);
            
            _checkRoutine = null;
        }
    }
    
    public void ForceCheckNow() => _coroutineRunner.StartCoroutine(CheckInternetConnectionAsync());
    
    private void UpdateStatus(bool connectedNow)
    {
        if (connectedNow == _isConnected)
            return;

        _isConnected = connectedNow;

        if (_isConnected)
            Connected?.Invoke();
        else
            ConnectionLost?.Invoke();
    }

    private List<string> BuildProbeList()
    {
        var list = new List<string>(4);
        
        string self = BuildSameOriginUrl();
        
        if (!string.IsNullOrEmpty(self))
            list.Add(self);
        
        list.AddRange(FallbackProbeUrls);
        
        return list;
    }

    private string BuildSameOriginUrl()
    {
        string abs = Application.absoluteURL;
        
        if (string.IsNullOrEmpty(abs))
            return null;
        try
        {
            var uri = new Uri(abs);
            string origin = uri.GetLeftPart(UriPartial.Authority) + "/";
            
            return origin;
        }
        catch
        {
            return null;
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
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            UpdateStatus(false);
            yield break;
        }
        
        List<string> urls = BuildProbeList();

        bool ok = false;

        foreach (var url in urls)
        {
            using (UnityWebRequest req = UnityWebRequest.Get(url))
            {
                req.timeout = 3;
                yield return req.SendWebRequest();
                
                bool success = req.result == UnityWebRequest.Result.Success;
                
                if (!success)
                {
                    long code = req.responseCode;
                    
                    if (code >= 200 && code < 400)
                        success = true;
                }

                if (success)
                {
                    ok = true;
                    
                    break;
                }
            }
        }

        UpdateStatus(ok);
    }
}