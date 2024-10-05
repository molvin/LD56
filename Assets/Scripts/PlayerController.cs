using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float BaseSpeed;
    public float Acceleration;
    public float PlanarWingDrag;
    public float AirDrag;

    public float RollAcceleration;
    public float MaxRollSpeed;
    public float RollDrag;
    public float PlanarWingRollForce;
    public float StraightenRollForce;

    public float PitchAcceleration;
    public float MaxPitchSpeed;
    public float PitchDrag;

    public float AngleOffset;

    public Vector3 GlideRotationSpeed;
    public float BoostSmoothing;
    public float DecelerationSmoothing;
    public float GlideDrag;
    public float GlideGravity;

    public Transform Model;

    [Header("Camera")]
    public Camera FollowCamera;
    public float CameraSmoothing;
    public Transform LookAtPoint;
    public float DefaultFov = 80;
    public float StallFov = 65;
    public float FovSmoothing = 0.1f;

    [Header("Shooting")]
    public Transform FirePoint;
    public Bullet BulletPrefab;
    public float FireRate;

    private Rigidbody rb;
    private Vector3 cameraOffset;
    private Vector3 cameraVelocity;
    private float rollVelocity;
    private float pitchVelocity;
    private float lastFireTime;
    private float fovVelocity;

    private void Start()
    {
        cameraOffset = FollowCamera.transform.localPosition;
        rb = GetComponent<Rigidbody>();
        rb.velocity = Model.forward * BaseSpeed;
    }

    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        bool stall = Input.GetButton("Stall");

        if(!stall)
        {
            {
                float t = (1.0f - (Mathf.Abs(rollVelocity) / MaxRollSpeed));
                rollVelocity += (-horizontal) * RollAcceleration * t * Time.deltaTime;
                rollVelocity -= rollVelocity * RollDrag * Time.deltaTime;

                float velocityForwardFactor = (1.0f - Mathf.Abs(Vector3.Dot(Model.forward, rb.velocity.normalized)));
                float velocitySide = Mathf.Abs(Vector3.Dot(Model.right, rb.velocity));
                float upSign = Mathf.Sign(Vector3.Dot(Model.up, rb.velocity.normalized));
                float angle = Mathf.Sign(Vector3.SignedAngle(Model.right * upSign, rb.velocity.normalized, Model.forward) - AngleOffset);
                rollVelocity += angle * velocityForwardFactor * velocitySide * PlanarWingRollForce * Time.deltaTime;


                float upDot = Vector3.Dot(Model.up, Vector3.up) + 0.5f;
                float upAngle = Mathf.Sign(Vector3.SignedAngle(Model.up, Vector3.up, Model.forward));

                rollVelocity += (1.5f - Mathf.Max(upDot, 0)) * upAngle * Mathf.Max(Vector3.Dot(Model.forward, rb.velocity.normalized), 0) * StraightenRollForce * Time.deltaTime;
            }

            {
                float speed = Vector3.Dot(rb.velocity, Model.forward);
                float t = 1.0f - (speed / BaseSpeed);
                rb.velocity += Model.forward * Acceleration * t * Time.deltaTime;
                rb.velocity -= rb.velocity.normalized * Mathf.Abs(Vector3.Dot(rb.velocity, Model.up)) * PlanarWingDrag * Time.deltaTime;
                rb.velocity -= rb.velocity * AirDrag * Time.deltaTime;
            }
        }
        else
        {
            Model.Rotate(new Vector3(0, horizontal * GlideRotationSpeed.y, 0) * Time.deltaTime, Space.World);
            Model.Rotate(Vector3.Cross(Model.forward, Vector3.up), -vertical * GlideRotationSpeed.x * Time.deltaTime, Space.World);
            //Model.Rotate(new Vector3(vertical * GlideRotationSpeed.x, 0, 0) * Time.deltaTime, Space.World);

            //rb.velocity = Vector3.SmoothDamp(rb.velocity, Vector3.zero, ref acceleration, DecelerationSmoothing);

            Vector3 planar = rb.velocity;
            planar.y = 0.0f;
            rb.velocity -= planar * GlideDrag * Time.deltaTime;
            rb.velocity += Vector3.down * GlideGravity * Time.deltaTime;
        }

        {
            float t = (1.0f - (Mathf.Abs(pitchVelocity) / MaxPitchSpeed));
            pitchVelocity += vertical * PitchAcceleration * t * Time.deltaTime;
            pitchVelocity -= pitchVelocity * PitchDrag * Time.deltaTime;
        }

        Vector3 deltaRotation = new Vector3(pitchVelocity, 0, rollVelocity);
        Model.Rotate(deltaRotation * Time.deltaTime);
        // TODO: vertical pitches plane

        UpdateShoot();
    }

    private void LateUpdate()
    {
        bool stall = Input.GetButton("Stall");

        FollowCamera.fieldOfView = Mathf.SmoothDamp(FollowCamera.fieldOfView, stall ? StallFov : DefaultFov, ref fovVelocity, FovSmoothing);

        FollowCamera.transform.position = Vector3.SmoothDamp(FollowCamera.transform.position, transform.position + Model.rotation * cameraOffset, ref cameraVelocity, CameraSmoothing);

        FollowCamera.transform.LookAt(LookAtPoint);
    }

    private void UpdateShoot()
    {
        if((Input.GetAxis("Fire1") > 0.3f || Input.GetButton("Fire1")) && (Time.time - lastFireTime) > FireRate)
        {
            lastFireTime = Time.time;
            Bullet bullet = Instantiate(BulletPrefab, FirePoint.position, FirePoint.rotation);
            bullet.Init(Model.forward * rb.velocity.magnitude);
        }
    }
}
