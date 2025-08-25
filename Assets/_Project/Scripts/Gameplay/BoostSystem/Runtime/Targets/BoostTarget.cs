using System.Collections;
using UnityEngine;

public class BoostTarget : MonoBehaviour
{
    [SerializeField, Min(0f)] private float _clearProbeRadius = 0.25f;
    
    private Coroutine _decelerationCoroutine;
    private Coroutine _flightCoroutine;
    
    private int _collisionOffFrames;
    
    public Rigidbody Rigidbody { get; private set; }
    public float ClearProbeRadius => _clearProbeRadius;
    public bool IsBoosting { get; private set; }

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (_collisionOffFrames > 0)
        {
            _collisionOffFrames--;
            
            if (_collisionOffFrames == 0 && Rigidbody != null)
                Rigidbody.detectCollisions = true;
        }
    }
    
    private void OnDisable()
    {
        if (Rigidbody != null)
            Rigidbody.detectCollisions = true;
        
        _collisionOffFrames = 0;
    }
    
    public void EnableCollisionDelayFrames(int frames)
    {
        if (frames <= 0 || Rigidbody == null)
            return;
        
        _collisionOffFrames = Mathf.Max(_collisionOffFrames, frames);
        Rigidbody.detectCollisions = false;
    }
    
    public void Boost(Vector3 direction, float speedMultiplier, float decelerationTime)
    {
        IsBoosting = true;
        StopRunningCoroutines();

        float currentSpeed = Rigidbody.velocity.magnitude;
        float baseSpeed = Mathf.Max(currentSpeed, 1f);

        Vector3 directionNormalized = direction.sqrMagnitude > 0.001f ? direction.normalized : transform.forward;
        directionNormalized.y = 0f;
        directionNormalized.Normalize();

        Vector3 newVelocity = directionNormalized * baseSpeed * speedMultiplier;
        newVelocity.y = 0f;

        Rigidbody.velocity = newVelocity;

        if (decelerationTime > 0f)
            _decelerationCoroutine = StartCoroutine(DecelerateTo(currentSpeed, decelerationTime));
        else
            IsBoosting = false;
    }

    public void BoostArc(Vector3 velocity)
    {
        StartArcFlight(velocity, 0f);
    }
    
    public void BoostArc(Vector3 velocity, float flightTime = -1f)
    {
        StartArcFlight(velocity, flightTime);
    }
    
    private void StartArcFlight(Vector3 velocity, float flightTime)
    {
        StopRunningCoroutines();
        
        IsBoosting = true;

        Rigidbody.isKinematic = false;
        Rigidbody.useGravity = true;
        Rigidbody.angularVelocity = Vector3.zero;
        Rigidbody.velocity = velocity;

        if (flightTime > 0f)
            _flightCoroutine = StartCoroutine(FlightRoutine(flightTime));
        else
            IsBoosting = false;
    }

    private void StopRunningCoroutines()
    {
        if (_decelerationCoroutine != null)
        {
            StopCoroutine(_decelerationCoroutine);
            _decelerationCoroutine = null;
        }

        if (_flightCoroutine != null)
        {
            StopCoroutine(_flightCoroutine);
            _flightCoroutine = null;
        }
    }
    
    private IEnumerator FlightRoutine(float flightTime)
    {
        BotController botDriver = GetComponentInParent<BotController>();
        
        if (botDriver != null)
            botDriver.SuspendControl(flightTime);
        
        PlayerCharacterRoot playerController = GetComponentInParent<PlayerCharacterRoot>();
        
        if (playerController != null)
            playerController.LockControl(flightTime);

        float previousDrag = Rigidbody.drag;
        float previousAngularDrag = Rigidbody.angularDrag;
        var prevConstraints = Rigidbody.constraints;
        
        Rigidbody.drag = 0f;
        Rigidbody.constraints = prevConstraints | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        yield return new WaitForSeconds(flightTime);

        Rigidbody.drag = previousDrag;
        Rigidbody.angularDrag = previousAngularDrag;
        Rigidbody.constraints = prevConstraints;

        IsBoosting = false;
        _flightCoroutine = null;
    }

    private IEnumerator DecelerateTo(float finalSpeed, float duration)
    {
        float startTime = Time.time;
        Vector3 startVelocity = Rigidbody.velocity;
        float startSpeed = new Vector2(startVelocity.x, startVelocity.z).magnitude;

        while (Time.time - startTime < duration)
        {
            float t = (Time.time - startTime) / duration;
            float speed = Mathf.Lerp(startSpeed, finalSpeed, t);

            Vector3 horizontalDirection = new Vector3(Rigidbody.velocity.x, 0f, Rigidbody.velocity.z).normalized;
            Rigidbody.velocity = horizontalDirection * speed;

            yield return null;
        }

        if (Rigidbody.velocity.sqrMagnitude > 0.0001f)
        {
            Vector3 horizontalDirection = new Vector3(Rigidbody.velocity.x, 0f, Rigidbody.velocity.z).normalized;
            Rigidbody.velocity = horizontalDirection * finalSpeed;
        }

        IsBoosting = false;
        _decelerationCoroutine = null;
    }
}