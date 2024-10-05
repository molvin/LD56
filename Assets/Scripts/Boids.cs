using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;

public struct Boid
{
    public Vector3 Position;
    public Vector3 Velocity;

    public override bool Equals(object obj)
    {
        if (!(obj is Boid)) return false;

        Boid boid = (Boid)obj;

        return boid.Position.Equals(Position) && boid.Velocity.Equals(Velocity);
    }
}

public class Boids : MonoBehaviour
{
    public List<Material> BoidMaterials;
    public Vector3 Space = Vector3.one * 50;
    public int NumBoids = 200;
    public float VisualRange = 2f;
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
    public int UpdateNumPerCycle = 100;
    private int updateIndex = 0;

    private List<Boid> boids = new();
    private List<GameObject> physicalBoids = new();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Space * 0.5f, Space);
    }

    private void Start()
    {
        for (int i = 0; i < NumBoids; i++)
        {
            boids.Add(
                new()
                {
                    Position = new(Random.value * Space.x, Random.value * Space.y, Random.value * Space.z),
                    Velocity = new(Random.value * MaxVelocity - MaxVelocity * 0.5f, Random.value * MaxVelocity - MaxVelocity * 0.5f, Random.value * MaxVelocity - MaxVelocity * 0.5f),
                });

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = boids[i].Position;
            sphere.GetComponent<MeshRenderer>().material = BoidMaterials[Random.Range(0, BoidMaterials.Count)];
            physicalBoids.Add(sphere);
        }
    }

    private void FlyTowardsCenter(ref Boid boid, List<Boid> neighbours = null)
    {
        if (neighbours == null) neighbours = boids;

        Vector3 centroid = Vector3.zero;
        int numBoids = 0;
        
        // Including self
        foreach (Boid b in neighbours)
        {
            if (Vector3.Distance(boid.Position, b.Position) < VisualRange)
            {
                centroid += b.Position;
                numBoids++;
            }
        }

        centroid /= numBoids;
        boid.Velocity += (centroid - boid.Position) * (CenteringFactor * Time.deltaTime * (boids.Count / UpdateNumPerCycle));
    }
    private void AvoidOthers(ref Boid boid, List<Boid> neighbours = null)
    {
        if (neighbours == null) neighbours = boids;

        Vector3 move = Vector3.zero;

        foreach (Boid b in neighbours)
        {
            if (!boid.Equals(b) && Vector3.Distance(boid.Position, b.Position) < AvoidDistance)
            {
                move += (boid.Position - b.Position);
            }
        }

        boid.Velocity += move * (AvoidFactor * Time.deltaTime * (boids.Count / UpdateNumPerCycle));
    }
    private void MatchVelocity(ref Boid boid, List<Boid> neighbours = null)
    {
        if (neighbours == null) neighbours = boids;

        Vector3 velocity = Vector3.zero;
        int numBoids = 0;

        // Including self
        foreach (Boid b in neighbours)
        {
            if (Vector3.Distance(boid.Position, b.Position) < VisualRange)
            {
                velocity += b.Velocity;
                numBoids++;
            }
        }

        velocity /= numBoids;
        boid.Velocity += (velocity - boid.Velocity) * (MatchingFactor * Time.deltaTime * (boids.Count / UpdateNumPerCycle));
    }
    private void LimitSpeed(ref Boid boid)
    {
        if (boid.Velocity.magnitude > MaxVelocity)
        {
            boid.Velocity = boid.Velocity.normalized * MaxVelocity;
        }
        else if (boid.Velocity.magnitude < MaxVelocity * MinVelocityFactor)
        {
            boid.Velocity = boid.Velocity.normalized * MaxVelocity * MinVelocityFactor;
        }
    }
    private void KeepWithinBounds(ref Boid boid)
    {
        Vector3 Margin = Space * BoundsMargin;
        float turnAmount = TurnFactor * Time.deltaTime * (boids.Count / UpdateNumPerCycle);

        if (boid.Position.x < Margin.x)
        {
            boid.Velocity.x += turnAmount;
        }
        if (boid.Position.x > Space.x - Margin.x)
        {
            boid.Velocity.x -= turnAmount;
        }
        if (boid.Position.y < Margin.y)
        {
            boid.Velocity.y += turnAmount;
        }
        if (boid.Position.y > Space.y - Margin.y)
        {
            boid.Velocity.y -= turnAmount;
        }
        if (boid.Position.z < Margin.z)
        {
            boid.Velocity.z += turnAmount;
        }
        if (boid.Position.z > Space.z - Margin.z)
        {
            boid.Velocity.z -= turnAmount;
        }
    }

    private void HandleCollision(ref Boid boid)
    {
        if (boid.Position.x < 0.0f && boid.Velocity.x < 0.0f || boid.Position.x > Space.x && boid.Velocity.x > 0.0f)
        {
            float vel = Mathf.Abs(boid.Velocity.x);
            boid.Velocity.x = 0f;
            boid.Velocity += boid.Velocity.normalized * vel;
        }
        if (boid.Position.y < 0.0f && boid.Velocity.y < 0.0f || boid.Position.y > Space.y && boid.Velocity.y > 0.0f)
        {
            float vel = Mathf.Abs(boid.Velocity.y);
            boid.Velocity.y = 0f;
            boid.Velocity += boid.Velocity.normalized * vel;
        }
        if (boid.Position.z < 0.0f && boid.Velocity.z < 0.0f || boid.Position.z > Space.z && boid.Velocity.z > 0.0f)
        {
            float vel = Mathf.Abs(boid.Velocity.z);
            boid.Velocity.z = 0f;
            boid.Velocity += boid.Velocity.normalized * vel;
        }
    }

    private void Update()
    {
        KDTree<Boid> tree = new(boids.Select(b => (b, b.Position)).ToList());
        for (int i = 0; i < UpdateNumPerCycle; i++)
        {
            Boid boid = boids[updateIndex];
            List<Boid> neighbours = null;
            if (UseKDTree)
            {
                neighbours = tree.NearestNeighbours(boid.Position, 10);
            }

            FlyTowardsCenter(ref boid, neighbours);
            AvoidOthers(ref boid, neighbours);
            MatchVelocity(ref boid, neighbours);
            boid.Velocity += Vector3.down * (Gravity * Time.deltaTime * (boids.Count / UpdateNumPerCycle));
            LimitSpeed(ref boid);
            KeepWithinBounds(ref boid);

            boids[updateIndex] = boid;

            updateIndex = (updateIndex + 1) % boids.Count;
        }

        for (int i = 0; i < boids.Count; i++)
        {
            Boid boid = boids[i];

            HandleCollision(ref boid);

            boid.Position += boid.Velocity * Time.deltaTime;
            physicalBoids[i].transform.position = boid.Position;

            boids[i] = boid;
        }
    }
}
