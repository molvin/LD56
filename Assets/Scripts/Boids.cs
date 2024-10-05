using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Boids : MonoBehaviour
{
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

    public int UpdateNumPerCycle => Mathf.Min(Mathf.Max(DesiredUpdateNumPerCycle, physicalBoids.Count / 10), physicalBoids.Count);
    public float DeltaTime => Time.fixedDeltaTime * physicalBoids.Count / UpdateNumPerCycle;

    private List<Rigidbody> physicalBoids = new();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Space * 0.5f, Space);
    }

    private void Start()
    {
        for (int i = 0; i < NumBoids; i++)
        {
            Vector3 position = new(Random.value * Space.x, Random.value * Space.y, Random.value * Space.z);
            Vector3 velocity = new(Random.value * MaxVelocity - MaxVelocity * 0.5f, Random.value * MaxVelocity - MaxVelocity * 0.5f, Random.value * MaxVelocity - MaxVelocity * 0.5f);

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = position;
            sphere.GetComponent<MeshRenderer>().material = BoidMaterials[Random.Range(0, BoidMaterials.Count)];
            Rigidbody rb = sphere.AddComponent<Rigidbody>();
            rb.velocity = velocity;
            physicalBoids.Add(rb);
        }
    }

    private void FlyTowardsCenter(Rigidbody boid, List<Rigidbody> neighbours = null)
    {
        if (neighbours == null) neighbours = physicalBoids;

        Vector3 centroid = Vector3.zero;
        int numBoids = 0;
        
        // Including self
        foreach (Rigidbody b in neighbours)
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
    private void AvoidOthers(Rigidbody boid, List<Rigidbody> neighbours = null)
    {
        if (neighbours == null) neighbours = physicalBoids;

        Vector3 move = Vector3.zero;

        foreach (Rigidbody b in neighbours)
        {
            if (!boid.Equals(b) && Vector3.Distance(boid.position, b.position) < AvoidDistance)
            {
                move += (boid.position - b.position);
            }
        }

        boid.velocity += move * (AvoidFactor * DeltaTime);
    }
    private void MatchVelocity(Rigidbody boid, List<Rigidbody> neighbours = null)
    {
        if (neighbours == null) neighbours = physicalBoids;

        Vector3 velocity = Vector3.zero;
        int numBoids = 0;

        // Including self
        foreach (Rigidbody b in neighbours)
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
    private void LimitSpeed(Rigidbody boid)
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
    private void KeepWithinBounds(Rigidbody boid)
    {
        Vector3 Margin = Space * BoundsMargin;
        float turnAmount = TurnFactor * DeltaTime;

        Vector3 deltaVelocity = Vector3.zero;

        if (boid.position.x < Margin.x)
        {
            deltaVelocity.x += turnAmount;
        }
        if (boid.position.x > Space.x - Margin.x)
        {
            deltaVelocity.x -= turnAmount;
        }
        if (boid.position.y < Margin.y)
        {
            deltaVelocity.y += turnAmount;
        }
        if (boid.position.y > Space.y - Margin.y)
        {
            deltaVelocity.y -= turnAmount;
        }
        if (boid.position.z < Margin.z)
        {
            deltaVelocity.z += turnAmount;
        }
        if (boid.position.z > Space.z - Margin.z)
        {
            deltaVelocity.z -= turnAmount;
        }
    }

    private void HandleCollision(Rigidbody boid)
    {
        Vector3 velocity = boid.velocity;

        if (boid.position.x < 0.0f && velocity.x < 0.0f || boid.position.x > Space.x && velocity.x > 0.0f)
        {
            float vel = Mathf.Abs(velocity.x);
            velocity.x = 0f;
            velocity += velocity.normalized * vel;
        }
        if (boid.position.y < 0.0f && velocity.y < 0.0f || boid.position.y > Space.y && velocity.y > 0.0f)
        {
            float vel = Mathf.Abs(velocity.y);
            velocity.y = 0f;
            velocity += velocity.normalized * vel;
        }
        if (boid.position.z < 0.0f && velocity.z < 0.0f || boid.position.z > Space.z && velocity.z > 0.0f)
        {
            float vel = Mathf.Abs(velocity.z);
            velocity.z = 0f;
            velocity += velocity.normalized * vel;
        }

        Vector3 pos = boid.position;
        pos.x = Mathf.Clamp(pos.x, 0.0f, Space.x);
        pos.y = Mathf.Clamp(pos.y, 0.0f, Space.y);
        pos.z = Mathf.Clamp(pos.z, 0.0f, Space.z);

        boid.position = pos;
        boid.velocity = velocity;
    }

    private void FixedUpdate()
    {
        KDTree<Rigidbody> tree = new(physicalBoids.Select(b => (b, b.position)).ToList());
        for (int i = 0; i < UpdateNumPerCycle; i++)
        {
            Rigidbody boid = physicalBoids[updateIndex];

            List<Rigidbody> neighbours = null;
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

            //physicalBoids[updateIndex].GetComponent<Rigidbody>().AddForce(boid.velocity * Time.fixedDeltaTime * (physicalBoids.Count / UpdateNumPerCycle), ForceMode.VelocityChange);

            updateIndex = (updateIndex + 1) % physicalBoids.Count;
        }

        for (int i = 0; i < physicalBoids.Count; i++)
        {
            Rigidbody boid = physicalBoids[i];

            HandleCollision(boid);

            //boid.Position += boid.Velocity * Time.fixedDeltaTime;
            //physicalBoids[i].transform.position = boid.Position;

        }
    }
}
