using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    public Rigidbody Rigidbody;
    private new MeshRenderer renderer;

    private int health = 100;

    public bool IsDead => health <= 0;

    public Vector3 position 
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    public Vector3 velocity
    {
        get { return Rigidbody.velocity; }
        set { Rigidbody.velocity = value; }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        // TODO: Hit effect
        renderer.enabled = false;
    }
    
    public void Update()
    {
        // TODO: Hit effect
        renderer.enabled = true;
    }

    public static Boid CreateBoid(Vector3 position, Vector3 velocity, Material material)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = position;
        Boid boid = sphere.AddComponent<Boid>();

        boid.renderer = sphere.GetComponent<MeshRenderer>();
        boid.renderer.material = material;
        boid.Rigidbody = sphere.AddComponent<Rigidbody>();
        boid.Rigidbody.velocity = velocity;

        return boid;
    }
}
