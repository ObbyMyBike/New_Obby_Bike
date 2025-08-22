using UnityEngine;

public class PlatformCrossActivator : MonoBehaviour
{
    [SerializeField] private PlatformActivationHub _sourceHub;
    [SerializeField] private MovingPlatformTriggered _otherPlatform;
    [SerializeField] private bool _stopOtherOnExit = false;
    
    private void Reset()
    {
        if (_sourceHub == null)
            _sourceHub = GetComponentInParent<PlatformActivationHub>();
    }

    private void OnEnable()
    {
        if (_sourceHub == null)
            _sourceHub = GetComponentInParent<PlatformActivationHub>();

        if (_sourceHub != null)
        {
            _sourceHub.FirstUserEntered += OnFirstUser;
            _sourceHub.LastUserExited  += OnLastUser;
        }
    }

    private void OnDisable()
    {
        if (_sourceHub != null)
        {
            _sourceHub.FirstUserEntered -= OnFirstUser;
            _sourceHub.LastUserExited  -= OnLastUser;
        }
    }

    private void OnFirstUser()
    {
        if (_otherPlatform != null)
            _otherPlatform.SetPlayerInside(true);
    }

    private void OnLastUser()
    {
        if (_stopOtherOnExit && _otherPlatform != null)
            _otherPlatform.SetPlayerInside(false);
    }
}