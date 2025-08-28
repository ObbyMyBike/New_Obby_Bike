using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class InternetConnectionUI : MonoBehaviour
{
    [SerializeField] private GameObject _offlineImageRoot;
    [SerializeField] private Button _retryButton;
    [SerializeField] private float _retryCooldown = 0.2f;

    private IInternetConnectionChecker _checker;
    private ICoroutineRunner _coroutineRunner;

    [Inject]
    public void Construct(IInternetConnectionChecker checker, ICoroutineRunner coroutineRunner)
    {
        _checker = checker;
        _coroutineRunner = coroutineRunner;
    }

    private void Start()
    {
        if (_offlineImageRoot != null)
            _offlineImageRoot.SetActive(_checker != null && !_checker.IsConnected);
    }

    private void OnEnable()
    {
        if (_checker != null)
        {
            _checker.ConnectionLost += HandleLost;
            _checker.Connected += HandleRestored;
        }
        
        if (_retryButton != null)
            _retryButton.onClick.AddListener(OnRetryClicked);
    }
    
    private void OnDisable()
    {
        if (_checker != null)
        {
            _checker.ConnectionLost -= HandleLost;
            _checker.Connected -= HandleRestored;
        }
        
        if (_retryButton != null)
            _retryButton.onClick.RemoveListener(OnRetryClicked);
    }

    private void HandleLost() => _offlineImageRoot?.SetActive(true);

    private void HandleRestored() => _offlineImageRoot?.SetActive(false);
    
    private void OnRetryClicked()
    {
        if (_checker == null)
            return;

        _checker.ForceCheckNow();

        if (_retryButton != null)
        {
            _retryButton.interactable = false;
            
            if (_coroutineRunner != null)
                _coroutineRunner.StartCoroutine(ReenableRetryButtonAfterDelay());
            else
                StartCoroutine(ReenableRetryButtonAfterDelay());
        }
    }

    private IEnumerator ReenableRetryButtonAfterDelay()
    {
        yield return new WaitForSecondsRealtime(_retryCooldown);
        
        if (_retryButton != null)
            _retryButton.interactable = true;
    }
}