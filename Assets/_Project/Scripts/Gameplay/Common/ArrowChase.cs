using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class ArrowChase : MonoBehaviour
{
    [Header("Segments (order = chase order)")]
    [SerializeField] private Renderer[] _segments;
    
    [Header("Emission")]
    [SerializeField, ColorUsage(true, true)] private Color _baseEmission = new Color(1, 0.2f, 0.2f, 1) * 0.2f;
    [SerializeField, ColorUsage(true, true)] private Color _glowEmission = new Color(1, 0.2f, 0.2f, 1) * 5f;
    
    [Header("Timing")]
    [SerializeField] private float _stepDelay = 0.08f;
    [SerializeField] private float _onTime = 0.20f;
    [SerializeField] private float _fadeIn = 0.06f;
    [SerializeField] private float _fadeOut = 0.14f;
    
    [Header("Play")]
    [SerializeField] private bool _playOnAwake = true;
    [SerializeField] private bool _loop = true;
    [SerializeField] private bool _reverse = false;
    
    private readonly int emissionID = Shader.PropertyToID("_EmissionColor");
    
    private MaterialPropertyBlock[] _materialProperty;
    private Coroutine _loopRoutine;

    private void Awake()
    {
        if (_segments == null || _segments.Length == 0)
            _segments = GetComponentsInChildren<Renderer>();

        _materialProperty = new MaterialPropertyBlock[_segments.Length];

        for (int i = 0; i < _segments.Length; i++)
        {
            if (_segments[i] == null)
                continue;

            _materialProperty[i] = new MaterialPropertyBlock();
            _materialProperty[i].SetColor(emissionID, _baseEmission);
            _segments[i].SetPropertyBlock(_materialProperty[i]);
            
            Material material = _segments[i].sharedMaterial;
            
            if (material != null)
                material.EnableKeyword("_EMISSION");
        }
    }

    private void OnEnable()
    {
        if (_playOnAwake)
            Play();
    }

    private void OnDisable()
    {
        Stop();
        
        for (int i = 0; i < _segments.Length; i++)
        {
            if (_segments[i] == null)
                continue;
            
            _materialProperty[i].SetColor(emissionID, _baseEmission);
            _segments[i].SetPropertyBlock(_materialProperty[i]);
        }
    }

    private void Play()
    {
        if (_loopRoutine != null)
            return;
        
        _loopRoutine = StartCoroutine(Loop());
    }

    private void Stop()
    {
        if (_loopRoutine != null)
        {
            StopCoroutine(_loopRoutine);
            
            _loopRoutine = null;
        }
    }

    private void Reset()
    {
        _segments = GetComponentsInChildren<Renderer>();
    }
    
    private IEnumerator Loop()
    {
        if (_segments == null || _segments.Length == 0)
            yield break;

        int index = _reverse ? _segments.Length - 1 : 0;
        int direction = _reverse ? -1 : 1;

        while (true)
        {
            if (_segments[index] != null)
                StartCoroutine(GlowSegment(index));

            index += direction;

            if (index >= _segments.Length || index < 0)
            {
                if (!_loop)
                    yield break;
                
                index = _reverse ? _segments.Length - 1 : 0;
            }

            yield return new WaitForSeconds(_stepDelay);
        }
    }

    private IEnumerator GlowSegment(int i)
    {
        Renderer rendererSegment = _segments[i];
        
        if (rendererSegment == null)
            yield break;

        MaterialPropertyBlock properties = _materialProperty[i];
        float time = 0f;
        
        while (time < _fadeIn && _fadeIn > 0f)
        {
            time += Time.deltaTime;
            
            float clamp01 = Mathf.Clamp01(time / _fadeIn);
            
            properties.SetColor(emissionID, Color.Lerp(_baseEmission, _glowEmission, clamp01));
            rendererSegment.SetPropertyBlock(properties);
            
            yield return null;
        }
        
        float hold = Mathf.Max(0f, _onTime - _fadeIn - _fadeOut);
        
        if (hold > 0f)
        {
            properties.SetColor(emissionID, _glowEmission);
            rendererSegment.SetPropertyBlock(properties);
            
            yield return new WaitForSeconds(hold);
        }
        
        time = 0f;
        
        while (time < _fadeOut && _fadeOut > 0f)
        {
            time += Time.deltaTime;
            
            float clamp01 = Mathf.Clamp01(time / _fadeOut);
            
            properties.SetColor(emissionID, Color.Lerp(_glowEmission, _baseEmission, clamp01));
            rendererSegment.SetPropertyBlock(properties);
            
            yield return null;
        }

        properties.SetColor(emissionID, _baseEmission);
        rendererSegment.SetPropertyBlock(properties);
    }
}