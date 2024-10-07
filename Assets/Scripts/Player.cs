using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    private enum State
    {
        Running,
        Dodging,
        Attacking,
        Dead,
        Shopping
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
    public float TimeBeforeFireFactor;
    public float AttackStunDuration;
    public float AttackStoppingTime;
    public float ThrowSpeed;
    public BoomerangController BoomerangPrefab;
    public Inventory Inventory;
    [Header("Leveling")]
    public int Level = 0;
    public int BaseKillsPerLevel = 16;
    public float LevelUpFactor;
    [Header("Death")]
    public float DeathStoppingTime;
    public float TimeBeforeFadeout;
    public HUD HUD;
    [Header("Shop")]
    public Shop ShopPrefab;
    public float ShopRespawnTime;
    public PlayerStats Stats;
    [Header("Collision")]
    public CapsuleCollider Collider;
    public LayerMask GroundLayer;
    public int CollisionSteps = 4;
    public LayerMask CollisionLayers;
    public float SkinWidth;
    public float Bounciness = 0.1f;
    public float PushingForce;
    public float PerBoidPush;
    public int MaxBoidPushers = 10;
    public LayerMask BoidLayer;
    public float BoidPushRadius;
    [Header("Animation")]
    public Animator Anim;
    public float BaseAnimSpeed = 1.0f;
    public float DodgeAnimSpeed = 3.0f;

    private Vector3 velocity;
    private Vector3 deceleration;
    private State state;
    private float timeOfLastDodge;
    private Vector3 dodgeDirection;
    private float timeOfLastShot;
    private Vector3 throwDir;
    private bool fired;
    private float timeOfDeath;
    private bool deathDone;
    private float timeOfLastShop;
    private int previousKillsForLevelUp = 0;
    private Shop shop;



    public Vector2 Position2D => new Vector2(transform.position.x, transform.position.z);

    private void Start()
    {
        state = State.Running;
        timeOfLastShop = Time.time;

        UpdateKills(0);

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
            case State.Dead:
                UpdateDead();
                break;
        }

        UpdateCollision();

        if(shop != null && state != State.Dead)
        {
            Vector3 shopPos = shop.transform.position;
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(shopPos);
            bool visible = (screenPoint.x > 0 && screenPoint.x < Screen.width && screenPoint.y > 0 && screenPoint.y < Screen.height);
            if(!visible)
            {
                shopPos.y = transform.position.y;
                Vector3 dir = shopPos - transform.position;

                float angle = Mathf.Atan(dir.x / dir.z) * 180f / Mathf.PI;
                if (dir.z < 0)
                {
                    if (dir.x >= 0)
                        angle += 180f;
                    else
                        angle -= 180f;
                }

                HUD.ShowShopIndicator(angle);
            }
            else
            {
                HUD.DisableShopIndicator();
            }
        }
        else
        {
            HUD.DisableShopIndicator();
        }

      
    }

    private void Run()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        Anim.SetBool("Running", state == State.Running && input.magnitude > 0.1f);
        bool sliding = state == State.Running && ((input.magnitude < 0.1f && velocity.magnitude > MaxSpeed * 0.3f) || (Vector3.Dot(input.normalized, velocity.normalized) < 0.0f));
        Anim.SetBool("Sliding", sliding);

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

        if (input.magnitude > 0.3f)
            transform.forward = input.normalized;

        if (Input.GetButtonDown("Jump"))
        {
            Dodge();
        }
        Attack();

        LookForShop();
    }

    private void Dodge()
    {
        if((Time.time - timeOfLastDodge) > DodgeCooldown)
        {
            timeOfLastDodge = Time.time;
            state = State.Dodging;
            Anim.SetTrigger("Dodge");
            Anim.speed = DodgeAnimSpeed;
        }
    }

    private void UpdateDodge()
    {
        float t = (Time.time - timeOfLastDodge) / DodgeDuration;
        float speed = DodgeSpeed * DodgeSpeedCurve.Evaluate(t);
        velocity = dodgeDirection * speed;

        transform.forward = dodgeDirection;

        if (t > 1.0f)
        {
            state = State.Running;
            Anim.speed = BaseAnimSpeed;
        }

    }

    private void Attack()
    {
        // TODO: controller support
        if (Input.GetMouseButtonDown(0) && (Time.time - timeOfLastShot) > AttackCooldown)
        {
            RaycastHit mouseHit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out mouseHit))
            {
                Vector3 toMouse = (mouseHit.point - transform.position);
                Vector2 throwDirection = new Vector2(toMouse.x, toMouse.z).normalized;
                throwDir = new Vector3(throwDirection.x, 0, throwDirection.y);

                if(Inventory.PeekNextWeapon() != null)
                {
                    timeOfLastShot = Time.time;
                    state = State.Attacking;
                    Anim.SetTrigger("Attack");
                }
            }
        }
    }

    private void UpdateAttacking()
    {
        velocity = Vector3.SmoothDamp(velocity, Vector3.zero, ref deceleration, AttackStoppingTime);

        transform.forward = throwDir;

        if(!fired && (Time.time - timeOfLastShot) >= (AttackStunDuration * TimeBeforeFireFactor))
        {
            Weapon weapon = Inventory.UseNextWeapon();

            BoomerangController boomerang = BoomerangController.New(BoomerangPrefab);
            boomerang.Init(this, weapon, transform.position + throwDir * 1.6f, new Vector2(throwDir.x, throwDir.z), new Vector2(velocity.x, velocity.z) * 0.5f);
            fired = true;
            Audioman.getInstance().PlaySound(Resources.Load<AudioOneShotClipConfiguration>("object/throw"), this.transform.position);
        }


        if ((Time.time - timeOfLastShot) > AttackStunDuration)
        {
            state = State.Running;
            fired = false;
        }
    }

    public void Die()
    {
        Anim.SetTrigger("Die");
        state = State.Dead;
        timeOfDeath = Time.time;
    }

    private void UpdateDead()
    {
        velocity = Vector3.SmoothDamp(velocity, Vector3.zero, ref deceleration, DeathStoppingTime);

        if(!deathDone && (Time.time - timeOfDeath) > TimeBeforeFadeout)
        {
            deathDone = true;
            HUD.GameOver();
        }
    }

    private void LookForShop()
    {
        if(shop != null)
        {
            float d = Vector3.Distance(transform.position, shop.transform.position);
            if(d < shop.Radius)
            {

                Stats.FullHeal();

                // NOTE: Upgrade shop every 2 levels
                HUD.Shop(Level / 2, Buy, shop);

                state = State.Shopping;
                Time.timeScale = 0.0f;
            }
        }
        else
        {
            
        }
    }

    private void Buy(Weapon weapon, Weapon weaponToReplace)
    {
        Time.timeScale = 1.0f;
        state = State.Running;
        if(weapon != null)
        {
            if(Inventory.AtMax)
                Inventory.ReplaceWeapon(weaponToReplace, weapon);
            else
                Inventory.AddNewWeapon(weapon);
        }

        Destroy(shop.gameObject);
        shop = null;
        timeOfLastShop = Time.time;
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

        if(state == State.Running || state == State.Attacking)
        {
            var boids = Physics.OverlapSphere(transform.position + Vector3.down * Collider.height / 2.0f, BoidPushRadius, BoidLayer);
            int count = 0;
            for (int i = 0; i < boids.Length && count < MaxBoidPushers; i++)
            {
                Vector3 myPos = transform.position;
                myPos.y = 0.0f;
                Vector3 boidPos = boids[i].transform.position;
                boidPos.y = 0.0f;

                Vector3 dir = (myPos - boidPos).normalized;
                velocity += dir * PerBoidPush * Time.deltaTime;
                count++;
            }
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

        // Nav Mesh Snapping
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit navHit, transform.localScale.y * 4f, NavMesh.AllAreas))
        {
            transform.position = navHit.position + Vector3.up * transform.localScale.y;
        }
    }

    public void PickUp(Weapon weapon)
    {
        Inventory.AddWeapon(weapon);
    }

    public void UpdateKills(int kills)
    {
        int killsForLevelUp = Mathf.RoundToInt(BaseKillsPerLevel * (1 + (Level * LevelUpFactor)));
        if (kills >= killsForLevelUp)
        {
            Level++;
            previousKillsForLevelUp = killsForLevelUp;
            killsForLevelUp = Mathf.RoundToInt(BaseKillsPerLevel + (1 + (Level * LevelUpFactor)));

            if (shop == null)
            {
                Vector3 point = Vector3.zero;
                var shopSpawns = FindObjectOfType<ShopSpawns>();
                if (shopSpawns)
                {
                    point = shopSpawns.GetRandomPoint(this);
                }
                bool hit = Physics.Raycast(point + Vector3.up * 1000.0f, Vector3.down, out RaycastHit hitInfo, 10000.0f, GroundLayer);
                if (hit)
                {
                    point = hitInfo.point;
                }
                shop = Instantiate(ShopPrefab, point, Quaternion.identity);
                // timeOfLastShop = Time.time;
            }
        }
        HUD.SetKills(kills, killsForLevelUp, previousKillsForLevelUp, Level);
    }
}
