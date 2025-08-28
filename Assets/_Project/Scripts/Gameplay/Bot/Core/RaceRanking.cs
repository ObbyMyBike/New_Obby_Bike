using UnityEngine;
using Zenject;

public class RaceRanking : ITickable
{
    private const float EPS = 0.0005f;
    
    private readonly Player player;
    private readonly LevelDirector levelDirector;
    private readonly BotRegistry botRegistry;
    private readonly RacePlaceView placeView;

    private RacePath _path;

    [Inject]
    public RaceRanking(Player player, LevelDirector levelDirector, BotRegistry botRegistry, RacePlaceView placeView)
    {
        this.player = player;
        this.levelDirector = levelDirector;
        this.botRegistry = botRegistry;
        this.placeView = placeView;

        _path = this.levelDirector?.GlobalPath;
        
        if (this.levelDirector != null)
            this.levelDirector.ActiveLevelChanged += OnLevelChanged;
    }
    
    void ITickable.Tick()
    {
        if (player?.PlayerCharacterRoot == null || _path == null || !_path.IsValid)
            return;

        Transform playerTransform = player.PlayerCharacterRoot.transform;
        float playerProgress = _path.ComputeProgress(playerTransform.position);
        
        int totalRacers = 1;
        int ahead = 0;

        foreach (BotController bot in botRegistry.All)
        {
            if (bot == null)
                continue;
            
            Transform botTransform = bot.transform;
            float positionProgress = _path.ComputeProgress(botTransform.position);
            
            totalRacers++;
            
            if (positionProgress > playerProgress + EPS)
                ahead++;
        }

        int place = ahead + 1;
        
        placeView?.SetPlace(place, totalRacers);
    }
    
    private void OnLevelChanged(int oldIdx, int newIdx) => _path = levelDirector?.GlobalPath;
}