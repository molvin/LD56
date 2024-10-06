using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomerangController : MonoBehaviour
{
    public const float SMALL_NUMBER = 0.001f;

    public Material ThrowMat;
    public Material ReturnMat;

    public const float PhaseTime = 3.0f;

    public BadController Owner;

    public float StayTime = 1.6f;
    public float Bouncyness = 0.24f;
    public float InitialSpeed = 26f;
    public float ReturnAcceleration = 16f;
    public float ReturnJerk = 46f;
    public float ReturnDrag = 0.1f;
    public float StayEffect = 0.16f;

    public Vector2 velocity;
    private bool returning = false;
    private float stuckTime = 0f;
    private float timeStayed = 0f;

    private void Awake()
    {
        returning = false;
    }
    private void OnEnable()
    {
        returning = false;
    }

    void Update()
    {
        if (velocity.magnitude > SMALL_NUMBER)
        {
            stuckTime = 0f;
        }
        else
        {
            stuckTime += Time.deltaTime;
        }

        if (stuckTime > PhaseTime)
        {
            // TODO: Phase
            Destroy(gameObject);
        }

        GetComponent<MeshRenderer>().material = returning ? ReturnMat : ThrowMat;

        Vector3 toOwner = (Owner.transform.position - transform.position);
        Vector2 accDir = new Vector2(toOwner.x, toOwner.z).normalized;

        float deltaTime = Time.deltaTime * (returning && timeStayed < StayTime ? Mathf.Lerp(StayEffect, 1f, timeStayed / StayTime) : 1f);

        velocity += accDir * ReturnAcceleration * deltaTime;
        ReturnAcceleration += ReturnJerk * deltaTime;

        if (!returning && Vector2.Dot(velocity, accDir) > 0)
        {
            returning = true;
            timeStayed = 0f;
        }

        if (returning)
        {
            timeStayed += Time.deltaTime;

            float dot = (Vector2.Dot(velocity.normalized, accDir) + 1f) / 2f;
            float damping = Mathf.Lerp(ReturnDrag * .5f, ReturnDrag * 2f, dot);
            velocity *= Mathf.Pow(damping, deltaTime);
        }

        Vector2 actual = new Vector2(transform.position.x, transform.position.z);
        Vector2 target = actual + velocity * Time.deltaTime;
        float y = transform.position.y;

        if (UnityEngine.AI.NavMesh.SamplePosition(
            new Vector3(target.x, transform.position.y - 1f, target.y),
            out UnityEngine.AI.NavMeshHit hit,
            2f,
            UnityEngine.AI.NavMesh.AllAreas))
        {
            actual = new Vector2(hit.position.x, hit.position.z);
            y = hit.position.y + 1f;
        }

        if (Vector2.Distance(actual, target) > SMALL_NUMBER)
        {
            Vector2 normal = (actual - target).normalized;
            Vector2 projection = normal * Vector2.Dot(velocity, normal);
            velocity -= projection * (1f + Bouncyness);
        }

        transform.position = new Vector3(actual.x, y, actual.y);

        if (Vector3.Distance(transform.position, Owner.transform.position) < 1.5f)
        {
            Destroy(gameObject);
        }
    }
}
