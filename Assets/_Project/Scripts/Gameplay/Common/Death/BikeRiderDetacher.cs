using UnityEngine;

public class BikeRiderDetacher : MonoBehaviour
{
    [Header("Roots")]
    [SerializeField] private Transform _riderRoot;
    [SerializeField] private Transform _bikeRoot;
    
    [Header("Kick params")]
    [SerializeField] private float _kickForward = 3f;
    [SerializeField] private float _kickUp = 2f;
    [SerializeField] private float _kickTorque = 2f;

    [Header("Collider override")]
    [SerializeField] private bool _overrideColliders = true;
    [SerializeField] private BoxColliderSettings _riderCollider = new BoxColliderSettings
        {
            WorldSize = new Vector3(0.03f, 0.06f, 0.07f),
            WorldCenter = new Vector3(0f, 0.02f, -0.005f)
        };
    [SerializeField] private BoxColliderSettings _bikeCollider = new BoxColliderSettings
        { 
            WorldSize = new Vector3(1f, 2f, 3.4f),
            WorldCenter = new Vector3(0f, 1f, -3.95f)
        };
    
    private Saved _rider;
    private Saved _bike;

    private bool _detached;

    private void Awake()
    {
        if (_riderRoot != null)
            _rider = Cache(_riderRoot);
        
        if (_bikeRoot != null)
            _bike = Cache(_bikeRoot);
    }

    public void OverrideRoots(Transform newRiderRoot, Transform newBikeRoot = null)
    {
        if (newRiderRoot != null)
        {
            _riderRoot = newRiderRoot;
            _rider = Cache(_riderRoot);
        }

        if (newBikeRoot != null)
        {
            _bikeRoot = newBikeRoot;
            _bike = Cache(_bikeRoot);
        }
    }
    
    public void Detach(Vector3 forward)
    {
        if (_detached)
            return;
        
        RefreshIfDestroyed(ref _rider, _riderRoot);
        RefreshIfDestroyed(ref _bike, _bikeRoot);

        if (_rider?.ObjectTransform)
            DetachOne(_rider, forward, isRider:true);
        
        if (_bike?.ObjectTransform)
            DetachOne(_bike, forward, isRider:false);
        
        _detached = true;
    }

    public void Reattach()
    {
        if (!_detached)
            return;

        RefreshIfDestroyed(ref _rider, _riderRoot);
        RefreshIfDestroyed(ref _bike, _bikeRoot);

        if (_rider?.ObjectTransform)
            ReattachOne(_rider);
        
        if (_bike?.ObjectTransform)
            ReattachOne(_bike);

        _detached = false;
    }
    
    private Saved Cache(Transform root)
    {
        return new Saved
        {
            ObjectTransform = root,
            Parent = root ? root.parent : null,
            LocalPosition = root ? root.localPosition : Vector3.zero,
            LocalRotation = root ? root.localRotation : Quaternion.identity,
            LocalScale = root ? root.localScale : Vector3.one,
            HadRigidbody = root && root.TryGetComponent(out Rigidbody _)
        };
    }

    private void RefreshIfDestroyed(ref Saved saved, Transform currentField)
    {
        if (saved == null || !saved.ObjectTransform)
            if (currentField)
                saved = Cache(currentField);
    }
    
