using UnityEngine;

public class BotAIController : MonoBehaviour
{
    private BotInputAI _inputAI;
    
    public void SetAI(BotInputAI ai) => _inputAI = ai;
    
    private void Update() => _inputAI?.Tick();
}