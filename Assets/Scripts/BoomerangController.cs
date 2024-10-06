using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BoomerangController : MonoBehaviour
{
    public const float SMALL_NUMBER = 0.001f;

    public Material ThrowMat;
    public Material ReturnMat;

    public const float PhaseTime = 3.0f;
    public const float MinLifeTime = 3.0f;

    public bool GracePeriod => Time.time - spawnTime < MinLifeTime;

    public GameObject Owner;

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
    private float spawnTime;

    private Audioman.LoopHolder loopHolderSteps;

    private void Awake()
    {
        returning = false;
        spawnTime = Time.time;
    }
    private void OnEnable()
    {
        returning = false;
        spawnTime = Time.time;
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

        if (!GracePeriod && stuckTime > PhaseTime)
        {
            // TODO: Phase
            Destroy(gameObject);
        }

        GetComponent<MeshRenderer>().material = returning ? ReturnMat : ThrowMat;

        Accelerate();

        Move();
        RegulateMovementVolume();

        HitBoid();

        if (!GracePeriod && Vector3.Distance(transform.position, Owner.transform.position) < 1f)
        {
            loopHolderSteps?.Stop();
            Audioman.getInstance()?.PlaySound(Resources.Load<AudioOneShotClipConfiguration>("object/back_to_pouch"), this.transform.position);
            Destroy(gameObject);
        }
    }

    private void Accelerate()
    {
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
    }

    private void Move()
    {
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
    }

    private void HitBoid()
    {
        if (Boids.Instance == null) return;

        List<Rigidbody> boids = Boids.Instance.GetNearest(transform.position, 8);

        Vector2 thisPos = new Vector2(transform.position.x, transform.position.z);

        foreach (Rigidbody b in boids)
        {
            if (b == null) continue;

            Vector2 boidPos = new Vector2(b.position.x, b.position.z);
            if (Vector2.Distance(thisPos, boidPos) < 1f)
            {
                Boids.Instance.DamageBoid(b);
                Audioman.getInstance()?.PlaySound(Resources.Load<AudioOneShotClipConfiguration>("object/chomp"), this.transform.position);

            }
        }
    }

    private void RegulateMovementVolume()
    {
        var auido_man = Audioman.getInstance();
        if(auido_man == null)
        {
            Debug.Log("No audioman in scene");
            return;
        }
        if(loopHolderSteps == null)
        {
            loopHolderSteps = auido_man.PlayLoop(Resources.Load<AudioLoopConfiguration>("object/creature_step_loop"), this.transform.position);
        }
        loopHolderSteps.setVolume(velocity.magnitude > 0.1f ? velocity.magnitude / InitialSpeed : 0);


    }
}
