using UnityEngine;
using Zenject;

public class BillboardToCamera : MonoBehaviour
{
    private Camera _target;

    [Inject]
    public void Construct([InjectOptional] Camera mainCamera)
    {
        _target = mainCamera != null ? mainCamera : Camera.main;
    }

    public void SetCamera(Camera mainCamera) => _target = mainCamera;

    private void LateUpdate()
    {
        if (_target == null)
        {
            Camera mainCamera = Camera.main;
            
            if (mainCamera == null)
                return;
            
            _target = mainCamera;
        }

        Quaternion rotation = _target.transform.rotation;
        transform.rotation = Quaternion.LookRotation(rotation * Vector3.forward, rotation * Vector3.up);
    }
}