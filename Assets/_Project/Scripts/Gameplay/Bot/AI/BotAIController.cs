using UnityEngine;

public class BotAIController : MonoBehaviour
{
    public BotInputAI AI { get; private set; }
    
    public void SetAI(BotInputAI ai) => AI = ai;
    
    private void Update() => AI?.Tick();
}