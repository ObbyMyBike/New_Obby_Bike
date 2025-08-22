using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoostTimerUI : MonoBehaviour
{
    [Header("Speed Boost UI")]
    [SerializeField] private GameObject _speedBoostPanel;
    [SerializeField] private TextMeshProUGUI _speedTimerText;
    [SerializeField] private Image _speedFillImage;

    [Header("Jump Boost UI")]
    [SerializeField] private GameObject _jumpBoostPanel;
    [SerializeField] private TextMeshProUGUI _jumpTimerText;
    [SerializeField] private Image _jumpFillImage;

    private PlayerCharacterRoot _playerCharacterRoot;
    private Coroutine _speedCoroutine;
    private Coroutine _jumpCoroutine;

    private void Awake()
    {
        SetSpeedUIActive(false);
        SetJumpUIActive(false);
    }

    private void OnDestroy()
    {
        if (_playerCharacterRoot != null)
            Unsubscribe(_playerCharacterRoot);
    }

    public void SetPlayerController(PlayerCharacterRoot playerCharacterRoot)
    {
        if (_playerCharacterRoot != null)
            Unsubscribe(_playerCharacterRoot);

        _playerCharacterRoot = playerCharacterRoot;

        if (_playerCharacterRoot != null)
            Subscribe(_playerCharacterRoot);
    }
    
    private void Subscribe(PlayerCharacterRoot playerCharacterRoot)
    {
        playerCharacterRoot.SpeedBoostStarted += OnSpeedBoostStarted;
        playerCharacterRoot.SpeedBoostEnded += OnSpeedBoostEnded;
        playerCharacterRoot.JumpBoostStarted += OnJumpBoostStarted;
        playerCharacterRoot.JumpBoostEnded += OnJumpBoostEnded;
    }
    
    private void Unsubscribe(PlayerCharacterRoot playerCharacterRoot)
    {
        if (playerCharacterRoot == null)
            return;
        
        playerCharacterRoot.SpeedBoostStarted -= OnSpeedBoostStarted;
        playerCharacterRoot.SpeedBoostEnded -= OnSpeedBoostEnded;
        playerCharacterRoot.JumpBoostStarted -= OnJumpBoostStarted;
        playerCharacterRoot.JumpBoostEnded -= OnJumpBoostEnded;
    }

    private void OnSpeedBoostStarted(float duration)
    {
        if(_speedCoroutine != null)
            StopCoroutine(_speedCoroutine);

        _speedCoroutine = StartCoroutine(CountdownRoutine(duration, _speedTimerText, _speedFillImage, SetSpeedUIActive));
    }
    
    private void OnSpeedBoostEnded()
    {
        if(_speedCoroutine != null)
            StopCoroutine(_speedCoroutine);
        
        SetSpeedUIActive(false);
    }
    
    private void OnJumpBoostStarted(float duration)
    {
        if(_jumpCoroutine != null)
            StopCoroutine(_jumpCoroutine);

        _jumpCoroutine = StartCoroutine(CountdownRoutine(duration, _jumpTimerText, _jumpFillImage, SetJumpUIActive));
    }

    private void OnJumpBoostEnded()
    {
        if(_jumpCoroutine != null)
            StopCoroutine(_jumpCoroutine);
        
        SetJumpUIActive(false);
    }

    private IEnumerator CountdownRoutine(float totalDuration, TextMeshProUGUI text, Image fillImage, Action<bool> setActive)
    {
        setActive(true);
        float remaining = totalDuration;

        while (remaining > 0f)
        {
            UpdateUI(remaining, totalDuration, text, fillImage);

            yield return null;
            
            remaining -= Time.deltaTime;
        }
        
        UpdateUI(0f, totalDuration, text, fillImage);

        yield return new WaitForSeconds(0.1f);
        
        setActive(false);
    }

    private void UpdateUI(float remaining, float total, TextMeshProUGUI text, Image fillImage)
    {
        if (text != null)
            text.text = FormatTime(remaining);
        
        if(fillImage != null)
            fillImage.fillAmount = Mathf.Clamp01(remaining / total);
    }

    private string FormatTime(float time)
    {
        if (time >= 60f)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            return $"{minutes:00}:{seconds:00}";
        }
        else
        {
            return time.ToString("F1") + "s";
        }
    }

    private void SetSpeedUIActive(bool active)
    {
        if(_speedBoostPanel != null)
            _speedBoostPanel.SetActive(active);
    }

    private void SetJumpUIActive(bool active)
    {
        if (_jumpBoostPanel != null)
            _jumpBoostPanel.SetActive(active);
    }
}