using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    public Rigidbody Rigidbody;

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

    public static Boid CreateBoid(Vector3 position, Vector3 velocity, Material material)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = position;
        sphere.GetComponent<MeshRenderer>().material = material;
        Boid boid = sphere.AddComponent<Boid>();
        boid.Rigidbody = sphere.AddComponent<Rigidbody>();
        boid.Rigidbody.velocity = velocity;

        return boid;
    }
}
