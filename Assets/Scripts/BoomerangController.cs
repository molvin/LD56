using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomerangController : MonoBehaviour
{
    public const float SMALL_NUMBER = 0.001f;
    public const float InternalHitCooldown = 0.225f;
    public const float InternalProcCooldown = 0.15f;

    public Material ThrowMat;
    public Material ReturnMat;

    public const float PhaseTime = 3.0f;
    public const float MinLifeTime = 0.3f;

    public bool GracePeriod => Time.time - spawnTime < MinLifeTime;

    public Player Owner;
    public Weapon Weapon;

    public float StayTime = 1.6f;
    public float Bouncyness = 0.24f;
    public float InitialSpeed = 26f;
    public float ReturnAcceleration = 16f;
    public float ReturnJerk = 46f;
    public float ReturnDrag = 0.1f;
    public float StayEffect = 0.16f;
    public float EndStayEffect = 0.92f;

    public Vector2 velocity;
    private bool returning = false;
    private float stuckTime = 0f;
    private float timeStayed = 0f;
    private float spawnTime;
    private float lastPeriodProc;

    public bool Animated = false;
    private List<Vector3> animationPoints;
    public bool Temporary = false;

    private Audioman.LoopHolder loopHolderSteps;

    private Dictionary<Boid, float> internalBoidCooldown = new();
    private float lastProcTime = 0f;

    public bool IsInternalBoidCooldown(Boid b) => internalBoidCooldown.TryGetValue(b, out float time) && Time.time - time < InternalHitCooldown;

    public Vector2 Position2D => new Vector2(transform.position.x, transform.position.z);



    public void Init(Player owner, Weapon weapon, Vector3 position, Vector2 throwDirection, Vector2 extraVelocity)
    {
        Owner = owner;
        Weapon = weapon;
        Weapon.Reset();
        transform.position = position;

        InitialSpeed *= weapon.InitialSpeedBoost * weapon.SpeedModifier;
        ReturnAcceleration *= weapon.SpeedModifier;
        ReturnJerk *= weapon.SpeedModifier;
        StayEffect *= weapon.StayModifier;
        EndStayEffect *= weapon.StayModifier;
        StayTime /= weapon.StayModifier;

        velocity = throwDirection * InitialSpeed + extraVelocity;

        Weapon.OnSpawn?.Invoke(this);
        Audioman.getInstance().PlaySound(Resources.Load<AudioOneShotClipConfiguration>("object/throw_minion"), this.transform.position);

        GetComponentInChildren<Animator>()?.SetTrigger("Toss");
    }

    private void OnDestroy()
    {
        loopHolderSteps?.Stop();
    }

    private void Awake()
    {
        returning = false;
        spawnTime = Time.time;
    }
    private void OnEnable()
    {
        returning = false;
        spawnTime = Time.time;
        lastPeriodProc = Time.time;
    }

    public void Animate(List<Vector3> points)
    {
        Animated = true;
        velocity = Vector2.zero;
        animationPoints = points;
    }

    private void RunAnimation()
    {
        Vector3 toTarget = (animationPoints[0] - transform.position);
        Vector3 desiredVec = toTarget.normalized * Mathf.Min(Time.deltaTime * InitialSpeed, toTarget.magnitude);
        transform.position += desiredVec;
        if (Vector3.Distance(animationPoints[0], transform.position) < SMALL_NUMBER)
        {
            if (animationPoints.Count == 1)
            {
                Animated = false;
                Weapon.OnAnimationDone?.Invoke(this);
            }
            else
            {
                animationPoints.RemoveAt(0);
            }
        }
    }

    void Update()
    {
        if (Animated)
        {
            RunAnimation();
            return;
        }
        GetComponentInChildren<Animator>()?.SetBool("IsRunning", velocity.magnitude > SMALL_NUMBER);


        if (Time.time - lastPeriodProc > Weapon.PeriodTime)
        {
            Weapon.OnPeriod?.Invoke(this);
            lastPeriodProc = Time.time;
        }

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
            if (!Temporary)
                Owner.PickUp(Weapon);
        }

        GetComponentInChildren<SkinnedMeshRenderer>().material = returning ? ReturnMat : ThrowMat;

        Accelerate();

        Move();
        RegulateMovementVolume();

        HitBoid();

        if (!GracePeriod && Vector2.Distance(Position2D, Owner.Position2D) < 1.5f)
        {
            Audioman.getInstance()?.PlaySound(Resources.Load<AudioOneShotClipConfiguration>("object/back_to_pouch"), this.transform.position);
            Destroy(gameObject);
            if (!Temporary)
                Owner.PickUp(Weapon);
        }
    }

    private void Accelerate()
    {
        Vector2 accDir = (Owner.Position2D - Position2D).normalized;

        float deltaTime = Time.deltaTime * (returning && timeStayed < StayTime ? Mathf.Lerp(StayEffect, EndStayEffect, timeStayed / StayTime) : 1f);

        velocity += accDir * ReturnAcceleration * deltaTime;
        ReturnAcceleration += ReturnJerk * deltaTime;

        if (!returning && Vector2.Dot(velocity, accDir) > 0 && velocity.magnitude < InitialSpeed)
        {
            returning = true;
            timeStayed = 0f;
            Weapon.OnApex?.Invoke(this);
        }

        if (returning)
        {
            timeStayed += Time.deltaTime;

            //float dot = (Vector2.Dot(velocity.normalized, accDir) + 1f) / 2f;
            //float damping = Mathf.Lerp(ReturnDrag * .5f, ReturnDrag * 2f, dot);
            //velocity *= Mathf.Pow(damping, deltaTime);

            if (Vector2.Dot(velocity, accDir) < 0)
            {
                velocity *= Mathf.Pow(ReturnDrag, Time.deltaTime);
            }
            else
            {
                Vector2 projection = accDir * Vector2.Dot(velocity, accDir);
                Vector2 reflection = velocity - projection;
                velocity = projection + reflection * Mathf.Pow(ReturnDrag, deltaTime);
            }
        }
    }

    private void Move()
    {
        Vector2 actual = new Vector2(transform.position.x, transform.position.z);
        Vector2 target = actual + velocity * Time.deltaTime;
        float y = transform.position.y;
        transform.forward = new Vector3(velocity.normalized.x, 0, velocity.normalized.y);

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
            Audioman.getInstance()?.PlaySound(Resources.Load<AudioOneShotClipConfiguration>("object/wall_bonk"), this.transform.position);
        }

        transform.position = new Vector3(actual.x, y, actual.y);
    }

    private void HitBoid()
    {
        if (Boids.Instance == null) return;

        List<Boid> boids = Boids.Instance.GetNearest(transform.position, 8);

        Vector2 thisPos = new Vector2(transform.position.x, transform.position.z);

        bool hitBoid = false;
        Vector3 hitPos = Vector3.zero;

        foreach (Boid b in boids)
        {
            if (b == null || b.IsDead)
                continue;

            if (IsInternalBoidCooldown(b))
            {
                continue;
            }

            Vector2 boidPos = new Vector2(b.position.x, b.position.z);
            if (Vector2.Distance(thisPos, boidPos) < 1.05f)
            {
                Boids.Instance.DamageBoid(b, Weapon.Damage);
                internalBoidCooldown[b] = Time.time;

                hitBoid = true;
                hitPos = b.position + Vector3.up * 0.5f;
            }
        }

        if (hitBoid)
        {
            Audioman.getInstance()?.PlaySound(Resources.Load<AudioOneShotClipConfiguration>("object/chomp"), this.transform.position);
            Instantiate(
                Resources.Load<GameObject>("Effects/BiteEffect"),
                hitPos,
                Quaternion.LookRotation(Camera.main.transform.forward *-1, Camera.main.transform.up)
                );

            Weapon.OnHit?.Invoke(this);

            if (Time.time - lastProcTime > InternalProcCooldown)
            {
                Weapon.OnProc?.Invoke(this);
                lastProcTime = Time.time;
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
        loopHolderSteps.setVolume((velocity.magnitude / InitialSpeed ) * 2f);
        loopHolderSteps.setWorldPosition(this.transform.position);


    }
}