    private void DetachOne(Saved saved, Vector3 forward, bool isRider)
    {
        if (!saved.ObjectTransform)
            return;
        
        saved.Parent = saved.ObjectTransform.parent;
        saved.LocalPosition = saved.ObjectTransform.localPosition;
        saved.LocalRotation = saved.ObjectTransform.localRotation;
        saved.LocalScale = saved.ObjectTransform.localScale;
        
        saved.ObjectTransform.SetParent(null, true);

        if (!saved.ObjectTransform.TryGetComponent(out Rigidbody objectRigidbody))
            objectRigidbody = saved.ObjectTransform.gameObject.AddComponent<Rigidbody>();
        
        saved.ObjectRigidbody = objectRigidbody;
        objectRigidbody.isKinematic = false;
        objectRigidbody.useGravity = true;
        
        if (_overrideColliders)
            OverrideColliders(saved, isRider ? _riderCollider : _bikeCollider);
        else
            EnsureAnyCollider(saved);
        
        Vector3 direction = forward.sqrMagnitude > 0.0001f ? forward.normalized : transform.forward;
        
        objectRigidbody.AddForce(direction * _kickForward + Vector3.up * _kickUp, ForceMode.VelocityChange);
        objectRigidbody.AddTorque(Random.onUnitSphere * _kickTorque, ForceMode.VelocityChange);
    }
    
    private void ReattachOne(Saved saved)
    {
        if (saved.ObjectRigidbody != null)
        {
            saved.ObjectRigidbody.velocity = Vector3.zero;
            saved.ObjectRigidbody.angularVelocity = Vector3.zero;

            if (!saved.HadRigidbody)
                Destroy(saved.ObjectRigidbody);
            else
                saved.ObjectRigidbody.isKinematic = true;
        }
        
        for (int i = 0; i < saved.AddedColliders.Count; i++)
            if (saved.AddedColliders[i] != null)
                Destroy(saved.AddedColliders[i]);
        
        saved.AddedColliders.Clear();
        
        for (int i = 0; i < saved.DisabledColliders.Count; i++)
        {
            var st = saved.DisabledColliders[i];
            if (st != null && st.Collider != null)
                st.Collider.enabled = st.WasEnabled;
        }
        saved.DisabledColliders.Clear();
        
        Transform targetParent = saved.Parent ? saved.Parent : transform;
        
        saved.ObjectTransform.SetParent(targetParent, false);
        saved.ObjectTransform.localPosition = saved.LocalPosition;
        saved.ObjectTransform.localRotation = saved.LocalRotation;
        saved.ObjectTransform.localScale = saved.LocalScale;
    }
    
    private void OverrideColliders(Saved saved, BoxColliderSettings settings)
    {
        Collider[] existing = saved.ObjectTransform.GetComponentsInChildren<Collider>(true);
        
        if (existing != null)
        {
            for (int i = 0; i < existing.Length; i++)
            {
                Collider objectCollider = existing[i];
                
                if (!objectCollider)
                    continue;
                
                saved.DisabledColliders.Add(new DisabledColliderState { Collider = objectCollider, WasEnabled = objectCollider.enabled });
                
                objectCollider.enabled = false;
            }
        }
        
        BoxCollider boxCollider = saved.ObjectTransform.gameObject.AddComponent<BoxCollider>();
        
        saved.AddedColliders.Add(boxCollider);
        
        Vector3 savedScale = saved.ObjectTransform.lossyScale;
        Vector3 safeScale = new Vector3(Mathf.Approximately(savedScale.x, 0f) ? 1f : savedScale.x, Mathf.Approximately(savedScale.y, 0f) ? 1f : savedScale.y, Mathf.Approximately(savedScale.z, 0f) ? 1f : savedScale.z);

        boxCollider.size = new Vector3(settings.WorldSize.x   / safeScale.x, settings.WorldSize.y   / safeScale.y, settings.WorldSize.z   / safeScale.z);
        boxCollider.center = new Vector3(settings.WorldCenter.x / safeScale.x, settings.WorldCenter.y / safeScale.y, settings.WorldCenter.z / safeScale.z);
        boxCollider.enabled = true;
    }
    
    private void EnsureAnyCollider(Saved saved)
    {
        Collider[] existing = saved.ObjectTransform.GetComponentsInChildren<Collider>(true);
        
        if (existing == null || existing.Length == 0)
        {
            BoxCollider boxCollider = saved.ObjectTransform.gameObject.AddComponent<BoxCollider>();
            saved.AddedColliders.Add(boxCollider);
        }
    }
}