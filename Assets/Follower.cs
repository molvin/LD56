using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Follower : MonoBehaviour
{
    public HatSelector HatSelector;
    public Vector3 TargetPosition;
    public float Acceleration;
    public float MaxDistance;
    public float Friction;
    public Animator Anim;
    public float AnimationVelocityFactor;
    private Vector3 velocity;

    public Weapon Weapon { get; private set; }
    private Transform owner;
    private bool update;

    public void Init(Weapon weapon, Transform owner, bool update = true)
    {
        this.update = update;
        HatSelector.SetHat(weapon);
        Weapon = weapon;
        if(owner)
        {
            this.owner = owner;
            transform.position = owner.position;
        }
    }

    private void Update()
    {
        if (!update)
        {
            return;
        }

        Vector3 toTarget = TargetPosition - transform.position;
        float distance = toTarget.magnitude;
        velocity += toTarget.normalized * Acceleration * (distance / MaxDistance) * Time.deltaTime;
        velocity -= velocity * Friction * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;

        Anim.SetBool("IsRunning", velocity.magnitude >= 0.001f);
        Anim.SetFloat("RunSpeed", velocity.magnitude * AnimationVelocityFactor);
        this.transform.forward = velocity.normalized;
    }
}
