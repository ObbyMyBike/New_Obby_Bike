using UnityEngine;

public class CheckingAccessPlace
{
    private readonly LayerMask mask;
    private readonly Collider[] buffer = new Collider[16];
    private readonly float radius;

    public CheckingAccessPlace(LayerMask mask, float radius)
    {
        this.mask = mask;
        this.radius = Mathf.Max(0.01f, radius);
    }

    public bool IsClear(Vector3 pos)
    {
        int count = Physics.OverlapSphereNonAlloc(pos, radius, buffer, mask, QueryTriggerInteraction.Collide);
        
        return count == 0;
    }
}