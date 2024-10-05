using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Rigidbody Rb;
    public float Speed;

    public void Init(Vector3 velocity)
    {
        Rb.velocity = velocity + velocity.normalized * Speed;    
    }
}
