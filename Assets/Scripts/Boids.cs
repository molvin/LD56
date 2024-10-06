using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Boids : MonoBehaviour
{
    public static Boids Instance;

    public List<Material> BoidMaterials;
    public Vector3 Space = Vector3.one * 50;
    public int NumBoids = 1000;
    public float VisualRange = 3f;
    public float MaxVelocity = 6f;
    public float MinVelocityFactor = 0.4f;
    public float CenteringFactor = 0.2f;
    public float AvoidDistance = 1.6f;
    public float AvoidFactor = 5f;
    public float MatchingFactor = 0.7f;
    public float BoundsMargin = 0.3f;
    public float TurnFactor = 1f;
    public float Gravity = 0.7f;

    public bool UseKDTree = true;
    public int DesiredUpdateNumPerCycle = 100;
    private int updateIndex = 0;

    public int UpdateNumPerCycle => Mathf.Min(Mathf.Max(DesiredUpdateNumPerCycle, allBoids.Count / 10), allBoids.Count);
    public float DeltaTime => Time.fixedDeltaTime * allBoids.Count / UpdateNumPerCycle;

    private List<Boid> allBoids = new();
    private KDTree<Boid> tree;

    private Vector3 MinBounds;
    private Vector3 MaxBounds;

    public List<Boid> GetNearest(Vector3 pos, int num)
    {
        if (tree == null)
            return new();

        return tree.NearestNeighbours(pos, num);
    }

    public void DamageBoid(Boid boid, int damage)
    {
        boid.TakeDamage(damage);

        if (boid.IsDead)
        {
            allBoids.Remove(boid);
            Destroy(boid.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Space);
    }

    private void Start()
    {
        Instance = this;

        MinBounds = transform.position - Space * 0.5f;
        MaxBounds = transform.position + Space * 0.5f;

        for (int i = 0; i < NumBoids; i++)
        {
            Vector3 position = MinBounds + new Vector3(Random.value * Space.x, Random.value * Space.y, Random.value * Space.z);
            Vector3 velocity = new(Random.value * MaxVelocity - MaxVelocity * 0.5f, Random.value * MaxVelocity - MaxVelocity * 0.5f, Random.value * MaxVelocity - MaxVelocity * 0.5f);

            Boid boid = Boid.CreateBoid(position, velocity, BoidMaterials[Random.Range(0, BoidMaterials.Count)]);
            allBoids.Add(boid);
        }
    }

    private void FlyTowardsCenter(Boid boid, List<Boid> neighbours = null)
    {
        if (neighbours == null) neighbours = allBoids;

        Vector3 centroid = Vector3.zero;
        int numBoids = 0;
        
        // Including self
        foreach (Boid b in neighbours)
        {
            if (Vector3.Distance(boid.position, b.position) < VisualRange)
            {
                centroid += b.position;
                numBoids++;
            }
        }

        centroid /= numBoids;
        boid.velocity += (centroid - boid.position) * (CenteringFactor * DeltaTime);
    }
    private void AvoidOthers(Boid boid, List<Boid> neighbours = null)
    {
        if (neighbours == null) neighbours = allBoids;

        Vector3 move = Vector3.zero;

        foreach (Boid b in neighbours)
        {
            if (!boid.Equals(b) && Vector3.Distance(boid.position, b.position) < AvoidDistance)
            {
                move += (boid.position - b.position);
            }
        }

        boid.velocity += move * (AvoidFactor * DeltaTime);
    }
    private void MatchVelocity(Boid boid, List<Boid> neighbours = null)
    {
        if (neighbours == null) neighbours = allBoids;

        Vector3 velocity = Vector3.zero;
        int numBoids = 0;

        // Including self
        foreach (Boid b in neighbours)
        {
            if (Vector3.Distance(boid.position, b.position) < VisualRange)
            {
                velocity += b.velocity;
                numBoids++;
            }
        }

        velocity /= numBoids;
        boid.velocity += (velocity - boid.velocity) * (MatchingFactor * DeltaTime);
    }
    private void LimitSpeed(Boid boid)
    {
        if (boid.velocity.magnitude > MaxVelocity)
        {
            boid.velocity = boid.velocity.normalized * MaxVelocity;
        }
        else if (boid.velocity.magnitude < MaxVelocity * MinVelocityFactor)
        {
            boid.velocity = boid.velocity.normalized * MaxVelocity * MinVelocityFactor;
        }
    }
    private void KeepWithinBounds(Boid boid)
    {
        Vector3 Margin = Space * BoundsMargin;
        float turnAmount = TurnFactor * DeltaTime;

        Vector3 deltaVelocity = Vector3.zero;

        if (boid.position.x < MinBounds.x + Margin.x)
        {
            deltaVelocity.x += turnAmount;
        }
        if (boid.position.x > MaxBounds.x - Margin.x)
        {
            deltaVelocity.x -= turnAmount;
        }
        if (boid.position.y < MinBounds.y + Margin.y)
        {
            deltaVelocity.y += turnAmount;
        }
        if (boid.position.y > MaxBounds.y - Margin.y)
        {
            deltaVelocity.y -= turnAmount;
        }
        if (boid.position.z < MinBounds.z + Margin.z)
        {
            deltaVelocity.z += turnAmount;
        }
        if (boid.position.z > MaxBounds.z - Margin.z)
        {
            deltaVelocity.z -= turnAmount;
        }

        boid.velocity += deltaVelocity;
    }

    private void HandleCollision(Boid boid)
    {
        Vector3 velocity = boid.velocity;

        if (boid.position.x < MinBounds.x && velocity.x < 0.0f || boid.position.x > MaxBounds.x && velocity.x > 0.0f)
        {
            float vel = Mathf.Abs(velocity.x);
            velocity.x = 0f;
            velocity += velocity.normalized * vel;
        }
        if (boid.position.y < MinBounds.y && velocity.y < 0.0f || boid.position.y > MaxBounds.y && velocity.y > 0.0f)
        {
            float vel = Mathf.Abs(velocity.y);
            velocity.y = 0f;
            velocity += velocity.normalized * vel;
        }
        if (boid.position.z < MinBounds.z && velocity.z < 0.0f || boid.position.z > MaxBounds.z && velocity.z > 0.0f)
        {
            float vel = Mathf.Abs(velocity.z);
            velocity.z = 0f;
            velocity += velocity.normalized * vel;
        }

        Vector3 pos = boid.position;
        pos.x = Mathf.Clamp(pos.x, MinBounds.x, MaxBounds.x);
        pos.y = Mathf.Clamp(pos.y, MinBounds.y, MaxBounds.y);
        pos.z = Mathf.Clamp(pos.z, MinBounds.z, MaxBounds.z);

        boid.position = pos;
        boid.velocity = velocity;
    }

    private void FixedUpdate()
    {
        tree = new(allBoids.Select(b => (b, b.position)).ToList());

        updateIndex %= allBoids.Count;
        for (int i = 0; i < UpdateNumPerCycle; i++)
        {
            Boid boid = allBoids[updateIndex];

            List<Boid> neighbours = null;
            if (UseKDTree)
            {
                neighbours = tree.NearestNeighbours(boid.position, 10);
            }

            FlyTowardsCenter(boid, neighbours);
            AvoidOthers(boid, neighbours);
            MatchVelocity(boid, neighbours);
            //boid.Velocity += Vector3.down * (Gravity * Time.fixedDeltaTime * (boids.Count / UpdateNumPerCycle));
            // Rigidbody gravity
            boid.velocity += Vector3.down * ((Gravity - 9.81f) * DeltaTime);
            LimitSpeed(boid);
            KeepWithinBounds(boid);

            //allBoids[updateIndex].GetComponent<Rigidbody>().AddForce(boid.velocity * Time.fixedDeltaTime * (allBoids.Count / UpdateNumPerCycle), ForceMode.VelocityChange);

            updateIndex = (updateIndex + 1) % allBoids.Count;
        }

        for (int i = 0; i < allBoids.Count; i++)
        {
            Boid boid = allBoids[i];

            HandleCollision(boid);

            //boid.Position += boid.Velocity * Time.fixedDeltaTime;
            //allBoids[i].transform.position = boid.Position;

        }
    }
}
