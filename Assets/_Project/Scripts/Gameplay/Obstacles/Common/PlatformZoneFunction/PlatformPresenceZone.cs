using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlatformPresenceZone : MonoBehaviour
{
    private const string PLAYER_TAG = "Player";
    private const string BOT_TAG = "Bot";
    
    [SerializeField] private PlatformActivationHub _hub;
    [SerializeField] private bool _reactToPlayer = true;
    [SerializeField] private bool _reactToBots = true;
    [SerializeField] private bool _useBotTagCheck = true;
    
    private readonly HashSet<int> inside = new HashSet<int>();
    
    private Collider _collider;

    private void Reset()
    {
        _collider = GetComponent<Collider>();
        _collider.isTrigger = true;
        
        if (_hub == null)
            _hub = GetComponentInParent<PlatformActivationHub>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!TryGetAgentRoot(other, out var root))
            return;
        
        if (inside.Add(root.GetInstanceID()) && inside.Count == 1)
            _hub?.AddUser();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!TryGetAgentRoot(other, out var root))
            return;
        
        if (inside.Remove(root.GetInstanceID()) && inside.Count == 0)
            _hub?.RemoveUser();
    }

    private bool TryGetAgentRoot(Collider other, out Component root)
    {
        root = null;
        
        if (_reactToPlayer)
        {
            Player player = other.GetComponentInParent<Player>();

            if (player != null)
            {
                root = player;
                
                return true;
            }

            if (other.CompareTag(PLAYER_TAG))
            {
                root = other.attachedRigidbody ? (Component)other.attachedRigidbody : other.transform.root;
                
                return true;
            }
        }

        if (_reactToBots)
        {
            BotController bot = other.GetComponentInParent<BotController>();

            if (bot != null)
            {
                root = bot;
                
                return true;
            }

            if (_useBotTagCheck && other.CompareTag(BOT_TAG))
            {
                root = other.attachedRigidbody ? (Component)other.attachedRigidbody : other.transform.root;
                
                return true;
            }
        }
        
        return false;
    }
}