using UnityEngine;

public class CheckpointOccupancyChecker
{
    private LayerMask _mask;
    private float _radius;

    public CheckpointOccupancyChecker(LayerMask mask, float radius)
    {
        _mask = mask;
        _radius = radius;
    }

    public void UpdateParams(LayerMask mask, float radius)
    {
        _mask = mask;
        _radius = radius;
    }

    public bool IsClear(Vector3 position)
    {
        return !IsOccupied(position);
        
    }
    private bool IsOccupied(Vector3 position)
    {
        Collider[] hits = Physics.OverlapSphere(position, _radius, _mask);
        
        return hits != null && hits.Length > 0;
    }
}