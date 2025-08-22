using System.Collections;
using UnityEngine;
using Zenject;

public abstract class BaseObstacle : MonoBehaviour
{
    private readonly float timeDelay = 0.5f;
    
    [Inject] private Restart _restartPanel;
    [Inject] private DeathEffect _deathEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out BotRespawn botRespawner))
        {
            _deathEffect.PlayDieEffect(botRespawner.transform.position);
            
            BikeRiderDetacher botDetacher = botRespawner.GetComponent<BikeRiderDetacher>();
            
            if (botDetacher != null)
                botDetacher.Detach(botRespawner.transform.forward);
            
            StartCoroutine(RespawnBotAfterKick(botRespawner, botDetacher));
            
            return;
        }
        
        if (!other.TryGetComponent(out PlayerCharacterRoot player))
            return;
        
        _deathEffect.PlayDieEffect(player.transform.position);
        
        if (other.TryGetComponent(out BoostTarget boostTarget))
            boostTarget.BoostArc(Vector3.zero);
        
        BikeRiderDetacher playerDetacher = player.GetComponent<BikeRiderDetacher>();
        
        if (playerDetacher != null)
            playerDetacher.Detach(player.transform.forward);
        
        StartCoroutine(ShowRestartAfterKick(other.gameObject));
    }
    
    private IEnumerator RespawnBotAfterKick(BotRespawn botRespawn, BikeRiderDetacher detacher)
    {
        yield return new WaitForSecondsRealtime(timeDelay);
        
        if (detacher != null)
            detacher.Reattach();

        botRespawn.Respawn();
    }

    
    private IEnumerator ShowRestartAfterKick(GameObject characterObject)
    {
        yield return new WaitForSecondsRealtime(timeDelay);

        Time.timeScale = 0f;

        if (!Application.isMobilePlatform)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
        
        characterObject.SetActive(false);
        _restartPanel.gameObject.SetActive(true);
        
        CameraControl.IsPause = true;
    }
}