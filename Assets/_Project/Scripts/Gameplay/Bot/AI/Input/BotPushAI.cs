using UnityEngine;
using Zenject;

public class BotPushAI : MonoBehaviour
{
    [Header("Push Settings")]
    [SerializeField, Min(0f)] private float _pushRadius = 2f;
    [SerializeField, Min(0f)] private float _pushForce = 10f;
    [SerializeField, Min(0f)] private float _pushDuration = 1f;
    [SerializeField, Min(0f)] private float _cooldown = 1.2f;
    [SerializeField, Range(0f, 1f)] private float _chancePerAttempt = 0.2f;

    private Transform _botTransform;
    private BotDriver _botDriver;
    private PlayerCharacterRoot _player;
    private float _nextTime;

    [Inject]
    private void Construct(Player player)
    {
        _player = player != null ? player.PlayerCharacterRoot : null;
    }

    private void Awake()
    {
        _botTransform = transform;
        _botDriver = GetComponentInChildren<BotDriver>();
        _nextTime = Time.time + Random.Range(0.2f, 0.6f);
    }

    private void Update()
    {
        if (_player == null || _botDriver == null)
            return;

        if (Time.time < _nextTime)
            return;

        _nextTime = Time.time + _cooldown;

        Vector3 botPosition = _botTransform.position;
        Vector3 playerPosition = _player.transform.position;
        Vector3 positionToPlayer = playerPosition - botPosition;
        positionToPlayer.y = 0f;

        if (positionToPlayer.sqrMagnitude > _pushRadius * _pushRadius)
            return;

        if (Random.value > _chancePerAttempt)
            return;

        if (positionToPlayer.sqrMagnitude < 1e-6f)
            return;

        Vector3 direction = positionToPlayer.normalized;
        
        _player.TryApplyPush(direction * _pushForce, _pushDuration);
        _botDriver.ApplyPush(-direction * _pushForce, _pushDuration);
    }
}