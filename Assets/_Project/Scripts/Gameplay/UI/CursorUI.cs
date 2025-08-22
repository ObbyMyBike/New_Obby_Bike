using UnityEngine;

public class CursorUI : MonoBehaviour
{
    private void Start()
    {
        if(Application.isMobilePlatform)
        {
            Destroy(gameObject);
            gameObject.SetActive(false);
        }
    }
}