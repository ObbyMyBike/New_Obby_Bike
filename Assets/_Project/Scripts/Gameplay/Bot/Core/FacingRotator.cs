using UnityEngine;

public class FacingRotator
{
    public void RotateTowardsMotion(Transform transform, Rigidbody rigidbody, Vector3 desiredMoveDirection, float angularSpeedDegPerSec)
    {
        Vector3 refDirection = (rigidbody.velocity.sqrMagnitude > 0.16f) ? new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z) : new Vector3(desiredMoveDirection.x, 0f, desiredMoveDirection.z);

        if (refDirection.sqrMagnitude > 1e-4f)
        {
            Quaternion target = Quaternion.LookRotation(refDirection.normalized);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, angularSpeedDegPerSec * Time.deltaTime);
        }
    }
}