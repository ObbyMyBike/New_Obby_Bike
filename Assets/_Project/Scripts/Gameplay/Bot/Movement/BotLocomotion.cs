using UnityEngine;

public class BotLocomotion
{
    private const float DEFAULT_SPEED_OVERSHOOT = 1.5f;
    
    public void HandleMovement(Rigidbody botRigidbody, Vector3 desired, bool isGrounded, float maxSpeed, float acceleration)
    {
        if (!IsFinite(botRigidbody.velocity))
            botRigidbody.velocity = Vector3.zero;
        
        float desiredMagnitude = desired.magnitude;
        Vector3 direction = desiredMagnitude > 1e-6f ? desired / desiredMagnitude : Vector3.zero;
        
        float throttle = Mathf.Clamp(desiredMagnitude, 0f, 2f);
        
        Vector3 targetVelocity = direction * (maxSpeed * throttle);
        Vector3 velocity = botRigidbody.velocity;
        Vector3 velocityFlat = new Vector3(velocity.x, 0f, velocity.z);
        Vector3 delta = targetVelocity - velocityFlat;
        
        float accelCap = Mathf.Max(acceleration, 1f);
        Vector3 accelerationVector = Vector3.ClampMagnitude(delta * Mathf.Max(0f, acceleration), accelCap);
        botRigidbody.AddForce(new Vector3(accelerationVector.x, 0f, accelerationVector.z), ForceMode.Acceleration);
        
        float targetDrag = isGrounded ? 3.5f : 0.5f;
        botRigidbody.drag = Mathf.Lerp(botRigidbody.drag, targetDrag, 8f * Time.fixedDeltaTime);
        
        float overshoot = DEFAULT_SPEED_OVERSHOOT;
        
        ClampSpeed(botRigidbody, maxSpeed * Mathf.Max(1f, throttle) * overshoot);
    }

    private void ClampSpeed(Rigidbody botRigidbody, float maxHorizSpeed)
    {
        if (!IsFinite(botRigidbody.velocity))
        {
            botRigidbody.velocity = Vector3.zero;
            
            return;
        }

        Vector3 flat = new Vector3(botRigidbody.velocity.x, 0f, botRigidbody.velocity.z);
        float speed = flat.magnitude;

        if (speed > maxHorizSpeed)
        {
            Vector3 limited = flat / speed * maxHorizSpeed;
            botRigidbody.velocity = new Vector3(limited.x, botRigidbody.velocity.y, limited.z);
        }
    }

    private bool IsFinite(Vector3 v) => !float.IsNaN(v.x) && !float.IsInfinity(v.x) && !float.IsNaN(v.y) && !float.IsInfinity(v.y) 
                                        && !float.IsNaN(v.z) && !float.IsInfinity(v.z);
}