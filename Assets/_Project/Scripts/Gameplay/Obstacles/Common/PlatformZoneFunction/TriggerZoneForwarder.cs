using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider))]
public class TriggerZoneForwarder : MonoBehaviour
{
    private const string BOT_TAG = "Bot";
    
    [Header("Targets")]
    [SerializeField] private OrbitActivationHub _explicitHub; 
    [SerializeField] private MovingPlatformTriggered _platformTrigger;
    [SerializeField] private float _minHoldOnFirstEnter = 1.0f;
    [SerializeField] private float _deactivateDelay = 0.4f;
    
    [Header("Who should activate")]
    [SerializeField] private bool _reactToPlayer = true;
    [SerializeField] private bool _reactToBots = true;
    [SerializeField] private bool _useBotTagCheck = true;
    [SerializeField] private bool _stopImmediatelyOnExit = true;
        
    [Header("Tween settings")]
    [SerializeField] private Transform _tweenTarget;
    [SerializeField] private Ease _ease = Ease.OutCubic;
    [SerializeField] private float _downOffsetY = 0.2f;
    [SerializeField] private float _duration = 0.25f;
    [SerializeField] private bool _useLocal = true;
    [SerializeField] private bool _returnOnExit = true;
    
    private AgentClassifier _agentClassifier;
    private InsideSet _inside;
    private ExitDelayPolicy _exitPolicy;
    private OrbitPlatformActivator _activator;
    private ButtonTweenAnimator _tween;
    
    private OrbitActivationHub _orbitHub;
    private Coroutine _pendingDeactivate;
    
    private void Awake()
    {
        _orbitHub = _explicitHub;

        if (_orbitHub == null)
        {
            SimpleOrbitMovement orbit = GetComponentInParent<SimpleOrbitMovement>();
            
            if (orbit != null)
            {
                _orbitHub = orbit.GetComponent<OrbitActivationHub>();
                
                if (_orbitHub == null)
                    _orbitHub = orbit.gameObject.AddComponent<OrbitActivationHub>();
            }
        }

        if (_orbitHub == null)
        {
            enabled = false;
            
            return;
        }
        
        _agentClassifier = new AgentClassifier(_reactToPlayer, _reactToBots, _useBotTagCheck);
        _inside = new InsideSet();
        _exitPolicy = new ExitDelayPolicy(_minHoldOnFirstEnter, _deactivateDelay);
        _activator = new OrbitPlatformActivator(_orbitHub, _platformTrigger);
        _tween = new ButtonTweenAnimator(_tweenTarget, _useLocal, _ease, _downOffsetY, _duration);

        _tween.CaptureStart();
    }
    
    private void OnEnable()
    {
        _tween?.CaptureStart();
    }

    private void OnDisable()
    {
        CancelPendingDeactivate();

        if (_inside != null && _inside.Count > 0)
            _activator?.DeactivateLastUser();

        _tween?.Kill();
        _tween?.ResetToStart();
        _inside?.Clear();
        _exitPolicy?.ResetFirstEnter();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (_agentClassifier == null || !_agentClassifier.TryGetAgentRoot(other, out var agentRoot))
            return;

        if (_inside.Add(agentRoot))
        {
            CancelPendingDeactivate();

            if (_inside.Count == 1)
            {
                _exitPolicy.MarkFirstEnter();
                _activator.ActivateFirstUser();
                _tween.PressDown();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_agentClassifier == null || !_agentClassifier.TryGetAgentRoot(other, out var agentRoot))
            return;

        if (_inside.Remove(agentRoot) && _inside.Count == 0)
        {
            CancelPendingDeactivate();

            float delay = _exitPolicy.ComputeDelay(_stopImmediatelyOnExit);
            
            if (delay <= 0f)
                DeactivateNow();
            else
                _pendingDeactivate = StartCoroutine(DeactivateAfter(delay));
        }
    }
    
    private bool TryGetAgentRoot(Collider other, out Component agentRoot)
    {
        agentRoot = null;

        if (_reactToPlayer)
        {
            Player player = other.GetComponentInParent<Player>();

            if (player != null)
            {
                agentRoot = player;
                
                return true;
            }

            if (other.CompareTag("Player"))
            {
                agentRoot = other.attachedRigidbody != null ? other.attachedRigidbody : other.transform.root;
                
                return true;
            }
        }

        if (_reactToBots)
        {
            var bot = other.GetComponentInParent<BotDriver>();
            
            if (bot != null)
            {
                agentRoot = bot;
                
                return true;
            }

            if (_useBotTagCheck && other.CompareTag(BOT_TAG))
            {
                agentRoot = other.attachedRigidbody != null ? other.attachedRigidbody : other.transform.root;
                
                return true;
            }
        }

        return false;
    }
    
    private void DeactivateNow()
    {
        _activator.DeactivateLastUser();
        _tween.Release(_returnOnExit);
        _exitPolicy.ResetFirstEnter();
    }
    
    private void CancelPendingDeactivate()
    {
        if (_pendingDeactivate != null)
        {
            StopCoroutine(_pendingDeactivate);
            
            _pendingDeactivate = null;
        }
    }
    
    private void Reset()
    {
        if (_tweenTarget == null)
            _tweenTarget = transform;
    }
    
    private IEnumerator DeactivateAfter(float delay)
    {
        float time = 0f;
        
        while (time < delay)
        {
            if (_inside.Count > 0)
            {
                _pendingDeactivate = null;
                
                yield break;
            }
            
            time += Time.deltaTime;
            
            yield return null;
        }

        if (_inside.Count == 0)
            DeactivateNow();

        _pendingDeactivate = null;
    }
}