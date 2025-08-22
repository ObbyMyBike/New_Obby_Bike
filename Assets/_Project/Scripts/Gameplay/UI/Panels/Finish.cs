using UnityEngine;

public class Finish : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject finishBar;
    [SerializeField] private BotSpawn _botSpawn;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerCharacterRoot player))
        {
            gameManager.Finish();
            finishBar.SetActive(true);

            Time.timeScale = 0;
            CameraControl.IsPause = true;

            if (!Application.isMobilePlatform)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
            }
            
            return;
        }
        
        if (other.TryGetComponent(out BotDriver bot))
            _botSpawn?.DespawnAndRespawn(bot);
    }
}