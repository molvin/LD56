using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Policy;
using UnityEngine;

public class Boid : MonoBehaviour
{
    [System.Serializable]
    public struct HealthMaterial
    {
        public Color Col;
        public int MaxHealth;
    }

    public Rigidbody Rigidbody;
    public List<HealthMaterial> HealthMaterials;
    public Color DeathColor;
    private new SkinnedMeshRenderer renderer;
    public Animator Anim;


    public const int BaseHealth = 10;
    private int health;
    public float damage = 1;
    public float SpeedModifier = 1f;
    public float DeathFriction = 10;
    public float AnimationVelocityFactor = 0.3f;

    public bool IsDead => health <= 0;
    public float DeathDuration = 1.0f;
    public AnimationCurve DeathSizeCurve;
    private float timeOfDeath;
    private float deathStartRadius;

    public float Radius
    {
        get { return transform.localScale.x * 0.5f; }
        set { transform.localScale = Vector3.one * value * 2f; }
    }

    public Vector2 Position2D => new Vector2(position.x, position.z);
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
        SetHealth(health - damage);
        // TODO: Hit effect
        renderer.enabled = false;
    }

    public void SetHealth(int health)
    {
        this.health = health;

        if(IsDead)
        {
            renderer.material.color = DeathColor;
            return;
        }

        foreach(HealthMaterial hpMat in HealthMaterials)
        {
            if(health > hpMat.MaxHealth)
                continue;

            renderer.material.color = hpMat.Col;
            return;
        }

        renderer.material.color = HealthMaterials[HealthMaterials.Count - 1].Col;
    }

    public void Update()
    {
        // TODO: Hit effect
        renderer.enabled = true;

        if (!IsDead)
        {
            Anim.SetBool("IsRunning", Rigidbody.velocity.magnitude >= 0.001f);
            Anim.SetFloat("RunSpeed", velocity.magnitude * AnimationVelocityFactor);
            this.transform.forward = Rigidbody.velocity.normalized;
        }
        else
        {
            Anim.SetBool("Dead", true);
            Rigidbody.velocity -= Rigidbody.velocity * DeathFriction * Time.deltaTime;

            float timeSinceDeath = Time.time - timeOfDeath;
            float t = timeSinceDeath / DeathDuration;
            Radius = Mathf.LerpUnclamped(0.0f, deathStartRadius, DeathSizeCurve.Evaluate(t));
            if (t >= 1.0f)
                Delete();
        }

    }
    public void Start()
    {
        StartCoroutine(LowChanceOfBulliBulli());
    }

    private IEnumerator LowChanceOfBulliBulli()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            if (UnityEngine.Random.Range(0, 100) == 1)
            {
                Audioman.getInstance()?.PlaySound(Resources.Load<AudioOneShotClipConfiguration>("object/bulli_bulli_dark"), this.transform.position);
            }
        }
    }

    private static Boid boidResource = null;

    public static Boid CreateBoid(Vector3 position, Vector3 velocity, int level)
    {
        if (boidResource == null)
        {
            boidResource = Resources.Load<Boid>("boid");
        }

        Boid boid = ObjectPool.Get(boidResource);
        boid.transform.position = position;
        boid.renderer = boid.GetComponentInChildren<SkinnedMeshRenderer>();

        float multiplier = Mathf.Pow(1.15f, level);
        boid.Radius = 0.5f * (1.0f + Mathf.Log(multiplier) * .8f);
        boid.SpeedModifier = 1.0f + Mathf.Log(multiplier) * .6f;
        boid.damage = multiplier;

        boid.SetHealth(Mathf.RoundToInt(BaseHealth * multiplier * (1f + Mathf.Log(multiplier)) + level));

        boid.Rigidbody = boid.GetComponent<Rigidbody>();
        boid.Rigidbody.velocity = velocity;

        return boid;
    }

    private void Delete()
    {
        ObjectPool.Return(this);
    }

    public void Die()
    {
        Anim.SetBool("Dead", true);
        timeOfDeath = Time.time;
        deathStartRadius = Radius;
    }
}
