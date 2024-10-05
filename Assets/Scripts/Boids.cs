using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BoidType
{
    A, B, C
}
public struct Boid
{
    public Vector3 Position;
    public Vector3 Velocity;
    public BoidType Type;

    public override bool Equals(object obj)
    {
        if (!(obj is Boid)) return false;

        Boid boid = (Boid)obj;

        return boid.Position.Equals(Position) && boid.Velocity.Equals(Velocity) && boid.Type == Type;
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
                    Type = (BoidType)Mathf.FloorToInt(Random.value * BoidType.GetNames(typeof(BoidType)).Length)
                });

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = boids[i].Position;
            sphere.GetComponent<MeshRenderer>().material = BoidMaterials[(int)boids[i].Type];
            physicalBoids.Add(sphere);
        }
    }

    private void FlyTowardsCenter(ref Boid boid)
    {
        Vector3 centroid = Vector3.zero;
        int numBoids = 0;
        
        // Including self
        foreach (Boid b in boids)
        {
            if (Vector3.Distance(boid.Position, b.Position) < VisualRange)
            {
                float alignment = boid.Type == BoidType.C ? -1 : 0.5f;
                centroid += b.Position * (boid.Type == b.Type ? 1.0f : alignment);
                numBoids++;
            }
        }

        centroid /= numBoids;
        boid.Velocity += (centroid - boid.Position) * CenteringFactor * Time.deltaTime;
    }
    private void AvoidOthers(ref Boid boid)
    {
        Vector3 move = Vector3.zero;

        foreach (Boid b in boids)
        {
            if (!boid.Equals(b) && Vector3.Distance(boid.Position, b.Position) < AvoidDistance)
            {
                move += (boid.Position - b.Position);
            }
        }

        boid.Velocity += move * AvoidFactor * Time.deltaTime;
    }
    private void MatchVelocity(ref Boid boid)
    {
        Vector3 velocity = Vector3.zero;
        int numBoids = 0;

        // Including self
        foreach (Boid b in boids)
        {
            if (Vector3.Distance(boid.Position, b.Position) < VisualRange)
            {
                float alignment = boid.Type == BoidType.C ? -1 : 0.5f;
                velocity += b.Velocity * (boid.Type == b.Type ? 1.0f : alignment);
                numBoids++;
            }
        }

        velocity /= numBoids;
        boid.Velocity += (velocity - boid.Velocity) * MatchingFactor * Time.deltaTime;
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

        if (boid.Position.x < Margin.x)
        {
            boid.Velocity.x += TurnFactor * Time.deltaTime;
        }
        if (boid.Position.x > Space.x - Margin.x)
        {
            boid.Velocity.x -= TurnFactor * Time.deltaTime;
        }
        if (boid.Position.y < Margin.y)
        {
            boid.Velocity.y += TurnFactor * Time.deltaTime;
        }
        if (boid.Position.y > Space.y - Margin.y)
        {
            boid.Velocity.y -= TurnFactor * Time.deltaTime;
        }
        if (boid.Position.z < Margin.z)
        {
            boid.Velocity.z += TurnFactor * Time.deltaTime;
        }
        if (boid.Position.z > Space.z - Margin.z)
        {
            boid.Velocity.z -= TurnFactor * Time.deltaTime;
        }

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
        for (int i = 0; i < boids.Count; i++)
        {
            Boid boid = boids[i];

            FlyTowardsCenter(ref boid);
            AvoidOthers(ref boid);
            MatchVelocity(ref boid);
            boid.Velocity += Vector3.down * Gravity * Time.deltaTime;
            LimitSpeed(ref boid);
            KeepWithinBounds(ref boid);

            boid.Position += boid.Velocity * Time.deltaTime;
            physicalBoids[i].transform.position = boid.Position;
            boids[i] = boid;
        }
    }
}
