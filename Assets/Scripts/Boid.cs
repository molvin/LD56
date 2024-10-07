using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    public Rigidbody Rigidbody;
    private new SkinnedMeshRenderer renderer;

    private int health = 10;

    public bool IsDead => health <= 0;


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
        health -= damage;
        // TODO: Hit effect
        renderer.enabled = false;
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
            if (UnityEngine.Random.Range(0 ,100) == 1)
            {
                Audioman.getInstance()?.PlaySound(Resources.Load<AudioOneShotClipConfiguration>("object/bulli_bulli_dark"), this.transform.position);
            }
        }
    }

    public static Boid CreateBoid(Vector3 position, Vector3 velocity, int level, Material material)
    {
        Boid boid = GameObject.Instantiate(Resources.Load<Boid>("boid"));
        boid.transform.position = position;

        boid.health = Mathf.RoundToInt(boid.health * Mathf.Pow(2, level));

        boid.renderer = boid.GetComponentInChildren<SkinnedMeshRenderer>();
        boid.renderer.material = material;
        boid.Rigidbody = boid.GetComponent<Rigidbody>();
        boid.Rigidbody.velocity = velocity;

        return boid;
    }
}
