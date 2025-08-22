using UnityEngine;

public class NameAssigner
{
    private readonly NameGenerator _generator;
    private readonly NameplateService _nameplates;

    public NameAssigner(NameGenerator generator, NameplateService nameplates)
    {
        _generator = generator;
        _nameplates = nameplates;
    }

    public void AssignToPlayer(Player player)
    {
        if (player == null)
            return;
        
        GameObject playerObject = player.PlayerCharacterRoot.gameObject;
        
        _nameplates.Attach(playerObject, _generator.GetNext());
    }

    public void AssignToBot(GameObject botRoot)
    {
        if (!botRoot)
            return;
        
        _nameplates.Attach(botRoot, _generator.GetNext());
    }
}