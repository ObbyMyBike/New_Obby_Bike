using UnityEngine;

public class TargetQuery
{
    public int FindTargetsInSphere(Vector3 center, float radius, Collider[] buffer, int layerMask = ~0,
        QueryTriggerInteraction triggerMode = QueryTriggerInteraction.Collide) =>
        Physics.OverlapSphereNonAlloc(center, radius, buffer, layerMask, triggerMode);

    public BoostTarget ExtractBoostTarget(Collider source)
    {
        if (source == null)
            return null;

        return source.GetComponent<BoostTarget>() ?? source.GetComponentInParent<BoostTarget>() ?? source.GetComponentInChildren<BoostTarget>();
    }
}