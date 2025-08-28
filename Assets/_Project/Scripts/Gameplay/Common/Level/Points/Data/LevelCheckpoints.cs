using System;

[Serializable]
public class LevelCheckpoints
{
    public int LevelIndex;
    public Waypoint LevelStart; 
    public CheckPoints[] Checkpoints;
}