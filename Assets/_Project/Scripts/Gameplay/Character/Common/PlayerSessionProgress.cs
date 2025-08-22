using System.Collections.Generic;

public static class PlayerSessionProgress
{
    public static int Point;
    public static int LastCheckpointNum = -1;
    
    public static HashSet<int> CollectedCheckpoints = new();

    public static void Reset()
    {
        CollectedCheckpoints.Clear();
        LastCheckpointNum = -1;
        Point = 0;
    }
}