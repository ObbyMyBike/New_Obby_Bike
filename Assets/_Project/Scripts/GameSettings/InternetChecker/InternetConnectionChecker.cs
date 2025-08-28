using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Zenject;

public class InternetConnectionChecker : IInternetConnectionChecker, IInitializable, IDisposable
{
    private const string LocalWebGlPingRelative = "StreamingAssets/net-ping.txt";
    
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
        
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            list.Add(LocalWebGlPingRelative);
            
            string nearIndex = BuildNearIndexUrl("StreamingAssets/net-ping.txt");
            
            if (!string.IsNullOrEmpty(nearIndex))
                list.Add(nearIndex);

            return list;
        }
        
        string sameOrigin = BuildSameOriginUrl();
        
        if (!string.IsNullOrEmpty(sameOrigin))
            list.Add(sameOrigin);

        list.AddRange(FallbackProbeUrls);
        
        return list;
    }

    private string BuildNearIndexUrl(string relative)
    {
        string abs = Application.absoluteURL;
        if (string.IsNullOrEmpty(abs))
            return null;

        try
        {
            var uri = new Uri(abs);
            var basePath = uri.AbsolutePath;
            int lastSlash = basePath.LastIndexOf('/');
            
            if (lastSlash >= 0)
                basePath = basePath.Substring(0, lastSlash + 1);

            var origin = uri.GetLeftPart(UriPartial.Authority);
            
            return origin + basePath + relative;
        }
        catch
        {
            return null;
        }
    }
    
    private string BuildSameOriginUrl()
    {
        string abs = Application.absoluteURL;
        
        if (string.IsNullOrEmpty(abs))
            return null;
        try
        {
            return BuildNearIndexUrl("StreamingAssets/net-ping.txt");
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
                req.SetRequestHeader("Cache-Control", "no-cache, no-store, must-revalidate");
                req.SetRequestHeader("Pragma", "no-cache");
                req.SetRequestHeader("Expires", "0");

                req.timeout = 3;
                
                yield return req.SendWebRequest();
                
                bool hasHttpResponse = req.responseCode != 0;
                bool hasFatalError = req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.DataProcessingError;

                if (hasHttpResponse && !hasFatalError)
                {
                    ok = true;
                    
                    break;
                }
                
                if (req.responseCode != 0)
                {
                    ok = true;
                    
                    break;
                }
            }
        }

        UpdateStatus(ok);
    }
}