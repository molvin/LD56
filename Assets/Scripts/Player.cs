using UnityEngine;

public class Player : MonoBehaviour
{
    private enum State
    {
        Running,
        Dodging,
        Attacking
    }

    [Header("Running")]
    public float MaxSpeed;
    public float MaxAcceleration;
    public float DriftFriction;
    public float OverSpeedingFriction;
    public AnimationCurve AccelerationCurve;
    public float StoppingTime;
    [Header("Dodging")]
    public float DodgeCooldown;
    public float DodgeDuration;
    public float DodgeSpeed;
    public AnimationCurve DodgeSpeedCurve;
    [Header("Attacking")]
    public float AttackCooldown;
    public float AttackStunDuration;
    public float AttackStoppingTime;
    [Header("Collision")]
    public CapsuleCollider Collider;
    public LayerMask GroundLayer;
    public int CollisionSteps = 4;
    public LayerMask CollisionLayers;
    public float SkinWidth;
    public float Bounciness = 0.1f;
    public float PushingForce;

    private Vector3 velocity;
    private Vector3 deceleration;
    private State state;
    private float timeOfLastDodge;
    private Vector3 dodgeDirection;

    private void Start()
    {
        state = State.Running;
    }

    private void Update()
    {
        switch (state)
        {
            case State.Running:
                Run();
                break;
            case State.Dodging:
                UpdateDodge();
                break;
            case State.Attacking:
                UpdateAttacking();
                break;
        }

        UpdateCollision();

        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        if (input.magnitude > 0.3f)
            transform.forward = input.normalized;
    }

    private void Run()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        if (input.magnitude > 0.3f)
            dodgeDirection = input.normalized;

        if (input.magnitude > 0.1f)
        {
            // Acceleration
            float velocityInInputDirection = Mathf.Clamp(Vector3.Dot(velocity, input.normalized), 0, MaxSpeed);
            float accelerationFactor = velocityInInputDirection / MaxSpeed;

            velocity += Vector3.ClampMagnitude(input, 1.0f) * MaxAcceleration * AccelerationCurve.Evaluate(accelerationFactor) * Time.deltaTime;
            
            // Drift friction
            Vector3 velocityNotInInputDirection = velocity - input.normalized * velocityInInputDirection;
            velocity -= velocityNotInInputDirection * DriftFriction * Time.deltaTime;

            // Overspeed friction
            float overSpeedAmount = Mathf.Max(Vector3.Dot(input.normalized, velocity) - MaxSpeed, 0);
            velocity -= velocity.normalized * overSpeedAmount * OverSpeedingFriction * Time.deltaTime;
        }
        else
        {
            velocity = Vector3.SmoothDamp(velocity, Vector3.zero, ref deceleration, StoppingTime);
        }

        if(Input.GetButtonDown("Jump"))
        {
            Dodge();
        }
    }

    private void Dodge()
    {
        if((Time.time - timeOfLastDodge) > DodgeCooldown)
        {
            timeOfLastDodge = Time.time;
            state = State.Dodging;
        }
    }

    private void UpdateDodge()
    {
        float t = (Time.time - timeOfLastDodge) / DodgeDuration;
        float speed = DodgeSpeed * DodgeSpeedCurve.Evaluate(t);
        velocity = dodgeDirection * speed;

        if(t > 1.0f)
        {
            state = State.Running;
        }

    }

    private void Attack()
    {
        // TODO: get weapon we are using and spawn it

        // TODO: get direction we fire in from mouse, on players plane

        // TODO: when stun is over switch state to running

        // 

    }

    private void UpdateAttacking()
    {

    }

    private void UpdateCollision()
    {
        bool Move(Vector3 position)
        {
            var colls = Physics.OverlapCapsule(
                position - Vector3.up * Collider.height * 0.5f,
                position + Vector3.up * Collider.height * 0.5f,
                Collider.radius,
                CollisionLayers
                );

            if (colls.Length > 0)
                return false;

            transform.position = position;
            return true;

        }

        RaycastHit hitInfo;
        bool hit;
        
        float halfHeight = Collider.height / 2.0f;
        Vector3 direction = velocity.normalized;
        float remainingMovement = velocity.magnitude * Time.deltaTime;

        for (int i = 0; i < CollisionSteps; i++)
        {
            Vector3 point1 = transform.position - Vector3.up * halfHeight;
            Vector3 point2 = transform.position + Vector3.up * halfHeight;

            hit = Physics.CapsuleCast(
                point1,
                point2,
                Collider.radius,
                direction,
                out hitInfo,
                remainingMovement + SkinWidth,
                CollisionLayers
            );

            if (!hit)
            {
                Move(transform.position + direction * remainingMovement);
                break;
            }
            else
            {
                if (remainingMovement <= SkinWidth)
                    break;

                Vector3 point = hitInfo.point;
                point.y = transform.position.y;
                Vector3 toPoint = point - transform.position;
                float moveDistance = toPoint.magnitude - SkinWidth;
                toPoint.Normalize();

                Vector3 normal = hitInfo.normal;
                normal.y = 0.0f;
                normal.Normalize();
                velocity += normal * Vector3.Dot(velocity, -normal) * (1.0f + Bounciness);
                velocity += normal * PushingForce * Time.deltaTime;

                if (moveDistance < 0.0f)
                    break;
                bool success = Move(transform.position + toPoint * moveDistance);
                direction = velocity.normalized;
                remainingMovement -= moveDistance;
                if (!success)
                    break;
            }
        }
        // Ground Snapping
        hit = Physics.Raycast(transform.position + Vector3.up * 50f, Vector3.down, out hitInfo, 100.0f, GroundLayer);
        if (hit)
        {
            Vector3 pos = transform.position;
            pos.y = hitInfo.point.y + Collider.height / 2.0f;
            transform.position = pos;
        }
    }

    private void OnGUI()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        GUILayout.Label($"Velocity: {velocity}");
        GUILayout.Label($"Speed: {velocity.magnitude}");
        float overSpeedAmount = Mathf.Max(Vector3.Dot(input.normalized, velocity) - MaxSpeed, 0);
        GUILayout.Label($"Overspeed: {overSpeedAmount}");
    }

}
