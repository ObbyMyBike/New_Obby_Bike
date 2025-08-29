using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelDirector : MonoBehaviour
{
    public event Action<int, int> ActiveLevelChanged;
    
    [SerializeField] private LevelCheckpoints[] _levels;
    [SerializeField] private int _defaultActiveLevel = 0;

    private readonly Dictionary<CheckPoints, int> levelByCheckpoint = new();
    private readonly Dictionary<int, List<CheckPoints>> listByLevel = new();
    private readonly Dictionary<int, Dictionary<int, CheckPoints>> lookupByNumber = new();
    private readonly Dictionary<int, Waypoint> startByLevel = new();
    private readonly List<int> orderedLevelIndices = new(); 
    
    private RacePath _currentPath;
    private RacePath _globalPath;
    
    private List<CheckPoints> _allCheckpointsOrdered;
    private int _activeLevel;
    
    public RacePath GlobalPath  => _globalPath  ?? BuildGlobalPath();
    public RacePath CurrentPath => _currentPath ?? BuildPathForActive();
    public IReadOnlyList<CheckPoints> AllCheckpoints => _allCheckpointsOrdered ?? (IReadOnlyList<CheckPoints>)Array.Empty<CheckPoints>();
    public int ActiveLevelIndex => _activeLevel;
    
    private void Awake()
    {
        BuildCaches();
        SetActiveLevel(_defaultActiveLevel);
    }

    public IReadOnlyList<CheckPoints> GetLevelCheckpoints(int levelIndex) => listByLevel.TryGetValue(levelIndex, out var list) ? list : Array.Empty<CheckPoints>();
    
    public bool TryGetLevelStart(int levelIndex, out Waypoint start) => startByLevel.TryGetValue(levelIndex, out start) && start != null;
    
    public bool TryGetNextLevelStartFrom(CheckPoints fromCheckpoint, out int nextLevelIndex, out Waypoint startWaypoint)
    {
        nextLevelIndex = -1;
        startWaypoint = null;

        if (fromCheckpoint == null)
            return false;
        
        if (!levelByCheckpoint.TryGetValue(fromCheckpoint, out int levelOfCp))
            return false;

        int position = orderedLevelIndices.IndexOf(levelOfCp);
        
        if (position < 0 || position + 1 >= orderedLevelIndices.Count)
            return false;

        nextLevelIndex = orderedLevelIndices[position + 1];
        
        return TryGetLevelStart(nextLevelIndex, out startWaypoint);
    }
    
    public void GoToLevel(int levelIndex) => SetActiveLevel(levelIndex);
    
    private void BuildCaches()
    {
        listByLevel.Clear();
        lookupByNumber.Clear();
        startByLevel.Clear();
        levelByCheckpoint.Clear();
        orderedLevelIndices.Clear();

        if (_levels == null)
            return;
        
        foreach (LevelCheckpoints level in _levels)
        {
            if (level == null)
                continue;
            
            int index = level.LevelIndex;

            List<CheckPoints> checkPoints = Sanitize(level.Checkpoints);
            listByLevel[index] = checkPoints;

            var checkPointsMap = new Dictionary<int, CheckPoints>(checkPoints.Count);
            
            foreach (CheckPoints checkPoint in checkPoints)
            {
                if (checkPoint != null)
                {
                    checkPointsMap[checkPoint.Number] = checkPoint;
                    levelByCheckpoint[checkPoint] = index;
                }
            }
            
            lookupByNumber[index] = checkPointsMap;
            
            if (level.LevelStart != null)
                startByLevel[index] = level.LevelStart;
            
            orderedLevelIndices.Add(index);
        }
        
        orderedLevelIndices.Sort();

        BuildGlobalCheckpointsOrdered();
        
        _globalPath = BuildGlobalPath();
    }

    private List<CheckPoints> Sanitize(CheckPoints[] arrayCheckPoints)
    {
        List<CheckPoints> checkPointsList = new List<CheckPoints>();
        
        if (arrayCheckPoints != null)
        {
            foreach (CheckPoints checkPoint in arrayCheckPoints)
            {
                if (checkPoint != null)
                    checkPointsList.Add(checkPoint);
            }
        }

        checkPointsList.Sort((checkPointA, checkPointsB) => checkPointA.Number.CompareTo(checkPointsB.Number));
        
        List<CheckPoints> dedup = new List<CheckPoints>(checkPointsList.Count);
        int? last = null;
        
        foreach (CheckPoints checkPoint in checkPointsList)
        {
            if (last == null || checkPoint.Number != last.Value)
            {
                dedup.Add(checkPoint);
                
                last = checkPoint.Number;
            }
        }
        
        return dedup;
    }

    private RacePath BuildPathForActive()
    {
        if (!listByLevel.TryGetValue(_activeLevel, out var list) || list.Count < 2)
            return new RacePath(Array.Empty<CheckPoints>());
        
        return new RacePath(list.ToArray());
    }
    
    private RacePath BuildGlobalPath()
    {
        if (orderedLevelIndices == null || orderedLevelIndices.Count == 0)
            return new RacePath(Array.Empty<Vector3>(), true);

        List<Vector3> points = new List<Vector3>(256);

        foreach (int levelIndex in orderedLevelIndices)
        {
            if (startByLevel.TryGetValue(levelIndex, out var start) && start != null)
                points.Add(start.transform.position);

            if (listByLevel.TryGetValue(levelIndex, out var list) && list != null)
            {
                foreach (CheckPoints checkPoint in list)
                {
                    if (checkPoint != null)
                        points.Add(checkPoint.transform.position);
                }
            }
        }

        if (points.Count < 2)
            return new RacePath(Array.Empty<Vector3>(), true);
        
        return new RacePath(points.ToArray(), true);
    }
    
    private void BuildGlobalCheckpointsOrdered()
    {
        _allCheckpointsOrdered = new List<CheckPoints>(128);

        if (_levels == null || _levels.Length == 0)
            return;
        
        Array.Sort(_levels, (a, b) => a.LevelIndex.CompareTo(b.LevelIndex));

        foreach (LevelCheckpoints level in _levels)
        {
            if (level == null)
                continue;
            
            List<CheckPoints> sanitized = Sanitize(level.Checkpoints);
            
            _allCheckpointsOrdered.AddRange(sanitized);
        }
    }
    
    private void SetActiveLevel(int levelIndex)
    {
        if (_activeLevel == levelIndex && _currentPath != null)
            return;

        int previewLevel = _activeLevel;
        
        _activeLevel = levelIndex;
        _currentPath = BuildPathForActive();

        ActiveLevelChanged?.Invoke(previewLevel, _activeLevel);
    }
}