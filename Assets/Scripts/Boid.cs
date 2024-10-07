using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Boid : MonoBehaviour
{
    [System.Serializable]
    public struct HealthMaterial
    {
        public Material Mat;
        public int MaxHealth;
    }

    public Rigidbody Rigidbody;
    public List<HealthMaterial> HealthMaterials;
    private new SkinnedMeshRenderer renderer;

    private int health = 10;
    public float damage = 1;

    public bool IsDead => health <= 0;

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

        foreach(HealthMaterial hpMat in HealthMaterials)
        {
            if(health > hpMat.MaxHealth)
                continue;

            renderer.material = hpMat.Mat;
            return;
        }

        renderer.material = HealthMaterials[HealthMaterials.Count - 1].Mat;
    }

    public void Update()
    {
        // TODO: Hit effect
        renderer.enabled = true;
        this.transform.forward = Rigidbody.velocity.normalized;
        GetComponentInChildren<Animator>()?.SetBool("IsRunning", Rigidbody.velocity.magnitude >= 0.001f);
        GetComponentInChildren<Animator>()?.SetFloat("RunSpeed", velocity.magnitude);

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

    public static Boid CreateBoid(Vector3 position, Vector3 velocity, int level)
    {
        Boid boid = GameObject.Instantiate(Resources.Load<Boid>("boid"));
        boid.transform.position = position;
        boid.renderer = boid.GetComponentInChildren<SkinnedMeshRenderer>();

        float multiplier = Mathf.Pow(1.15f, level);
        boid.health *= Mathf.RoundToInt(boid.health * multiplier * (1f + Mathf.Log(multiplier)) + level);
        boid.Radius = 0.5f * (1.0f + Mathf.Log(multiplier) * .8f);
        boid.damage = multiplier;

        boid.SetHealth(boid.health);

        boid.Rigidbody = boid.GetComponent<Rigidbody>();
        boid.Rigidbody.velocity = velocity;

        return boid;
    }
}
