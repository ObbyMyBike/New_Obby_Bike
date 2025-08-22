using UnityEngine;

public class RigidbodyGuard
{
    public void Sanitize(Rigidbody rigidbody, Transform objectTransform)
    {
        if (!IsFinite(objectTransform.position))
        {
            rigidbody.position = Vector3.zero;
            rigidbody.velocity = Vector3.zero;
        }

        if (!IsFinite(rigidbody.velocity))
            rigidbody.velocity = Vector3.zero;
    }

    private bool IsFinite(Vector3 v) => !float.IsNaN(v.x) && !float.IsInfinity(v.x) && !float.IsNaN(v.y) && !float.IsInfinity(v.y) &&
                                        !float.IsNaN(v.z) && !float.IsInfinity(v.z);
}