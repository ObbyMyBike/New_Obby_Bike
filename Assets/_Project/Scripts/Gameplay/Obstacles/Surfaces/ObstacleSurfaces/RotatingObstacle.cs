using UnityEngine;

public class RotatingObstacle : BaseObstacle
{
    [SerializeField] private Vector3 _rotationSpeed = new Vector3(0f, 90f, 0f);
   
    private void FixedUpdate()
    {
        transform.Rotate(_rotationSpeed * Time.fixedDeltaTime);
    }
}