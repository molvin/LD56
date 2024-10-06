using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float MaxHealth;
    public float CurrentHealth;

    public float BoidDamage = 1;
    public int MaxBoidDamageDealers = 10;
    public float BoidDamageRadius = 1.0f;
    public float BoidDamageCooldown = 0.5f;
    public HUD HUD;
    public Player Player;

    private float lastBoidDamage;

    private void Start()
    {
        CurrentHealth = MaxHealth;
        HUD.SetHealth(1.0f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            Player.Die();
        }
        if (CurrentHealth > 0)
            ApplyBoidDamage();
    }

    private void ApplyBoidDamage()
    {
        var boids = Boids.Instance.GetNearest(transform.position, MaxBoidDamageDealers);

        int count = 0;
        foreach(var boid in boids)
        {
            if (boid == null)
                continue;
            float dist = Vector3.Distance(boid.transform.position, transform.position);
            if (dist < BoidDamageRadius)
            {
                count++;
            }
        }

        if(count > 0 && (Time.time - lastBoidDamage) > BoidDamageCooldown)
        {
            lastBoidDamage = Time.time;
            TakeDamage(BoidDamage * count);
        }
    }

    public void TakeDamage(float dmg)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth - dmg, 0, MaxHealth);
        HUD.SetHealth(CurrentHealth / MaxHealth);

        if(CurrentHealth <= 0)
        {
            Player.Die();
        }
    }
}
